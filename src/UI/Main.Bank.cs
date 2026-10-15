using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private string BankDate(GameDate date)=>Tr("BANK_DATE").Replace("{year}",date.Year.ToString()).Replace("{month}",date.Month.ToString()).Replace("{week}",date.Week.ToString());
    private void ShowBank(GameState state)
    {
        if (ShowPendingBankLoanReceipt(state)) return;
        ClearContent();var canvas=NativeWindow("BankWindow",Tr("BANK"),new(414,220));
        string[] headings=["BANK_DUE_DATE","BANK_PRINCIPAL","BANK_REPAYMENT_COLUMN"];
        int[] xs=[10,126,258];int[] widths=[110,126,142];
        NativeFill(canvas,"BankLoanTable",new(4,24,406,65),Colors.White,true);
        for(int c=0;c<3;c++)NativeText(canvas,"BankHeader"+c,Tr(headings[c]),new(xs[c],4,widths[c],20),10,c==0?HorizontalAlignment.Left:HorizontalAlignment.Right);
        for(int i=0;i<3;i++)
        {
            var loan=state.Calendar?.Loans.ElementAtOrDefault(i);
            bool occupied=loan is {OriginalType:>0};
            string date=occupied && loan!.BorrowedOn is GameDate borrowed ? $"{BankLoanService.DueOn(borrowed).Year}/{BankLoanService.DueOn(borrowed).Month}/{BankLoanService.DueOn(borrowed).Week}" : loan is null||occupied?"?":"—";
            NativeText(canvas,"BankLoanDate"+i,date,new(xs[0],28+i*19,widths[0],19),11);
            NativeText(canvas,"BankLoanPrincipal"+i,loan is null?"?":occupied?$"¥{CalendarDialogueRules.LoanPrincipal(loan!.OriginalType):N0}":"—",new(xs[1],28+i*19,widths[1],19),11,HorizontalAlignment.Right);
            NativeText(canvas,"BankLoanRepayment"+i,loan is null?"?":occupied?$"¥{CalendarService.LoanRepayment(loan!.OriginalType):N0}":"—",new(xs[2],28+i*19,widths[2],19),11,HorizontalAlignment.Right);
        }
        var eligibility=BankLoanService.CheckBorrow(state);
        _status=NativeText(canvas,"BankStatus",eligibility==BankLoanResult.Success?"":Tr($"BANK_RESULT_{eligibility}"),new(4,164,406,46),10);
        _status.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        int? selected=null;var group=new ButtonGroup();
        var confirm=NativeAction(canvas,"ConfirmBankLoan","BANK_BORROW",new(310,96,100,25),()=>{},false);
        for(int type=1;type<=4;type++)
        {
            int sourceType=type;
            var choice=new CheckBox {Name=$"BankLoanType{type}",Text=$"¥{CalendarDialogueRules.LoanPrincipal(type):N0}",ButtonGroup=group,
                Disabled=eligibility!=BankLoanResult.Success,TooltipText=Tr("BANK_REPAYMENT").Replace("{amount}",$"¥{CalendarService.LoanRepayment(type):N0}")};
            foreach(string mode in new[]{"normal","pressed","hover","disabled","focus","hover_pressed"})choice.AddThemeStyleboxOverride(mode,new StyleBoxEmpty());
            choice.AddThemeFontSizeOverride("font_size",11);
            NativePlace(canvas,choice,new(4+((type-1)/2)*150,96+((type-1)%2)*23,148,23));
            choice.Pressed+=()=>{choice.ButtonPressed=true;selected=sourceType;confirm.Disabled=BankLoanService.CheckBorrow(state)!=BankLoanResult.Success;};
        }
        NativeText(canvas,"BankTerms",Tr("BANK_TERMS"),new(4,144,406,19),10);
        NativeAction(canvas,"CancelBankLoan","CANCEL",new(310,126,100,25),()=>ShowOfficeReconstruction(state));
        confirm.Pressed+=()=>
        {
            if (!selected.HasValue) return;
            confirm.Disabled=true;
            var result=BankLoanService.TryBorrow(state,selected.Value);
            if (result==BankLoanResult.Success) ShowPendingBankLoanReceipt(state);
            else _status.Text=Tr($"BANK_RESULT_{result}");
        };
    }
    private bool ShowPendingBankLoanReceipt(GameState state)
    {
        if (state.PendingBankLoanReceipt is not { } receipt) return false;
        RefreshDesktop();
        string body=Tr("BANK_RECEIPT").Replace("{principal}",$"¥{CalendarDialogueRules.LoanPrincipal(receipt.OriginalType):N0}")
            .Replace("{repayment}",$"¥{CalendarService.LoanRepayment(receipt.OriginalType):N0}").Replace("{date}",BankDate(BankLoanService.DueOn(receipt.BorrowedOn)));
        ShowConversation(Tr("BANK"),[new(Tr("BANK_CLERK"),body)],()=>
        {
            if (!ReferenceEquals(state.PendingBankLoanReceipt,receipt)) return;
            BankLoanService.Acknowledge(state);ShowOfficeReconstruction(state);
        });
        return true;
    }
}
