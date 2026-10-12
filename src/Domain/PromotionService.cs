namespace OpenGDS2.Domain;

public enum PromotionResult { Success, AlphaRequired, HistoryUnknown, MoneyUnknown, InsufficientFunds, EventPending, InvalidTarget }
public sealed record PromotionMedium(long Fee,int FirstBase,int SecondBase,int Span);
public sealed class PromotionMeetingState
{
    public int RoomIndex {get;set;}
    public int MediaIndex {get;set;}
    public int Callbacks {get;set;}
    public double? RecognitionBefore {get;set;}
    public double? Gain {get;set;}
}

/// <summary>00980A38 / 00982064; 009EFA40 mode 1, fresh-Empty room scope.</summary>
public static class PromotionService
{
    public static IReadOnlyList<PromotionMedium> Media {get;}=Array.AsReadOnly(new PromotionMedium[]
    {
        new(5_000_000,6,3,9),new(10_000_000,7,3,9),new(20_000_000,10,5,9),
        new(25_000_000,8,4,9),new(20_000_000,11,5,11),new(25_000_000,12,6,13)
    });
    public static PromotionResult Check(GameState state,DevelopmentProjectState project,int media)
    {
        if(media<0 || media>=Media.Count || !state.DevelopmentProjects.Contains(project) || project.OriginalRoomState!=1)
            return PromotionResult.InvalidTarget;
        if(state.PendingPromotion is not null || project.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(project) ||
            CalendarService.HasPending(state) || state.FinancialFailure is not null || state.Casino.ActivityOpen ||
            OwnHardwareService.HasPending(state) || state.PendingHireGreeting is not null || state.PendingSecretaryAdvice is not null ||
            state.PendingSecretaryReplacement is not null || state.PendingBankLoanReceipt is not null ||
            state.PendingProjectCancellation is not null || state.PendingMysteryLabResult is not null ||
            state.PendingFacilityReceipt is not null || state.PendingFeeNegotiation is not null ||
            state.ReleasedGames.Any(p=>p.ReviewPending) || SalesMilestoneService.HasPending(state) || RecallService.HasPending(state))
            return PromotionResult.EventPending;
        if(project.OriginalStage<1)return PromotionResult.AlphaRequired;
        if(project.PromotionUseCount is null)return PromotionResult.HistoryUnknown;
        if(state.Money is null)return PromotionResult.MoneyUnknown;
        return state.Money<Media[media].Fee ? PromotionResult.InsufficientFunds : PromotionResult.Success;
    }
    public static PromotionResult Start(GameState state,DevelopmentProjectState project,int media)
    {
        var result=Check(state,project,media);if(result!=PromotionResult.Success)return result;
        state.PendingPromotion=new() {RoomIndex=project.RoomIndex,MediaIndex=media};return result;
    }
    public static bool Advance(GameState state,Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if(state.PendingPromotion is not { } meeting || meeting.Callbacks>=27)return false;
        if(meeting.Callbacks<26){meeting.Callbacks++;return true;}
        var project=state.DevelopmentProjects.Single(p=>p.RoomIndex==meeting.RoomIndex);
        var medium=Media[meeting.MediaIndex];
        int count=checked(project.PromotionUseCount!.Value+1);
        var finance=FinancialRules.Prepare(state,-medium.Fee);
        float draw=random.NextSingle();
        if(!float.IsFinite(draw) || draw<0 || draw>=1)throw new ArgumentOutOfRangeException(nameof(random));
        int basis=project.PromotionUseCount switch {0=>medium.FirstBase,1=>medium.SecondBase,_=>0};
        double gain=Math.Min(basis+Math.Floor((double)draw*medium.Span),99-project.Recognition);
        meeting.RecognitionBefore=project.Recognition;meeting.Gain=gain;
        project.Recognition+=gain;project.PromotionUseCount=count;finance.Commit(state);meeting.Callbacks=27;
        return true;
    }
    public static bool Acknowledge(GameState state)
    {
        if(state.PendingPromotion?.Callbacks!=27)return false;
        state.PendingPromotion=null;return true;
    }
    public static void Validate(GameState state)
    {
        if(state.PendingPromotion is not { } meeting)return;
        var project=state.DevelopmentProjects?.SingleOrDefault(p=>p.RoomIndex==meeting.RoomIndex);
        if(project is null || project.OriginalRoomState!=1 || project.OriginalStage<1 || project.PromotionUseCount is null or <0 ||
            meeting.MediaIndex<0 || meeting.MediaIndex>=Media.Count || meeting.Callbacks is <0 or >27 ||
            project.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(project) || CalendarService.HasPending(state))
            throw new InvalidDataException("Invalid promotion meeting.");
        if(meeting.Callbacks<27 ? meeting.Gain is not null || meeting.RecognitionBefore is not null :
            meeting.Gain is not double gain || !double.IsFinite(gain) || meeting.RecognitionBefore is not double before ||
            !double.IsFinite(before) || before<0 || project.PromotionUseCount<1 || before+gain!=project.Recognition || project.Recognition>99)
            throw new InvalidDataException("Invalid promotion result.");
    }
}
