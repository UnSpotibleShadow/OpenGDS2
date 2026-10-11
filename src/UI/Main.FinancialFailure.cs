using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private bool ShowPendingFinancialFailure(GameState state)
    {
        if (state.FinancialFailure is not { } pending) return false;
        if (pending.Phase==4) {ShowTitleScreen();return true;}
        if (pending.Phase==2 && pending.Slots is null)
        {
            ClearContent();AddHeading(Tr("FINANCIAL_FAILURE"),24);
            AddHeading(Tr("FINANCIAL_FAILURE_ROSTER_UNKNOWN"),16);
            AddAction("LOAD_GAME",LoadSession,System.IO.File.Exists(SavePath));
            AddAction("BACK_TO_TITLE",ShowTitleScreen);
            return true;
        }
        int phase=pending.Phase,source=pending.NextSource;
        bool english=Godot.TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase);
        string Text(string key)=>english ? Tr(key) : _importer.LoadDialogue()?.Entries.GetValueOrDefault(key) ?? Tr(key);
        string heading=Tr("FINANCIAL_FAILURE"),speaker=DialogueText("CORE_SECRETARY"),body;
        int? portrait=null;
        if (phase==2 && pending.Reaction is { } reaction)
        {
            var slot=pending.Slots![reaction.SourceIndex];
            heading=Text("FINANCIAL_FAILURE_NAMED").Replace("{name}",EmployeeName(new EmployeeState {Name=slot.Name,SourceIndex=slot.SourceIndex}));
            speaker=Text("FINANCIAL_FAILURE_EMPLOYEE");portrait=slot.PortraitId;
            body=Text(FinancialFailureService.ReactionKey(reaction.Variant));
        }
        else body=Text(phase==3 ? "FINANCIAL_FAILURE_CLOSING" : $"FINANCIAL_FAILURE_INTRO_{phase}");
        ShowConversation(heading,[new(speaker,body,portrait)],()=>
        {
            if (!ReferenceEquals(_session.Current,state) || !ReferenceEquals(state.FinancialFailure,pending) || pending.Phase!=phase || pending.NextSource!=source) return;
            _session.AcknowledgeFinancialFailure();ShowOfficeReconstruction(state);
        });
    
        return true;
    }
}
