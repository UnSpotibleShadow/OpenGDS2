using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowComputerPurchase(GameState state,int? selectedSource=null)
    {
        ClearContent();var canvas=NativeWindow("ComputerPurchaseWindow",Tr("DEVELOPMENT_COMPUTERS"),new(298,244));
        var catalog=_importer.LoadCatalog()!;var computers=catalog.DevelopmentComputers.OrderBy(c=>c.SourceIndex).ToList();
        NativeFill(canvas,"ComputerInstructionBox",new(4,2,290,21),Colors.White,true);
        NativeText(canvas,"ComputerInstruction",Tr("COMPUTER_BUY"),new(6,2,286,21),10,HorizontalAlignment.Center);
        var table=CreateTable("ComputerPurchaseTable",["EMPLOYEE_NAME","COMPUTER_PROCESSING","PURCHASE_PRICE"],[118,48,88]);table.ScrollHorizontalEnabled=false;table.AddThemeFontSizeOverride("title_button_font_size",10);
        NativePlace(canvas,table,new(4,27,290,107));StyleSelection(table,new(.72f,1,.72f));
        var preview=NativePlace(canvas,Picture("ComputerPurchasePreview",null,new(64,64)),new(4,136,64,64));
        NativeFill(canvas,"ComputerDescriptionHeading",new(74,134,220,19),new(.72f,1,.72f));
        var name=NativeText(canvas,"ComputerSelectedName","",new(76,134,216,19),10);
        var description=new RichTextLabel {Name="ComputerDescription",ScrollActive=true};description.AddThemeFontSizeOverride("normal_font_size",10);NativeFill(canvas,"ComputerDescriptionBox",new(74,155,220,45),Colors.White,true);NativePlace(canvas,description,new(77,157,214,41));
        _status=NativeText(canvas,"ComputerPurchaseStatus","",new(4,202,290,17),10);
        var buy=NativeAction(canvas,"ConfirmComputerPurchase","COMPUTER_BUY",new(82,220,90,22),()=>{});
        NativeAction(canvas,"CancelComputerPurchase","CANCEL",new(179,220,115,22),()=>ShowOfficeReconstruction(state));
        int? selected=null;
        void Select()
        {
            var row=table.GetSelected();if(row is null)return;selected=row.GetMetadata(0).AsInt32();var computer=computers.Single(c=>c.SourceIndex==selected);
            preview.Texture=OriginalAssetStore.LoadTexture($"Graphic/Embedded/Computer{selected}.bmp");name.Text=CatalogName(catalog,"COMPUTER",computer.SourceIndex,computer.Name);description.Text=CatalogDescription(catalog,"COMPUTER",computer.SourceIndex,computer.Description);
            buy.Disabled=computer.SourceIndex==state.DevelopmentComputerIndex||state.DevelopmentComputerIndex is int current&&computers.Single(c=>c.SourceIndex==current).BitWidth>computer.BitWidth;
        }
        table.ItemSelected+=Select;
        foreach(var computer in computers)
        {
            var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,computer.SourceIndex);row.SetText(0,CatalogName(catalog,"COMPUTER",computer.SourceIndex,computer.Name));row.SetText(1,computer.BitWidth+" bit");row.SetText(2,$"¥{computer.PurchasePrice:N0}");row.SetTextAlignment(2,HorizontalAlignment.Right);
            if(computer.SourceIndex==(selectedSource??state.DevelopmentComputerIndex??computers[0].SourceIndex))row.Select(0);
        }
        Select();buy.Disabled|=computers.Count==0;buy.Pressed+=()=>{if(buy.Disabled||selected is not int source)return;var result=DevelopmentComputerService.TryPurchase(state,catalog,source);ShowComputerPurchase(state,source);_status.Text=Tr("COMPUTER_RESULT_"+result);};
    }
    private void ShowTechnologyPurchase(GameState state,int? selectedSource=null)
    {
        ClearContent();var canvas=NativeWindow("TechnologyPurchaseWindow",Tr("TECHNOLOGIES"),new(258,246));var catalog=_importer.LoadCatalog()!;_activeCatalog=catalog;
        var technologies=catalog.Technologies.OrderBy(t=>t.SourceIndex).ToList();
        var table=CreateTable("TechnologyPurchaseTable",["TECHNOLOGY_NAME","PURCHASE_PRICE"],[137,93]);table.ScrollHorizontalEnabled=false;table.AddThemeFontSizeOverride("title_button_font_size",10);StyleSelection(table,new(1,.68f,1));NativePlace(canvas,table,new(4,4,250,143));
        NativeFill(canvas,"TechnologyDescriptionBox",new(4,153,250,52),Colors.White,true);
        var description=new RichTextLabel {Name="TechnologyDescription",ScrollActive=true};description.AddThemeFontSizeOverride("normal_font_size",10);NativePlace(canvas,description,new(7,156,244,46));
        _status=NativeText(canvas,"TechnologyPurchaseStatus","",new(4,206,250,13),9);
        var buy=NativeAction(canvas,"ConfirmTechnologyPurchase","PURCHASE_TECHNOLOGY",new(74,219,80,22),()=>{});NativeAction(canvas,"CancelTechnologyPurchase","CANCEL",new(160,219,94,22),()=>ShowOfficeReconstruction(state));int? selected=null;
        void Select()
        {
            if(table.GetSelected() is not { } row)return;selected=row.GetMetadata(0).AsInt32();var technology=technologies.Single(t=>t.SourceIndex==selected);
            description.Text=CatalogDescription(catalog,"TECHNOLOGY",technology.SourceIndex,technology.Description);buy.Name="PurchaseTechnology"+selected;
            buy.Disabled=state.TechnologyOwnershipLevels is null||state.TechnologyOwnershipLevels.GetValueOrDefault(technology.SourceIndex)>=1;
        }
        table.ItemSelected+=Select;
        foreach(var technology in technologies){var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,technology.SourceIndex);row.SetText(0,TechnologyName(technology));row.SetText(1,$"¥{technology.PurchaseFee:N0}");row.SetTextAlignment(1,HorizontalAlignment.Right);if(technology.SourceIndex==(selectedSource??technologies[0].SourceIndex))row.Select(0);}
        Select();buy.Disabled|=technologies.Count==0;buy.Pressed+=()=>{if(buy.Disabled||selected is not int source)return;var result=TechnologyService.TryPurchase(state,technologies.Single(t=>t.SourceIndex==source));ShowTechnologyPurchase(state,source);_status.Text=Tr("TECHNOLOGY_PURCHASE_"+result);};
    }
    private string CatalogDescription(OriginalCatalog catalog,string kind,int source,string original)
    {
        if(!TranslationServer.GetLocale().StartsWith("en"))return original;
        return kind=="COMPUTER"?string.Format(Tr("COMPUTER_DESCRIPTION"),catalog.DevelopmentComputers.Single(c=>c.SourceIndex==source).BitWidth):string.Format(Tr("TECHNOLOGY_DESCRIPTION"),CatalogName(catalog,kind,source,catalog.Technologies.Single(t=>t.SourceIndex==source).Name));
    }
    private LevelUpResult LevelUpWithPresentation(GameState state,int source,IReadOnlyList<float> draws)
    {
        var employee=state.Employees.SingleOrDefault(e=>e.SourceIndex==source);
        int[] before=employee is null?[]:[employee.Fun??0,employee.Graphics??0,employee.Novelty??0,employee.Music??0,employee.Accuracy??0,employee.DevelopmentSpeed??0,employee.Inspiration??0];
        var result=EmployeeLevelUpService.TryLevelUp(state,source,draws);
        if(result==LevelUpResult.Success)
        {
            int[] after=[employee!.Fun!.Value,employee.Graphics!.Value,employee.Novelty!.Value,employee.Music!.Value,employee.Accuracy!.Value,employee.DevelopmentSpeed!.Value,employee.Inspiration!.Value];
            state.PendingLevelUpResult=new() {EmployeeSourceIndex=source,Name=EmployeeName(employee),PortraitId=employee.PortraitId,Level=employee.Level!.Value,Gains=after.Select((v,i)=>v-before[i]).ToArray(),SuperBonus=EmployeeProgressionRules.SuperBonus(draws[0])>0};
        }
        return result;
    }
    private bool ShowPendingLevelUp(GameState state)
    {
        if(state.PendingLevelUpResult is not { } pending)return false;
        ClearContent();var canvas=NativeWindow("LevelUpResultWindow",Tr("LEVEL_UP"),new(138,251));
        NativePlace(canvas,Picture("LevelUpPortrait",OriginalAssetStore.LoadEmployeePortrait(pending.PortraitId),new(64,64)),new(37,4,64,64));
        NativeText(canvas,"LevelUpName",pending.Name,new(3,69,132,22),10,HorizontalAlignment.Center);
        NativeFill(canvas,"LevelUpGainsPanel",new(4,95,130,126),new(.72f,1,.72f));
        string[] keys=["STAT_FUN","STAT_GRAPHICS","STAT_NOVELTY","STAT_MUSIC","STAT_ACCURACY","STAT_DEVELOPMENT_SPEED","STAT_INSPIRATION"];
        for(int i=0;i<7;i++){NativeText(canvas,"LevelUpStat"+i,Tr(keys[i]),new(6,95+i*18,99,18),10);NativeText(canvas,"LevelUpGain"+i,"+"+pending.Gains[i],new(105,95+i*18,26,18),10,HorizontalAlignment.Right);}
        NativeText(canvas,"LevelUpBonus",pending.SuperBonus?Tr("LEVEL_UP_SUPER"):"",new(4,224,130,22),10,HorizontalAlignment.Center);
        _dialog!.CloseRequested=()=>{state.PendingLevelUpResult=null;ShowOfficeReconstruction(state);};return true;
    }
}
