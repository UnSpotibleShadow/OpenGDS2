using OpenGDS2.Domain;
namespace OpenGDS2.Persistence;
public sealed record SaveSlotPreview(int Slot,string Path,GameState? State,string? Error,bool BackupAvailable);
public static class SaveSlotStore
{
    public static string PathFor(string directory,int slot)
    {if(slot is <1 or >9999)throw new ArgumentOutOfRangeException(nameof(slot));return Path.Combine(directory,$"slot{slot}.json");}
    public static IReadOnlyList<SaveSlotPreview> Read(string directory,bool includeEmpty)
    {
        var slots=new SortedSet<int>();
        if(Directory.Exists(directory))foreach(var file in Directory.EnumerateFiles(directory,"slot*.json"))
        {var name=Path.GetFileNameWithoutExtension(file);if(int.TryParse(name.AsSpan(4),out int slot)&&slot is >=1 and <=9999)slots.Add(slot);}
        if(includeEmpty){int free=1;while(slots.Contains(free)&&free<9999)free++;slots.Add(free);}
        var result=new List<SaveSlotPreview>();
        foreach(int slot in slots)
        {
            string path=PathFor(directory,slot);GameState? state=null;string? error=null;
            if(File.Exists(path))try{state=SaveStore.Load(path);}catch(Exception ex)when(ex is IOException or InvalidDataException or UnauthorizedAccessException){error=ex.Message;}
            result.Add(new(slot,path,state,error,File.Exists(SaveStore.BackupPath(path))));
        }
        return result;
    }
}
