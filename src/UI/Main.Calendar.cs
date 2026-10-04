using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private bool ShowPendingCalendarEvent(GameState state)
    {
        var calendar=state.Calendar;var pending=calendar?.Pending;
        if (pending is null || pending.PendingEvent is null && pending.Block is null) return false;
        ClearContent();AddHeading(Tr("CALENDAR_TRANSACTIONS"),28);
        AddHeading($"{Tr("FUNDS")}: ¥{state.Money:N0}",20);
        if (pending.Block is CalendarBlock block)
        {
            var message=new Label {Name="CalendarBlock",Text=Tr($"CALENDAR_BLOCK_{block}"),
                AutowrapMode=TextServer.AutowrapMode.WordSmart,CustomMinimumSize=new Vector2(540,0)};
            _content.AddChild(message);
            if (block==CalendarBlock.PayrollUnknown)
            {
                var staff=new Button {Name="OpenPayrollStaff",Text=Tr("STAFF")};_content.AddChild(staff);
                staff.Pressed+=()=>ShowStaff(state);
                var retry=new Button {Name="RetryCalendar",Text=Tr("CONTINUE")};_content.AddChild(retry);
                retry.Pressed+=()=>{_session.TickOffice(_activeCatalog);ShowOfficeReconstruction(state);};
            }
        }
        else if (pending.PendingEvent==CalendarEvent.ContractRenewal)
        {
            AddHeading(Tr("ANNUAL_CONTRACT_RENEWAL"),22);
            var selection=new Dictionary<int,CheckBox>();
            foreach (var employee in state.Employees.OrderBy(x=>x.SourceIndex))
            {
                int id=employee.SourceIndex ?? throw new InvalidOperationException("Unknown employee identity.");
                var checkbox=new CheckBox {Name=$"RenewEmployee{id}",ButtonPressed=true,
                    Text=$"{employee.Name} — ¥{employee.ContractFee:N0}"};
                selection.Add(id,checkbox);_content.AddChild(checkbox);
            }
            var confirm=new Button {Name="ConfirmContractRenewal",Text=Tr("CONFIRM_CONTRACT_RENEWAL")};_content.AddChild(confirm);
            confirm.Pressed+=()=>
            {
                var result=CalendarService.RenewContracts(state,selection.Where(x=>x.Value.ButtonPressed).Select(x=>x.Key).ToHashSet());
                if (result==ContractRenewalResult.Success) ShowOfficeReconstruction(state);
                else _status.Text=Tr($"CONTRACT_RENEWAL_{result}");
            };
        }
        else if (pending.PendingEvent is CalendarEvent kind)
        {
            AddHeading(Tr($"CALENDAR_EVENT_{kind}"),22);
            if (kind==CalendarEvent.LoanRepayment) AddHeading($"¥{pending.Amount:N0}",22);
            else
            {
                int index=kind is CalendarEvent.HardwareDevelopment or CalendarEvent.HardwareRetiring or CalendarEvent.HardwareRetired ?
                    pending.Index-1 : pending.Index;
                int source=calendar!.Hardware[index].SourceIndex;
                var hardware=_activeCatalog?.Hardware.SingleOrDefault(x=>x.SourceIndex==source);
                if (hardware is not null) AddHeading(CatalogName(_activeCatalog,"HARDWARE",source,hardware.Name),22);
            }
            var acknowledge=new Button {Name="AcknowledgeCalendar",Text=Tr("CONTINUE")};_content.AddChild(acknowledge);
            acknowledge.Pressed+=()=>{CalendarService.Acknowledge(state);ShowOfficeReconstruction(state);};
        }
        AddSaveActions(state);
        return true;
    }
}
