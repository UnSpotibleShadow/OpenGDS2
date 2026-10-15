using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private bool ShowPendingCalendarEvent(GameState state)
    {
        var calendar=state.Calendar;var pending=calendar?.Pending;
        if (pending is null || pending.PendingEvent is null && pending.Block is null) return false;
        if(pending.PendingEvent==CalendarEvent.HardwareLaunch){ShowHardwareLaunchCard(state,pending);return true;}
        if (pending.DialoguePage is not null)
        {
            string text=DialogueText(CalendarDialogueRules.MessageKey(state));
            if (pending.PendingEvent is CalendarEvent.HardwareDevelopment or CalendarEvent.HardwareSoon or
                CalendarEvent.HardwareRetiring or CalendarEvent.HardwareRetired)
            {
                int source=calendar!.Hardware[CalendarDialogueRules.HardwareIndex(pending)].SourceIndex;
                var hardware=_activeCatalog?.Hardware.SingleOrDefault(x=>x.SourceIndex==source);
                string maker=hardware is not null && hardware.RawMembers.TryGetValue("0FA8",out long id) && id is >=0 and <=9 ?
                    DialogueText($"CALENDAR_MAKER_{id}") : Tr("UNKNOWN");
                text=text.Replace("{maker}",maker).Replace("{hardware}",hardware is null ? Tr("UNKNOWN") : CatalogName(_activeCatalog,"HARDWARE",source,hardware.Name));
            }
            if (pending.PendingEvent==CalendarEvent.LoanRepayment)
                text=text.Replace("{principal}",CalendarDialogueRules.LoanPrincipal(calendar!.Loans[pending.Index].OriginalType).ToDisplayNumber())
                    .Replace("{repayment}",pending.Amount!.Value.ToDisplayNumber());
            if (pending.Graduates is { } graduates) text=text.Replace("{count}",graduates.Candidates.Count.ToString());
            // These callers explicitly pass generic secretary/-2, unlike the
            // named portrait used by manual secretary advice.
            ShowConversation(DialogueText("CORE_SECRETARY"),[new(DialogueText("CORE_SECRETARY"),text)],()=>
            {
                if (!ReferenceEquals(state.Calendar?.Pending,pending)) return;
                CalendarDialogueRules.Acknowledge(state);ShowOfficeReconstruction(state);
            });
            return true;
        }
        if (pending.Graduates is {GreetingVariant:not null} greeting)
        {ShowGraduate(state,greeting);return true;}
        if(pending.PendingEvent==CalendarEvent.Awards && pending.Awards is { } awards){ShowAwards(state,awards);return true;}
        ClearContent();IdentifyManagementWindow("CalendarEventWindow");AddHeading(Tr("CALENDAR_TRANSACTIONS"),28);
        AddHeading($"{Tr("FUNDS")}: ¥{DisplayNumbers.Format(state.Money)}",20);
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
        else if (ShowAnnualCalendarEvent(state,pending)) { }
        else if (pending.PendingEvent==CalendarEvent.ContractRenewal)
        {
            ShowAnnualRenewal(state);
        }
        else if (pending.PendingEvent is CalendarEvent kind)
        {
            AddHeading(Tr($"CALENDAR_EVENT_{kind}"),22);
            if (kind==CalendarEvent.LoanRepayment) AddHeading($"¥{DisplayNumbers.Format(pending.Amount)}",22);
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
        if (_dialog?.FindChild("NativeDialogCanvas",true,false) is null) AddSaveActions(state);
        return true;
    }
}
