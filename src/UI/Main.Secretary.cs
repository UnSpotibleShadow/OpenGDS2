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
        ClearContent();var canvas=NativeWindow("SecretaryHiringWindow",Tr("HIRE_SECRETARY"),new(390,259));
        if (!SecretaryManagementRules.HasCatalog(_activeCatalog) || state.SecretarySourceIndex is null)
        {
            NativeText(canvas,"SecretaryCatalogRequired",Tr("SECRETARY_CATALOG_REQUIRED"),new(4,4,382,60)).AutowrapMode=TextServer.AutowrapMode.WordSmart;
            NativeAction(canvas,"CancelSecretaryHiring","BACK_TO_OFFICE",new(273,229,113,25),()=>ShowOfficeReconstruction(state));return;
        }
        var rows=_activeCatalog!.Secretaries.Where(x=>x.SourceIndex!=state.SecretarySourceIndex).OrderBy(x=>x.SourceIndex).ToArray();
        NativeText(canvas,"SecretaryHiringInstruction",Tr("HIRE_SECRETARY"),new(4,4,382,20),11,HorizontalAlignment.Center);
        var table=ManagementTable(canvas,"SecretaryCandidates",["EMPLOYEE_NAME","CONTRACT_FEE","MONTHLY_SALARY"],[145,113,110],new(4,28,382,108));
        var root=table.GetRoot();
        NativeFill(canvas,"SecretaryDescriptionBox",new(78,161,308,57),Colors.White,true);
        var description=NativeText(canvas,"SecretaryDescription","",new(81,164,302,51),10);description.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var selectedName=NativeText(canvas,"SecretaryPreviewName","",new(78,139,308,20));
        var portrait=NativePlace(canvas,Picture("SecretaryPreviewPortrait",null,new(64,64)),new(4,141,64,64));
        var hire=NativeAction(canvas,"HireSelectedSecretary","HIRE_SECRETARY",new(118,229,148,25),()=>{},false);
        NativeAction(canvas,"CancelSecretaryHiring","CANCEL",new(273,229,113,25),()=>ShowOfficeReconstruction(state));
        SecretaryDefinition? selected=null;
        foreach (var candidate in rows)
        {
            var item=table.CreateItem(root);item.SetMetadata(0,candidate.SourceIndex);
            item.SetText(0,SecretaryName(candidate.SourceIndex));
            item.SetText(1,candidate.ContractFee.ToString("N0"));item.SetText(2,candidate.AdvertisedSalary.ToString("N0"));
            item.SetTextAlignment(1,HorizontalAlignment.Right);item.SetTextAlignment(2,HorizontalAlignment.Right);
        }
        table.ItemSelected+=()=>
        {
            var item=table.GetSelected();if (item is null) return;
            selected=rows.Single(x=>x.SourceIndex==(int)item.GetMetadata(0));
            description.Text=TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase)
                ? Tr($"SECRETARY_DESCRIPTION_{selected.SourceIndex}") : selected.Description;
            selectedName.Text=SecretaryName(selected.SourceIndex);
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
        ClearContent();var canvas=NativeWindow("SecretaryPolicyWindow",Tr("SECRETARY_POLICIES"),new(420,state.SecretaryPolicies is null?334:300));
        if (state.SecretaryPolicies is null)
            NativeText(canvas,"SecretaryPoliciesUnknown",Tr("SECRETARY_POLICIES_UNKNOWN"),new(4,302,412,28),10).AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var confirm=NativeAction(canvas,"ConfirmSecretaryPolicies","CONFIRM",new(316,233,100,25),()=>{},false);
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
            var rect=prefix=="Production"?new Rect2(4,4,412,81):prefix=="Break"?new Rect2(4,92,302,99):new Rect2(4,198,302,81);
            NativeFill(canvas,"Secretary"+prefix+"Frame",rect,new(.83f,.83f,.83f),true);
            NativeText(canvas,"Secretary"+prefix+"Heading",Tr(heading),new(rect.Position.X+7,rect.Position.Y,rect.Size.X-14,19),10);
            var column=NativePlace(canvas,new VBoxContainer(),new(rect.Position.X+7,rect.Position.Y+20,rect.Size.X-14,rect.Size.Y-22));
            column.AddThemeConstantOverride("separation",0);
            for (int i=0;i<count;i++)
            {
                var option=new CheckBox {Name=$"Secretary{prefix}{i}",Text=Tr($"SECRETARY_POLICY_{prefix}_{i}"),ButtonGroup=group,ButtonPressed=current==i};
                foreach(string mode in new[]{"normal","pressed","hover","disabled","focus","hover_pressed"})option.AddThemeStyleboxOverride(mode,new StyleBoxEmpty());
                option.AddThemeFontSizeOverride("font_size",10);option.SetMeta("policy",i);column.AddChild(option);option.Toggled+=_=>Refresh();
                if (returnColumn) returnControls.Add(option);
            }
        }
        Column("PRODUCTION_CAPACITY","Production",3,state.SecretaryPolicies is { } p ? (int)p.Production : null,production);
        Column("SECRETARY_BREAK_ENTRY","Break",4,state.SecretaryPolicies is { } b ? (int)b.Break : null,entry);
        Column("SECRETARY_BREAK_RETURN","Return",3,state.SecretaryPolicies is { } r ? (int)r.Return : null,returning,true);
        Refresh();
        confirm.Pressed+=()=>
        {
            var policy=new SecretaryPolicyState {Production=(ProductionPolicy)(int)production.GetPressedButton().GetMeta("policy"),
                Break=(BreakPolicy)(int)entry.GetPressedButton().GetMeta("policy"),Return=(ReturnPolicy)(int)returning.GetPressedButton().GetMeta("policy")};
            if (SecretaryPolicyRules.TrySet(state,policy)) ShowOfficeReconstruction(state);
        };
        NativeAction(canvas,"SecretaryPolicyAdvice","SECRETARY_ADVICE",new(316,194,100,25),()=>{ShowOfficeReconstruction(state);ShowSecretaryAdvice(state);});
        NativeAction(canvas,"CancelSecretaryPolicies","CANCEL",new(316,267,100,25),()=>ShowOfficeReconstruction(state));
    }
}
