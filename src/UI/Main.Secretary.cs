using Godot;
using OpenGDS2.Domain;
using OpenGDS2.Core;

namespace OpenGDS2.UI;

public partial class Main
{
    private string SecretaryName(int? source)
    {
        if (source is not (>=0 and <=4)) return DialogueText("CORE_SECRETARY");
        if (TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase)) return Tr($"SECRETARY_NAME_{source}");
        return _activeCatalog?.Secretaries.SingleOrDefault(x=>x.SourceIndex==source)?.Name ?? DialogueText("CORE_SECRETARY");
    }
    private string SecretaryCaption(int? source)=>source is null ? DialogueText("CORE_SECRETARY") :
        DialogueText("SECRETARY_NAMED").Replace("{name}",SecretaryName(source));
    private void ShowSecretaryHiring(GameState state)
    {
        if (!DesktopActionEnabled("HIRE_SECRETARY")) {ShowOfficeReconstruction(state);return;}
        ClearContent();CompactManagementWindow("SecretaryHiringWindow",new Vector2(440,298),424);
        AddHeading(Tr("HIRE_SECRETARY"),16);
        if (!SecretaryManagementRules.HasCatalog(_activeCatalog) || state.SecretarySourceIndex is null)
        {
            _content.AddChild(new Label {Text=Tr("SECRETARY_CATALOG_REQUIRED"),AutowrapMode=TextServer.AutowrapMode.WordSmart});
            AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));return;
        }
        var rows=_activeCatalog!.Secretaries.Where(x=>x.SourceIndex!=state.SecretarySourceIndex).OrderBy(x=>x.SourceIndex).ToArray();
        var table=CreateTable("SecretaryCandidates",["EMPLOYEE_NAME","SECRETARY_RANK","MONTHLY_SALARY","CONTRACT_FEE"],[152,38,105,115]);
        table.CustomMinimumSize=new Vector2(0,122);_content.AddChild(table);
        var root=table.GetRoot();
        var description=new Label {Name="SecretaryDescription",AutowrapMode=TextServer.AutowrapMode.WordSmart,CustomMinimumSize=new Vector2(0,45)};
        var portrait=new TextureRect {Name="SecretaryPreviewPortrait",CustomMinimumSize=new Vector2(64,64),
            TextureFilter=TextureFilterEnum.Nearest,ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered};
        var preview=new HBoxContainer();preview.AddChild(portrait);preview.AddChild(description);_content.AddChild(preview);
        description.SizeFlagsHorizontal=SizeFlags.ExpandFill;
        var hire=new Button {Name="HireSelectedSecretary",Text=Tr("HIRE_SECRETARY"),Disabled=true};_content.AddChild(hire);
        SecretaryDefinition? selected=null;
        foreach (var candidate in rows)
        {
            var item=table.CreateItem(root);item.SetMetadata(0,candidate.SourceIndex);
            item.SetText(0,SecretaryName(candidate.SourceIndex));item.SetText(1,candidate.Rank);
            item.SetText(2,candidate.AdvertisedSalary.ToString("N0"));item.SetText(3,candidate.ContractFee.ToString("N0"));
        }
        table.ItemSelected+=()=>
        {
            var item=table.GetSelected();if (item is null) return;
            selected=rows.Single(x=>x.SourceIndex==(int)item.GetMetadata(0));
            description.Text=TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase)
                ? Tr($"SECRETARY_DESCRIPTION_{selected.SourceIndex}") : selected.Description;
            portrait.Texture=OriginalAssetStore.LoadEmployeePortrait(selected.PortraitId);
            hire.Disabled=SecretaryManagementRules.CheckHire(state,_activeCatalog,selected.SourceIndex)!=SecretaryHireResult.Success;
            hire.TooltipText=hire.Disabled ? Tr("SECRETARY_HIRE_UNAVAILABLE") : Tr("SECRETARY_HIRE_FEE_HELP");
        };
        hire.Pressed+=()=>
        {
            if (selected is null) return;
            var result=SecretaryManagementRules.TryHire(state,_activeCatalog,selected.SourceIndex);
            if (result==SecretaryHireResult.Success) ShowOfficeReconstruction(state);
            else {ShowSecretaryHiring(state);AddHeading(Tr($"SECRETARY_HIRE_{result}"),12);}
        };
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
    }
    private bool ShowPendingSecretaryReplacement(GameState state)
    {
        if (state.PendingSecretaryReplacement is not { } pending) return false;
        int index=state.SecretarySourceIndex!.Value;
        bool farewell=pending.Phase==SecretaryReplacementPhase.Farewell;
        ShowConversation(DialogueText("CORE_SECRETARY"),[new(SecretaryCaption(index),
            DialogueText(farewell ? "SECRETARY_FAREWELL" : "SECRETARY_GREETING").Replace("{name}",SecretaryName(index)),
            SecretaryAdviceRules.PortraitId(index))],()=>
        {
            if (!ReferenceEquals(state.PendingSecretaryReplacement,pending)) return;
            SecretaryManagementRules.Acknowledge(state);ShowOfficeReconstruction(state);
        });
        return true;
    }
    private void ShowSecretaryPolicies(GameState state)
    {
        if (!DesktopActionEnabled("SECRETARY_POLICIES")) {ShowOfficeReconstruction(state);return;}
        ClearContent();CompactManagementWindow("SecretaryPolicyWindow",new Vector2(440,230),424);
        AddHeading(Tr("SECRETARY_POLICIES"),16);
        if (state.SecretaryPolicies is null)
            _content.AddChild(new Label {Text=Tr("SECRETARY_POLICIES_UNKNOWN"),AutowrapMode=TextServer.AutowrapMode.WordSmart});
        var columns=new HBoxContainer();columns.AddThemeConstantOverride("separation",10);_content.AddChild(columns);
        var confirm=new Button {Name="ConfirmSecretaryPolicies",Text=Tr("CONFIRM"),Disabled=true};
        var production=new ButtonGroup();var entry=new ButtonGroup();var returning=new ButtonGroup();
        var returnControls=new List<CheckBox>();
        void Refresh()
        {
            var b=entry.GetPressedButton();bool voluntary=b is not null && (int)b.GetMeta("policy")==0;
            foreach (var control in returnControls) control.Disabled=voluntary && returning.GetPressedButton() is not null;
            confirm.Disabled=production.GetPressedButton() is null || b is null || returning.GetPressedButton() is null;
        }
        void Column(string heading,string prefix,int count,int? current,ButtonGroup group,bool returnColumn=false)
        {
            var column=new VBoxContainer {SizeFlagsHorizontal=SizeFlags.ExpandFill,CustomMinimumSize=new Vector2(125,0)};columns.AddChild(column);
            column.AddChild(new Label {Text=Tr(heading),AutowrapMode=TextServer.AutowrapMode.WordSmart});
            for (int i=0;i<count;i++)
            {
                var option=new CheckBox {Name=$"Secretary{prefix}{i}",Text=Tr($"SECRETARY_POLICY_{prefix}_{i}"),ButtonGroup=group,ButtonPressed=current==i};
                option.SetMeta("policy",i);column.AddChild(option);option.Toggled+=_=>Refresh();
                if (returnColumn) returnControls.Add(option);
            }
        }
        Column("PRODUCTION_CAPACITY","Production",3,state.SecretaryPolicies is { } p ? (int)p.Production : null,production);
        Column("SECRETARY_BREAK_ENTRY","Break",4,state.SecretaryPolicies is { } b ? (int)b.Break : null,entry);
        Column("SECRETARY_BREAK_RETURN","Return",3,state.SecretaryPolicies is { } r ? (int)r.Return : null,returning,true);
        _content.AddChild(confirm);Refresh();
        confirm.Pressed+=()=>
        {
            var policy=new SecretaryPolicyState {Production=(ProductionPolicy)(int)production.GetPressedButton().GetMeta("policy"),
                Break=(BreakPolicy)(int)entry.GetPressedButton().GetMeta("policy"),Return=(ReturnPolicy)(int)returning.GetPressedButton().GetMeta("policy")};
            if (SecretaryPolicyRules.TrySet(state,policy)) ShowOfficeReconstruction(state);
        };
        AddAction("BACK_TO_OFFICE",()=>ShowOfficeReconstruction(state));
    }
}
