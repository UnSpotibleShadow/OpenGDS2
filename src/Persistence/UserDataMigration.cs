using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace OpenGDS2.Persistence;

public sealed record UserDataMigrationResult(bool Succeeded,int CopiedFiles=0,int ExistingFiles=0,string? Error=null);

/// <summary>One-time, non-destructive copy of private-test data into the stable app directory.</summary>
public static class UserDataMigration
{
    public const string DirectoryName="OpenGDS2";
    public const string LegacyDirectoryName="OpenGDS2-EarlyAccessTesting";
    public const string CompletionFile=".legacy-data-migration-v1.complete";
    private const string CompletionText="OpenGDS2 legacy data migration v1 complete\n";
    private static readonly string[] Folders=["original","saves","music","asset-overrides"];
    private static readonly string[] Files=["interface.cfg","audio.cfg","rankings.json","employee-name-readings.json"];

    public static UserDataMigrationResult Run(string destination)
    {
        int copied=0,existing=0;
        try
        {
            destination=Path.TrimEndingDirectorySeparator(Path.GetFullPath(destination));
            // Custom test/development destinations must not import neighboring user data.
            if(Path.GetFileName(destination)!=DirectoryName)return new(true);
            Directory.CreateDirectory(destination);RejectLink(destination);
            string lockPath=Path.Combine(destination,".legacy-data-migration-v1.lock");
            if(File.Exists(lockPath))RejectLink(lockPath);
            using var migrationLock=new FileStream(lockPath,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            string marker=Path.Combine(destination,CompletionFile);
            if(File.Exists(marker))
            {
                RejectLink(marker);
                if(File.ReadAllText(marker)!=CompletionText)throw new IOException("The migration completion record is invalid.");
                return new(true);
            }
            string source=Path.Combine(Path.GetDirectoryName(destination)!,LegacyDirectoryName);
            if(Directory.Exists(source))
            {
                RejectLink(source);
                foreach(string name in Files)
                {
                    string path=Path.Combine(source,name);
                    if(File.Exists(path))CopyFile(path,Path.Combine(destination,name));
                }
                foreach(string name in Folders)
                {
                    string path=Path.Combine(source,name);
                    if(Directory.Exists(path))CopyDirectory(path,Path.Combine(destination,name),name=="saves");
                }
            }
            // Mark fresh installations too: a later old-version launch must not
            // silently inject legacy progress into an already established profile.
            Publish(marker,stream=>
            {
                using var writer=new StreamWriter(stream,leaveOpen:true);writer.Write(CompletionText);writer.Flush();
            });
            return new(true,copied,existing);

            void CopyDirectory(string from,string to,bool saveRoot=false)
            {
                RejectLink(from);Directory.CreateDirectory(to);RejectLink(to);
                foreach(string path in Directory.EnumerateFileSystemEntries(from).Order(StringComparer.Ordinal))
                {
                    RejectLink(path);string name=Path.GetFileName(path),target=Path.Combine(to,name);
                    if(Directory.Exists(path))CopyDirectory(path,target);
                    else if(saveRoot&&Regex.IsMatch(name,@"^slot[1-9][0-9]*\.json(?:\.bak)?$")&&SavePairConflicts(from,to,name))existing++;
                    else CopyFile(path,target);
                }
            }
            void CopyFile(string from,string to)
            {
                RejectLink(from);
                if(File.Exists(to)){RejectLink(to);existing++;return;}
                // Open without write/delete sharing. Native Windows rejects a
                // concurrently written source instead of copying a torn save.
                using var input=new FileStream(from,FileMode.Open,FileAccess.Read,FileShare.Read);
                if(Publish(to,output=>input.CopyTo(output)))copied++;else existing++;
            }
        }
        catch(Exception error) when(error is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {return new(false,copied,existing,error.Message);}
    }

    private static bool SavePairConflicts(string from,string to,string name)
    {
        string main=name.EndsWith(".bak",StringComparison.Ordinal)?name[..^4]:name;
        foreach(string member in new[]{main,main+".bak"})
        {
            string target=Path.Combine(to,member),source=Path.Combine(from,member);
            if(!File.Exists(target))continue;
            RejectLink(target);
            if(!File.Exists(source))return true;
            RejectLink(source);
            using var a=new FileStream(source,FileMode.Open,FileAccess.Read,FileShare.Read);
            using var b=new FileStream(target,FileMode.Open,FileAccess.Read,FileShare.Read);
            if(a.Length!=b.Length||!SHA256.HashData(a).SequenceEqual(SHA256.HashData(b)))return true;
        }
        return false;
    }

    private static void RejectLink(string path)
    {
        if((File.GetAttributes(path)&FileAttributes.ReparsePoint)!=0)
            throw new IOException("Automatic migration does not follow symbolic links or junctions: "+path);
    }

    private static bool Publish(string target,Action<FileStream> write)
    {
        string temporary=Path.Combine(Path.GetDirectoryName(target)!,".migration-"+Guid.NewGuid().ToString("N")+".tmp");
        try
        {
            using(var output=new FileStream(temporary,FileMode.CreateNew,FileAccess.Write,FileShare.None))
            {write(output);output.Flush(true);}
            try{File.Move(temporary,target,false);return true;}
            catch(IOException) when(File.Exists(target)){RejectLink(target);return false;}
        }
        finally{if(File.Exists(temporary))File.Delete(temporary);}
    }
}
