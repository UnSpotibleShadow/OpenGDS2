using System.Text.Json.Serialization;

namespace OpenGDS2.Domain;

public enum BankLoanResult { Success, CalendarUnknown, DateUnknown, EventPending, MoneyUnknown, AllSlotsOccupied }
public sealed class BankLoanReceipt
{
    [JsonRequired] public int Slot {get;set;}
    [JsonRequired] public int OriginalType {get;set;}
    [JsonRequired] public GameDate BorrowedOn {get;set;}
}

/// <summary>Bounded fGMBank.Command1 transaction; original menu-enable parity is separate.</summary>
public static class BankLoanService
{
    public static BankLoanResult CheckBorrow(GameState state)
    {
        if (state.Calendar is not { } calendar) return BankLoanResult.CalendarUnknown;
        if (state.Year is not int y || state.Month is not int m || state.Week is not int w) return BankLoanResult.DateUnknown;
        CalendarService.Validate(state);
        if (calendar.Pending is not null || state.PendingBankLoanReceipt is not null ||
            state.PendingHireGreeting is not null || state.PendingSecretaryAdvice is not null || state.PendingSecretaryReplacement is not null ||
            state.DevelopmentProjects.Any(p=>p.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(p)) ||
            state.ReleasedGames.Any(p=>p.ReviewPending) || SalesMilestoneService.HasPending(state) || RecallService.HasPending(state))
            return BankLoanResult.EventPending;
        CalendarRules.Validate(new(y,m,w));
        if (state.Money is null) return BankLoanResult.MoneyUnknown;
        return calendar.Loans.All(l=>l.OriginalType!=0) ? BankLoanResult.AllSlotsOccupied : BankLoanResult.Success;
    }
    public static BankLoanResult TryBorrow(GameState state,int type)
    {
        long principal=CalendarDialogueRules.LoanPrincipal(type);
        var result=CheckBorrow(state);if (result!=BankLoanResult.Success) return result;
        var date=new GameDate(state.Year!.Value,state.Month!.Value,state.Week!.Value);
        _=checked(date.Year+1); // A readable repayment date must exist before any mutation.
        var finance=FinancialRules.Prepare(state,principal);
        int slot=state.Calendar!.Loans.FindIndex(l=>l.OriginalType==0);
        var receipt=new BankLoanReceipt {Slot=slot,OriginalType=type,BorrowedOn=date};
        // Resolve every possible checked failure before storing the loan or funds.
        state.Calendar.Loans[slot]=new() {OriginalType=type,BorrowedOn=date};
        finance.Commit(state);state.PendingBankLoanReceipt=receipt;
        return BankLoanResult.Success;
    }
    public static bool Acknowledge(GameState state)
    {
        if (state.PendingBankLoanReceipt is null) return false;
        Validate(state);state.PendingBankLoanReceipt=null;return true;
    }
    public static GameDate DueOn(GameDate borrowed)=>new(checked(borrowed.Year+1),borrowed.Month,borrowed.Week);
    public static void Validate(GameState state)
    {
        if (state.PendingBankLoanReceipt is not { } receipt) return;
        if (state.Money is null || state.PendingHireGreeting is not null || state.PendingSecretaryAdvice is not null || state.PendingSecretaryReplacement is not null ||
            receipt.Slot is <0 or >2 || receipt.OriginalType is <1 or >4 || state.Calendar?.Pending is not null ||
            state.Calendar?.Loans is not {Count:3} loans || loans[receipt.Slot].OriginalType!=receipt.OriginalType ||
            loans[receipt.Slot].BorrowedOn!=receipt.BorrowedOn ||
            state.Year!=receipt.BorrowedOn.Year || state.Month!=receipt.BorrowedOn.Month || state.Week!=receipt.BorrowedOn.Week)
            throw new InvalidDataException("Invalid bank loan receipt.");
        try {CalendarRules.Validate(receipt.BorrowedOn);_=DueOn(receipt.BorrowedOn);}
        catch (Exception ex) when (ex is ArgumentOutOfRangeException or OverflowException) {throw new InvalidDataException("Invalid loan receipt date.",ex);}
    }
}
