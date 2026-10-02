using Godot;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using OpenGDS2.Domain;

namespace OpenGDS2.Import;

public sealed class OriginalGameImporter
{
    public const string CacheRoot = "user://original";
    private static readonly string[] AllowedExtensions = [".bmp", ".wav", ".gds"];

    public sealed record Result(int FilesImported, string CachePath);

    public OriginalCatalog? LoadCatalog()
    {
        string path = ProjectSettings.GlobalizePath(CacheRoot + "/catalog.json");
        if (!File.Exists(path)) return null;
        var catalog = JsonSerializer.Deserialize<OriginalCatalog>(File.ReadAllText(path));
        if (catalog is null || catalog.SchemaVersion != 1 || catalog.SourceHash != EmployeeDatabaseImporter.ExpectedHash)
            throw new InvalidDataException("Unsupported local catalog.");
        return catalog;
    }

    public Result ImportArchive(string sourceLzh)
    {
        string source = ProjectSettings.GlobalizePath(sourceLzh);
        if (!File.Exists(source)) throw new FileNotFoundException("Original archive not found.", source);

        var archive = new LhaArchive(source);
        if (!archive.Entries.Any(e => e.Name.Equals("GameDev2DX.exe", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("This does not look like the expected GameDev2DX archive.");
        if (!archive.Entries.Any(e => e.Name.EndsWith("Graphic/Title.bmp", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("Expected Graphic/Title.bmp is missing.");

        // Validate and extract rules before replacing any previous cache.
        var executable = archive.Entries.Single(e => e.Name.Equals("GameDev2DX.exe", StringComparison.OrdinalIgnoreCase));
        var bytes = archive.Extract(executable);
        var database = EmployeeDatabaseImporter.Extract(bytes);
        var catalog = OriginalCatalogImporter.Extract(bytes);
        string cache = ProjectSettings.GlobalizePath(CacheRoot);
        string staging = cache + ".staging-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(staging);
        try
        {
            int count = 0;
            foreach (var entry in archive.Entries)
            {
                if (entry.IsDirectory) continue;
                string ext = Path.GetExtension(entry.Name);
                if (!AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) continue;

                string safeRelative = Sanitize(entry.Name);
                string destination = Path.Combine(staging, safeRelative.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.WriteAllBytes(destination, archive.Extract(entry));
                count++;
            }
            File.WriteAllText(Path.Combine(staging, "employees.json"), JsonSerializer.Serialize(database));
            File.WriteAllText(Path.Combine(staging, "catalog.json"), JsonSerializer.Serialize(catalog));
            File.WriteAllText(Path.Combine(staging, ".imported"), $"OpenGDS2 original asset import\nfiles={count}\n");
            string backup = cache + ".previous";
            if (Directory.Exists(backup)) Directory.Delete(backup, true);
            if (Directory.Exists(cache)) Directory.Move(cache, backup);
            try { Directory.Move(staging, cache); }
            catch { if (Directory.Exists(backup)) Directory.Move(backup, cache); throw; }
            if (Directory.Exists(backup)) Directory.Delete(backup, true);
            return new Result(count, cache);
        }
        finally { if (Directory.Exists(staging)) Directory.Delete(staging, true); }
    }

    public bool HasImportedAssets()
    {
        string cache = ProjectSettings.GlobalizePath(CacheRoot);
        return File.Exists(Path.Combine(cache, "employees.json")) && File.Exists(Path.Combine(cache, ".imported")) && File.Exists(Path.Combine(cache, "Graphic", "Title.bmp"));
    }

    public EmployeeDatabase? LoadEmployeeDatabase()
    {
        string path = Path.Combine(ProjectSettings.GlobalizePath(CacheRoot), "employees.json");
        if (!File.Exists(path)) return null;
        var data = JsonSerializer.Deserialize<EmployeeDatabase>(File.ReadAllText(path))
            ?? throw new InvalidDataException("Empty employee database.");
        if (data.SchemaVersion != 1 || data.SourceHash != EmployeeDatabaseImporter.ExpectedHash ||
            data.Employees is null || data.NameComponents is null || data.Employees.Count != 60 ||
            data.MinimumSourceIndex != 35 || data.SourceUpperBound != 60 || data.NameComponents.Count != 8 ||
            data.Employees.Any(x => x is null || x.RawAttributes is null || x.RawAttributes.Count != 7 || x.ContractFee < 0) ||
            !data.Employees.Select(x => x.SourceIndex).SequenceEqual(Enumerable.Range(0, 60)))
            throw new InvalidDataException("Unsupported or corrupt employee database. Reimport your original archive.");
        return data;
    }

    private static string Sanitize(string path)
    {
        string normalized = path.Replace('\\', '/');
        if (Path.IsPathRooted(normalized) || normalized.Contains(':') || normalized.Contains('\0') ||
            normalized.Split('/').Any(p => p is ".." or "." or ""))
            throw new InvalidDataException("Unsafe archive path.");
        return normalized;
    }
}
