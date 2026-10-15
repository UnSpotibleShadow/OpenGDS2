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
        var result = new OriginalDialogueCatalog {SchemaVersion=18,SourceHash=EmployeeDatabaseImporter.ExpectedHash};
        const int poachPool=0x606824;
        int[] rivalMessages=[0x40,0x41,0x42,0x44,0x45,0x46,0x47,0x48,0x49,0x4A];
        for(int i=0;i<10;i++)result.Entries.Add("POACHING_RIVAL_MESSAGE_"+i,pe.String(poachPool,rivalMessages[i]));
        for(int i=0;i<4;i++)result.Entries.Add("POACHING_EMPLOYEE_MESSAGE_"+i,pe.String(poachPool,0x4B+i));
        result.Entries.Add("POACHING_TARGET_INTRO","{name}"+pe.String(0x60B404,3));
        result.Entries.Add("POACHING_CONFIRM",pe.String(0x60B404,5));
        result.Entries.Add("POACHING_DONATION_ACK","{name}"+pe.String(0x5EE018,8)+"{amount}"+pe.String(0x5EE018,9));
        result.Entries.Add("POACHING_RIVAL_ALERT",pe.String(0x5F99AC,0x180)+"{name}"+pe.String(0x5F99AC,0x181));
        result.Entries.Add("POACHING_DEFENCE_INTRO",pe.String(0x5F99AC,0x182));
        result.Entries.Add("POACHING_DECISION_INTRO",pe.String(poachPool,0x50)+"{name}"+pe.String(poachPool,0x51));
        result.Entries.Add("POACHING_DECISION_SUSPENSE",pe.String(poachPool,0x55));
        result.Entries.Add("POACHING_CHOICE_RIVAL",pe.String(poachPool,0x56));
        result.Entries.Add("POACHING_CHOICE_PLAYER","{company}"+pe.String(poachPool,0x5D));
        result.Entries.Add("POACHING_RESULT_LOST_OFFENSIVE",pe.String(poachPool,0x58));
        result.Entries.Add("POACHING_RESULT_LOST_DEFENSIVE",pe.String(poachPool,0x59)+"{name}"+pe.String(poachPool,0x5A));
        result.Entries.Add("POACHING_RESULT_WON_OFFENSIVE","{name}"+pe.String(poachPool,0x5E));
        result.Entries.Add("POACHING_RESULT_WON_DEFENSIVE","{name}"+pe.String(poachPool,0x60));
        // fGMCrossR comment branches: 49 reachable variants, local strings only.
        const int reviewPool=0x5F92C4;
        int[][][] reviewComments=[
            [[0x2b], [0x2c, -1, 0x2f, -2, 0x31], [0x32], [0x33], [0x34], [0x35], [0x36], [0x37, -1, 0x38], [0x39]],
            [[0x3a], [0x3b], [0x3c], [0x3d, -2, 0x3e], [0x3f], [0x40], [0x41], [0x42, -2, 0x43]],
            [[0x3a], [-1, 0x44, -2, 0x45], [0x46], [0x47], [0x48], [0x49], [0x4a], [0x4b], [0x4c], [-2, 0x4d], [0x4e, -1, 0x4f]],
            [[0x50], [-1, 0x44, -2, 0x51], [0x52], [0x53], [0x54, -1, 0x55], [0x56, -2, 0x57], [0x58], [0x59, -1, 0x5a], [0x5b]],
            [[0x5c], [0x5d], [0x5e], [0x5f], [0x60, -3, 0x62], [-3, 0x63], [0x64], [-3, 0x65]],
            [[0x66], [0x67], [0x68], [0x69, -3, 0x6a]]
        ];
        for(int band=0;band<reviewComments.Length;band++)
            for(int variant=0;variant<reviewComments[band].Length;variant++)
                result.Entries.Add($"REVIEW_COMMENT_{band}_{variant}",string.Concat(reviewComments[band][variant].Select(token=>
                    token switch {-1=>"{genre}",-2=>"{type}",-3=>"{company}",_=>pe.String(reviewPool,token)})));
        const int awardPool=0x5F56BC;
        result.Entries.Add("AWARD_INTRO_COUNT",pe.String(awardPool,0x18)+"{count}"+pe.String(awardPool,0x19));
        result.Entries.Add("AWARD_HOST",pe.String(awardPool,0x1B));
        result.Entries.Add("AWARD_WELCOME",pe.String(awardPool,0x1E)+"{year}"+pe.String(awardPool,0x1F));
        int[] intros=[0x20,0x35,0x3B,0x42,0x4B],prizes=[0x21,0x36,0x3C,0x43,0x4C],winners=[0x28,0x39,0x3F,0x46,0x50];
        int[][] reactions=[[0x31,0x32,0x33],[0x31,0x3A,0x33],[0x31,0x40,0x41],[0x48,0x49,0x4A],[0x52,0x53,0x54]];
        for(int category=0;category<5;category++)
        {
            result.Entries.Add("AWARD_INTRO_"+category,pe.String(awardPool,intros[category]));
            result.Entries.Add("AWARD_PRIZE_"+category,pe.String(awardPool,prizes[category]));
            result.Entries.Add("AWARD_WINNER_"+category,pe.String(awardPool,winners[category])+"{company}"+pe.String(awardPool,0x29)+"{title}"+pe.String(awardPool,0x2B));
            for(int page=0;page<3;page++)result.Entries.Add($"AWARD_REACTION_{category}_{page}",pe.String(awardPool,reactions[category][page]));
        }
        result.Entries.Add("AWARD_GRAND_SUSPENSE",pe.String(awardPool,0x4D));
        result.Entries.Add("AWARD_RANKING",pe.String(awardPool,0x59));
        for(int page=0;page<3;page++)result.Entries.Add("AWARD_CLOSING_"+page,pe.String(awardPool,0x65+page));
        const int pool = 0x60C264;
        for (int variant = 0; variant < 9; variant++)
            result.Entries.Add($"HIRE_GREETING_{variant}",pe.String(pool,0x11+variant));
        result.Entries.Add("HIRE_GREETING_9",pe.String(pool,0x1A)+"{company}"+pe.String(pool,0x1C));
        result.Entries.Add("NEW_EMPLOYEE",pe.String(pool,0x10));
        const int meetingPool=0x5FAFCC;
        const int periodicPool=0x5F99AC;
        const int springPool=0x5F6354,recyclePool=0x5F1A68,productionPool=0x5F4400,mysteryPool=0x5F5D08;
        result.Entries.Add("FACILITY_INTRO_HotSpring",pe.String(springPool,8)+"{fee}"+pe.String(springPool,9));
        result.Entries.Add("FACILITY_QUESTION_HotSpring",pe.String(springPool,0xB));
        result.Entries.Add("FACILITY_DONE_HotSpring",pe.String(springPool,0x14));
        for(int i=0;i<5;i++)result.Entries.Add("HOT_SPRING_REACTION_"+i,pe.String(springPool,0x1C+i));
        result.Entries.Add("FACILITY_INTRO_Recycling",pe.String(recyclePool,0xA)+"{stock}"+pe.String(recyclePool,0xB));
        result.Entries.Add("FACILITY_QUESTION_Recycling",pe.String(recyclePool,0xD));
        result.Entries.Add("FACILITY_DONE_Recycling",pe.String(recyclePool,0x10)+"{amount}"+pe.String(recyclePool,0x11));
        result.Entries.Add("FACILITY_INTRO_ProductionLab",pe.String(productionPool,8)+"{fee}"+pe.String(productionPool,9));
        result.Entries.Add("FACILITY_QUESTION_ProductionLab",pe.String(productionPool,0xB));
        result.Entries.Add("FACILITY_DONE_ProductionLab",pe.String(productionPool,0xF)+"{before}"+pe.String(productionPool,0x11)+"{after}"+pe.String(productionPool,0x12));
        result.Entries.Add("FACILITY_INTRO_MysteryLab",pe.String(mysteryPool,0xB)+"{fee}"+pe.String(mysteryPool,0xC));
        result.Entries.Add("FACILITY_QUESTION_MysteryLab",pe.String(mysteryPool,0xD));
        result.Entries.Add("FACILITY_MYSTERY_BUSY",pe.String(mysteryPool,6));
        result.Entries.Add("FACILITY_DONE_TechnologySchool","{name}"+pe.String(0x5F319C,9)+"{capacity}"+pe.String(0x5F319C,0xA));
        result.Entries.Add("FACILITY_NO_EMPLOYEES",pe.String(springPool,0x10));
        result.Entries.Add("FACILITY_NO_FUNDS",pe.String(springPool,0x12));
        result.Entries.Add("FACILITY_ALREADY_OWNED",pe.String(0x5F4A34,7)+"{facility}"+pe.String(0x5F4A34,9));
        result.Entries.Add("FACILITY_HARDWARE_LIMIT",pe.String(0x5F14C4,6));
        result.Entries.Add("FACILITY_HARDWARE_BUSY",pe.String(0x5F14C4,9));
        result.Entries.Add("FACILITY_HARDWARE_ROOM_BUSY",pe.String(0x5F8BB0,8)+"{room}"+pe.String(0x5F8BB0,9));
        result.Entries.Add("FACILITY_CASINO_NO_COINS",pe.String(0x5EA164,0xC));
        result.Entries.Add("FACILITY_CASINO_COINS_QUESTION",pe.String(0x5EA164,0x17));
        result.Entries.Add("FACILITY_CLOSE_INTRO",pe.String(springPool,0x21)+"{facility}"+pe.String(springPool,0x25));
        result.Entries.Add("FACILITY_CLOSE_QUESTION","{facility}"+pe.String(springPool,0x26));
        int[] promotionResults=[0x66,0x69,0x6A,0x6B,0x6C,0x6D];
        for(int i=0;i<6;i++)
        {
            result.Entries.Add("PROMOTION_NATIVE_RESULT_"+i,pe.String(meetingPool,promotionResults[i])+"{title}"+pe.String(meetingPool,0x67)+"{gain}"+pe.String(meetingPool,0x68));
            result.Entries.Add("PROMOTION_ACTIVITY_"+i,pe.String(meetingPool,0x50+i));
        }

        for(int i=0;i<5;i++)result.Entries.Add("INTERVIEW_REPORTER_"+i,pe.String(meetingPool,0xA2+i));
        result.Entries.Add("INTERVIEW_SPEAKER",pe.String(meetingPool,0xA7));
        result.Entries.Add("INTERVIEW_INTRO",pe.String(periodicPool,0xA1)+"{room}"+pe.String(periodicPool,0x177)+"{magazine}"+pe.String(periodicPool,0x179));
        result.Entries.Add("INTERVIEW_RESULT",pe.String(periodicPool,0xA1)+"{room}"+pe.String(periodicPool,0x17A)+"{gain}"+pe.String(periodicPool,0x17B));
        result.Entries.Add("INTERVIEW_PAGE_0",pe.String(meetingPool,0xA9)+"{magazine}"+pe.String(meetingPool,0xAC)+"{name}"+pe.String(meetingPool,0xAD));
        result.Entries.Add("INTERVIEW_PAGE_1",pe.String(meetingPool,0xAE)+"{subject}"+pe.String(meetingPool,0xB0));
        result.Entries.Add("INTERVIEW_PAGE_2",pe.String(meetingPool,0xB1));
        result.Entries.Add("INTERVIEW_PAGE_3",pe.String(meetingPool,0xB2));
        result.Entries.Add("MYSTERY_LAB_INTRO",pe.String(meetingPool,0x11C));
        int[] labCells=[0x11E,0x11F,0x120,0x122,0x124,0x128,0x129];
        for (int i=0;i<labCells.Length;i++) result.Entries.Add($"MYSTERY_LAB_RESULT_{i}",pe.String(meetingPool,labCells[i]));
        result.Entries.Add("MYSTERY_LAB_RESULT_4_HIGH",pe.String(meetingPool,0x125));
        foreach (var target in Enum.GetValues<FeeNegotiationTarget>())
        {
            int[] cells=target==FeeNegotiationTarget.Transport ? [0x106,0x108,0x109,0x10E,0x10A,0x111] : [0x115,0x117,0x118,0x11A,0x119,0x11B];
            string[] names=["INTRO","SPEAKER","FIRST_GREETING","GREETING","QUESTION","FAILURE_SUMMARY"];
            for (int i=0;i<cells.Length;i++) result.Entries.Add(FeeNegotiationService.Key(target,names[i]),pe.String(meetingPool,cells[i]));
        }
        result.Entries.Add("NEGOTIATION_FIRST_SUCCESS",pe.String(meetingPool,0x10B)+"{fee}"+pe.String(meetingPool,0x10C));
        result.Entries.Add("NEGOTIATION_SUCCESS",pe.String(meetingPool,0x10F)+"{fee}"+pe.String(meetingPool,0x10C));
        result.Entries.Add("NEGOTIATION_FAILURE",pe.String(meetingPool,0x110));
        result.Entries.Add("NEGOTIATION_FLOOR",pe.String(meetingPool,0x113));
        result.Entries.Add("NEGOTIATION_SUCCESS_SUMMARY",pe.String(meetingPool,0x10D));
        result.Entries.Add("NEGOTIATION_FLOOR_SUMMARY",pe.String(meetingPool,0x114));
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
        const int backupPool=0x614CC8;
        result.Entries.Add("BACKUP_DEVICE_EXPLANATION",pe.String(backupPool,0x59));
        result.Entries.Add("BACKUP_DEVICE_RESULT_AlreadyOwned",pe.String(backupPool,0x58));
        result.Entries.Add("BACKUP_DEVICE_RESULT_InsufficientFunds",pe.String(backupPool,0x5D));
        result.Entries.Add("BACKUP_DEVICE_RESULT_Success",pe.String(backupPool,0x5E));
        result.Entries.Add("VIRUS_INTRO",pe.String(workPool,0x187)+"{room}"+pe.String(workPool,0x188));
        result.Entries.Add("VIRUS_PROTECTED",pe.String(workPool,0x194));
        result.Entries.Add("VIRUS_STRESS",pe.String(workPool,0x191)+"{room}"+pe.String(workPool,0x192));
        result.Entries.Add("VIRUS_BUY_BACKUP",pe.String(workPool,0x193));
        for(int i=0;i<7;i++)
        {
            result.Entries.Add($"VIRUS_LOSS_REACTION_{i}",pe.String(workPool,0x18A+i));
            result.Entries.Add($"VIRUS_PROTECTED_REACTION_{i}",pe.String(workPool,0x195+i));
        }
        result.Entries.Add("CORE_SECRETARY",pe.String(workPool,0x77));
        result.Entries.Add("FINANCIAL_FAILURE_INTRO_0",pe.String(workPool,0xEF));
        result.Entries.Add("FINANCIAL_FAILURE_INTRO_1",pe.String(workPool,0xF0));
        result.Entries.Add("FINANCIAL_FAILURE_CLOSING",pe.String(workPool,0xFB));
        result.Entries.Add("FINANCIAL_FAILURE_NAMED",pe.String(workPool,0xD6)+"{name}"+pe.String(workPool,0xD7));
        result.Entries.Add("FINANCIAL_FAILURE_EMPLOYEE",pe.String(workPool,0xD9));
        for (int i=0;i<8;i++) result.Entries.Add(FinancialFailureService.ReactionKey(i),pe.String(workPool,0xF3+i));
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
        const int cancellationPool=0x5FEE38;
        pe.Require(pe.Offset(0x9AAA8C+0x032E),0x27,0x58,0xFF,0x0A,0x15,0,4,0,0xF4,7,0xEB,0xB3,0xFB,0xE6);
        result.Entries.Add("CANCELLATION_EMPLOYEE_NAMED",pe.String(cancellationPool,0x16)+"{name}"+pe.String(cancellationPool,0x19));
        for (int i=0;i<7;i++) result.Entries.Add(ProjectCancellationDialogueRules.ReactionKey(i),pe.String(cancellationPool,0x1C+i));
        result.Entries.Add("CANCELLATION_STRESS_SUMMARY",pe.String(cancellationPool,0x24)+"{room}"+pe.String(cancellationPool,0x25));
        result.Entries.Add("CANCELLATION_DISCARD_SUMMARY",pe.String(cancellationPool,0x24)+"{room}"+pe.String(cancellationPool,0x26));
        // Serialized fGMKaiwa Timer1 interval property: 20 milliseconds.
        pe.Require(0x248EAC,6,0,(byte)'T',(byte)'i',(byte)'m',(byte)'e',(byte)'r',(byte)'1',0,0x0B,3,20,0,0,0);
        return result;
    }
}
