using System.Security.Cryptography;
using OpenGDS2.Domain;

namespace OpenGDS2.Import;

// Extract text only to the user's local cache. Never executes or ships the PE.
public static class OriginalDialogueImporter
{
    public static OriginalDialogueCatalog Extract(byte[] data)
    {
        if (!Convert.ToHexString(SHA256.HashData(data)).Equals(EmployeeDatabaseImporter.ExpectedHash,
            StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unsupported dialogue source.");
        var pe = new EmployeeDatabaseImporter.Image(data);
        // fGMSKoyou: floor(Rnd * 10), after employee copy; ten greeting arms.
        pe.Require(pe.Offset(0x99FFC8 + 0xE4),0x27,0x6C,0xFF,0x0A,0x0B,0,4,0,0xF4,0x0A,0xEB,0xB3,0xFB,0xE6);
        var result = new OriginalDialogueCatalog {SchemaVersion=3,SourceHash=EmployeeDatabaseImporter.ExpectedHash};
        const int pool = 0x60C264;
        for (int variant = 0; variant < 9; variant++)
            result.Entries.Add($"HIRE_GREETING_{variant}",pe.String(pool,0x11+variant));
        result.Entries.Add("HIRE_GREETING_9",pe.String(pool,0x1A)+"{company}"+pe.String(pool,0x1C));
        result.Entries.Add("NEW_EMPLOYEE",pe.String(pool,0x10));
        const int meetingPool=0x5FAFCC;
        result.Entries.Add("MEETING_SECRETARY",pe.String(meetingPool,0x21));
        result.Entries.Add("MEETING_INTRO",pe.String(meetingPool,0x23)+"{hardware}"+pe.String(meetingPool,0x24)+"{audience}"+pe.String(meetingPool,0x25));
        for (int line=0;line<27;line++) result.Entries.Add($"MEETING_LINE_{line}",pe.String(meetingPool,0x28+line));
        result.Entries.Add("MANGA_REPRESENTATIVE","{publisher}"+pe.String(meetingPool,0x130));
        result.Entries.Add("MANGA_MEETING_0",pe.String(meetingPool,0x78)+"{publisher}"+pe.String(meetingPool,0x131));
        result.Entries.Add("MANGA_MEETING_1",pe.String(meetingPool,0x132)+"{title}"+pe.String(meetingPool,0x134));
        result.Entries.Add("MANGA_MEETING_2",pe.String(meetingPool,0x135));
        result.Entries.Add("MANGA_MEETING_3",pe.String(meetingPool,0x136));
        result.Entries.Add("MANGA_RIGHTS_QUESTION","{title}"+pe.String(meetingPool,0x137)+"{fee}"+pe.String(meetingPool,0x84));
        result.Entries.Add("MANGA_NO_MONEY",pe.String(meetingPool,0x88));
        result.Entries.Add("MANGA_RIGHTS_APPROVED",pe.String(meetingPool,0x139));
        result.Entries.Add("MANGA_BEGIN_DEVELOPMENT",pe.String(meetingPool,0x13A));
        result.Entries.Add("MANGA_RIGHTS_DECLINED",pe.String(meetingPool,0x8B));
        const int workPool=0x5F99AC,shipmentPool=0x60736C;
        result.Entries.Add("CORE_SECRETARY",pe.String(workPool,0x77));
        result.Entries.Add("CHECKPOINT_TYPE_INTRO",pe.String(workPool,0xC4));
        result.Entries.Add("CHECKPOINT_GENRE_INTRO",pe.String(workPool,0xCC));
        result.Entries.Add("CHECKPOINT_COMPLETE_INTRO",pe.String(workPool,0xA1)+"{room}"+pe.String(workPool,0xCD));
        int[] shipmentEntries=[0x41,0x42,0x43,0x44,0x46,0x47,0x48,0x49,0x4B,0x4A,0x4C,0x4D,0x4E,0x53,0x54,0x55,0x56,0x57,0x6D];
        foreach (var message in Enum.GetValues<ShipmentMessage>())
            result.Entries.Add(ShipmentDialogueRules.MessageKey(message),pe.String(shipmentPool,shipmentEntries[(int)message]));
        // Serialized fGMKaiwa Timer1 interval property: 20 milliseconds.
        pe.Require(0x248EAC,6,0,(byte)'T',(byte)'i',(byte)'m',(byte)'e',(byte)'r',(byte)'1',0,0x0B,3,20,0,0,0);
        return result;
    }
}
