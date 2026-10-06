using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private string BankDate(GameDate date)=>Tr("BANK_DATE").Replace("{year}",date.Year.ToString()).Replace("{month}",date.Month.ToString()).Replace("{week}",date.Week.ToString());
    private void ShowBank(GameState state)
    {
        if (ShowPendingBankLoanReceipt(state)) return;
        ClearContent();CompactManagementWindow("BankWindow",new Vector2(510,390),490);_dialog?.SetTitle(Tr("BANK"));
        AddHeading(Tr("BANK_TERMS"),16);
        var eligibility=BankLoanService.CheckBorrow(state);
        if (state.Calendar is { } calendar)
        {
            for (int i=0;i<calendar.Loans.Count;i++)
            {
                var loan=calendar.Loans[i];
                string text=$"{Tr("BANK_LOAN_SLOT")} {i+1}: "+(loan.OriginalType==0 ? Tr("BANK_EMPTY_SLOT") :
                    $"¥{CalendarDialogueRules.LoanPrincipal(loan.OriginalType):N0} · {BankDate(BankLoanService.DueOn(loan.BorrowedOn!.Value))} · ¥{CalendarService.LoanRepayment(loan.OriginalType):N0}");
                _content.AddChild(new Label {Name=$"BankLoanSlot{i}",Text=text,AutowrapMode=TextServer.AutowrapMode.WordSmart});
            }
        }
        _status=new Label {Name="BankStatus",AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(_status);
        if (eligibility!=BankLoanResult.Success) _status.Text=Tr($"BANK_RESULT_{eligibility}");
        int? selected=null;var group=new ButtonGroup();
        var confirm=new Button {Name="ConfirmBankLoan",Text=Tr("BANK_BORROW"),Disabled=true};
        for (int type=1;type<=4;type++)
        {
            int sourceType=type;
            var choice=new Button {Name=$"BankLoanType{type}",Text=$"¥{CalendarDialogueRules.LoanPrincipal(type):N0}",ToggleMode=true,ButtonGroup=group,
                Disabled=eligibility!=BankLoanResult.Success,TooltipText=Tr("BANK_REPAYMENT").Replace("{amount}",$"¥{CalendarService.LoanRepayment(type):N0}")};
            _content.AddChild(choice);
            choice.Pressed+=()=>{choice.ButtonPressed=true;selected=sourceType;confirm.Disabled=BankLoanService.CheckBorrow(state)!=BankLoanResult.Success;};
        }
        _content.AddChild(confirm);
        confirm.Pressed+=()=>
        {
            if (!selected.HasValue) return;
            confirm.Disabled=true;
            var result=BankLoanService.TryBorrow(state,selected.Value);
            if (result==BankLoanResult.Success) ShowPendingBankLoanReceipt(state);
            else _status.Text=Tr($"BANK_RESULT_{result}");
        };
        AddAction("CANCEL",()=>ShowOfficeReconstruction(state));
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
