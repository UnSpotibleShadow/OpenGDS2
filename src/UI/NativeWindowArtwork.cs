using Godot;
using OpenGDS2.Core;
namespace OpenGDS2.UI;
public static class NativeWindowArtwork
{
    private static readonly Dictionary<string,string> Forms=new()
    {
        ["OfficeWindow"]="fMainVisual",["CompanyWindow"]="fGMInfo",["EmployeeDetailsWindow"]="fGMInfo",["EmployeeWindow"]="fGMSyain",["ProductWindow"]="fGMZisha",
        ["ManagementWindow"]="fGMComK",["RecruitmentWindow"]="fGMSKoyouKz",["SecretaryHiringWindow"]="fGMHisyoK",["SecretaryPolicyWindow"]="fGMHisyoSet",
        ["SequelSelectionWindow"]="fGMZokuhen",["PromotionSelectorWindow"]="fGMSendenBaitai",["CompletionWindow"]="fGMKansei",["DevelopmentProjectWindow2"]="fGMDevW2",
        ["TrainingWindow"]="fGMWSTech",["HelpTopicWindow"]="fGMHelp",["CasinoPokerWindow"]="fCasinoPoker",["CasinoBlackjackWindow"]="fCasinoBJ",["CasinoCardChainWindow"]="fCasinoCC",["CasinoHorseWindow"]="fKeibaMain",
        ["HardwareProjectWindow"]="fGMDevH",["HardwareLaunchWindow"]="fGMSyukkaH",["SalesAchievementWindow"]="fGMOuri",["HardwareAnnouncementWindow"]="fGMHardHatubai",
        ["MangaSelectionWindow"]="fGMManga",["ProgrammingChallengeWindow"]="fGMKoudo",["PlanningSelectionWindow"]="fGMKaihatu",["PlanningMeetingWindow"]="fGMKikakusyo",["TypeMeetingWindow"]="fGMalpha",["GenreMeetingWindow"]="fGMbeta",
        ["DevelopmentProjectWindow"]="fGMDevW1",["DevelopmentCompleteWindow"]="fGMKansei",["ShipmentWindow"]="fGMSyukka",["MediaInformationWindow"]="fGMMedia",
        ["CompactProductWindow"]="fGMZishaKW",["MagazineReviewsWindow"]="fGMCrossR",["HallCelebrationWindow"]="fGMCrossRDen",
        ["ExhibitionSelectionWindow"]="fGMGameDexSn",["ExhibitionAttendanceWindow"]="fGMGameDexKs",["AwardCeremonyWindow"]="fGMKontesuto",["AwardAnnouncementWindow"]="fGMKontesutoHp",
        ["AnnualRenewalWindow"]="fGMKeiyakuS",["ConversationWindow"]="fGMKaiwa",["ComputerPurchaseWindow"]="fGMComK",["TechnologyPurchaseWindow"]="fGMTechKau",["LevelUpResultWindow"]="fGMLevelUP",
        ["HardwarePlanWindow"]="fGMTenantHardS",["NegotiationConfirmationWindow"]="fGMTenantBengo",["FacilityPickerWindow"]="fGMTenaSel",["FacilityStudyWindow"]="fGMTenantTechSel",
        ["FacilitySchoolWindow"]="fGMTenantGameS",["FacilityHotSpringWindow"]="fGMTenantOnsen",["FacilityRecyclingWindow"]="fGMTenantReS",["FacilityTechnologySchoolWindow"]="fGMTenantTechS",
        ["FacilityCasinoWindow"]="fGMTenantCasino",["FacilityLawyerWindow"]="fGMTenantBengo",["FacilityMysteryLabWindow"]="fGMTenantNazoP",["FacilityProductionLabWindow"]="fGMTenantSeisan",["FacilityHardwareLabWindow"]="fGMTenantHard"
    };
    public static Texture2D? Load(string window)=>Forms.TryGetValue(window,out var form)?OriginalAssetStore.LoadTexture("Graphic/Embedded/WindowIcon-"+form+".png"):null;
}
