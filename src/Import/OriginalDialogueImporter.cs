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
        var result = new OriginalDialogueCatalog {SchemaVersion=7,SourceHash=EmployeeDatabaseImporter.ExpectedHash};
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
        const int alphaPool=0x605270,betaPool=0x604840;
        void Meeting(CheckpointMeetingMessage message,string text)=>result.Entries.Add(CheckpointMeetingRules.MessageKey(message),text);
        result.Entries.Add("CORE_EMPLOYEE",pe.String(alphaPool,0x12));
        result.Entries.Add("CHECKPOINT_ALPHA_MEETING",pe.String(meetingPool,0x0F));
        result.Entries.Add("CHECKPOINT_BETA_MEETING",pe.String(meetingPool,0x10));
        Meeting(CheckpointMeetingMessage.TypeSuggestion,pe.String(alphaPool,0x44)+"{type}"+pe.String(alphaPool,0x45));
        Meeting(CheckpointMeetingMessage.TypeFits,pe.String(alphaPool,0x46)+"{type}"+pe.String(alphaPool,0x47));
        Meeting(CheckpointMeetingMessage.TypeHit,pe.String(alphaPool,0x48)+"{type}"+pe.String(alphaPool,0x49));
        Meeting(CheckpointMeetingMessage.TypeAudience,"{audience}"+pe.String(alphaPool,0x4B)+"{type}"+pe.String(alphaPool,0x4C));
        Meeting(CheckpointMeetingMessage.TypeReaders,pe.String(alphaPool,0x46)+"{type}"+pe.String(alphaPool,0x4D)+"{audience}"+pe.String(alphaPool,0x4E));
        Meeting(CheckpointMeetingMessage.TypeChosen,pe.String(alphaPool,0x2B)+"{type}"+pe.String(alphaPool,0x2D));
        Meeting(CheckpointMeetingMessage.Publicity,pe.String(alphaPool,0x2E)+"{recognition}"+pe.String(alphaPool,0x31));
        Meeting(CheckpointMeetingMessage.BetaComplete,pe.String(betaPool,0x43));
        Meeting(CheckpointMeetingMessage.RecognitionLow,pe.String(betaPool,0x45));
        Meeting(CheckpointMeetingMessage.RecognitionMedium,pe.String(betaPool,0x46));
        Meeting(CheckpointMeetingMessage.RecognitionHigh,pe.String(betaPool,0x47));
        Meeting(CheckpointMeetingMessage.ChooseGenre,pe.String(betaPool,0x48));
        Meeting(CheckpointMeetingMessage.TypeReminder,pe.String(betaPool,0x49)+"{type}"+pe.String(betaPool,0x4A));
        Meeting(CheckpointMeetingMessage.GenreMatch,pe.String(betaPool,0x2F)+"{genre}"+pe.String(betaPool,0x31)+"{type}"+pe.String(betaPool,0x33));
        Meeting(CheckpointMeetingMessage.GenreChosen,pe.String(betaPool,0x2F)+"{genre}"+pe.String(betaPool,0x31)+"{type}"+pe.String(betaPool,0x36));
        Meeting(CheckpointMeetingMessage.FunBonus,pe.String(betaPool,0x34));
        foreach (var message in Enum.GetValues<SecretaryAdviceMessage>())
            result.Entries.Add(SecretaryAdviceRules.MessageKey(message),pe.String(0x60DF28,0x5B+(int)message));
        result.Entries.Add("SECRETARY_NAMED",pe.String(0x60DF28,0xB8)+"{name}"+pe.String(0x60DF28,0xB7));
        result.Entries.Add("SECRETARY_FAREWELL",pe.String(0x603E1C,0x0B));
        result.Entries.Add("SECRETARY_GREETING",pe.String(0x603E1C,0x0F)+"{name}"+pe.String(0x603E1C,0x10));
        const int calendarPool=0x60DF28,graduatePool=0x5F7714;
        void Calendar(string key,string text)=>result.Entries.Add("CALENDAR_DIALOGUE_"+key,text);
        Calendar("ContractRenewal",pe.String(calendarPool,0x91));
        Calendar("Awards",pe.String(calendarPool,0x95));
        Calendar("HardwareDevelopment","{maker}"+pe.String(calendarPool,0x9A));
        Calendar("HardwareSoon","{maker}"+pe.String(calendarPool,0x9B));
        Calendar("HardwareRetiring","{maker}"+pe.String(calendarPool,0x9F)+"{hardware}"+pe.String(calendarPool,0xA0));
        Calendar("HardwareRetiringExplanation",pe.String(calendarPool,0xA1));
        Calendar("HardwareRetired","{maker}"+pe.String(calendarPool,0x9F)+"{hardware}"+pe.String(calendarPool,0xA2));
        Calendar("HardwareRetiredExplanation",pe.String(calendarPool,0xA3));
        Calendar("ExhibitionAutumn",pe.String(calendarPool,0xA5));
        Calendar("ExhibitionSpring",pe.String(calendarPool,0xA9));
        Calendar("LoanRepayment",pe.String(calendarPool,0xAB)+"{principal}"+pe.String(calendarPool,0xAC)+"{repayment}"+pe.String(calendarPool,0xAD));
        Calendar("Graduates",pe.String(calendarPool,0xAE)+"{count}"+pe.String(calendarPool,0xAF));
        for (int i=0;i<10;i++) result.Entries.Add($"CALENDAR_MAKER_{i}",pe.String(calendarPool,0x25+i));
        int[] graduateEntries=[0x12,0x13,0x14,0x10];
        for (int i=0;i<4;i++) result.Entries.Add($"GRADUATE_GREETING_{i}",pe.String(graduatePool,graduateEntries[i]));
        result.Entries.Add("GRADUATE_SPEAKER","{name}"+pe.String(graduatePool,0x0A));
        // Serialized fGMKaiwa Timer1 interval property: 20 milliseconds.
        pe.Require(0x248EAC,6,0,(byte)'T',(byte)'i',(byte)'m',(byte)'e',(byte)'r',(byte)'1',0,0x0B,3,20,0,0,0);
        return result;
    }
}
