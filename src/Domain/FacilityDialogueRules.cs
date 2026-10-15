namespace OpenGDS2.Domain;

/// <summary>Local-only original facility messages; English uses authored translations.</summary>
public static class FacilityDialogueRules
{
    public static readonly IReadOnlyList<string> ImportKeys=new string[]
    {
        "FACILITY_INTRO_HotSpring",
        "FACILITY_QUESTION_HotSpring",
        "FACILITY_DONE_HotSpring",
        "FACILITY_INTRO_Recycling",
        "FACILITY_QUESTION_Recycling",
        "FACILITY_DONE_Recycling",
        "FACILITY_INTRO_ProductionLab",
        "FACILITY_QUESTION_ProductionLab",
        "FACILITY_DONE_ProductionLab",
        "FACILITY_INTRO_MysteryLab",
        "FACILITY_QUESTION_MysteryLab",
        "FACILITY_MYSTERY_BUSY",
        "FACILITY_DONE_TechnologySchool",
        "FACILITY_NO_EMPLOYEES",
        "FACILITY_NO_FUNDS",
        "FACILITY_ALREADY_OWNED",
        "FACILITY_HARDWARE_LIMIT",
        "FACILITY_HARDWARE_BUSY",
        "FACILITY_HARDWARE_ROOM_BUSY",
        "FACILITY_CASINO_NO_COINS",
        "FACILITY_CASINO_COINS_QUESTION",
        "FACILITY_CLOSE_INTRO",
        "FACILITY_CLOSE_QUESTION",
        "HOT_SPRING_REACTION_0",
        "HOT_SPRING_REACTION_1",
        "HOT_SPRING_REACTION_2",
        "HOT_SPRING_REACTION_3",
        "HOT_SPRING_REACTION_4",
    }.Concat(Enumerable.Range(0,6).Select(i=>"PROMOTION_NATIVE_RESULT_"+i))
        .Concat(Enumerable.Range(0,6).Select(i=>"PROMOTION_ACTIVITY_"+i)).ToArray();
}
