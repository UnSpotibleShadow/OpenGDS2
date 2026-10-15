using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private Control ManagementCanvas(string name,string title,Vector2 client)
    {
        ClearContent();if(_dialog is not null)return NativeWindow(name,title,client);
        _content.CustomMinimumSize=client;_content.AddThemeConstantOverride("separation",2);AddHeading(title,14);
        var canvas=new Control {Name="NativeDialogCanvas",CustomMinimumSize=client};_content.AddChild(canvas);return canvas;
    }
    private void EmployeePreview(Control canvas,EmployeeState employee,string prefix,Rect2 bounds)
    {
        var host=NativePlace(canvas,new Control {Name=prefix+"Details"},bounds);
        NativePlace(host,Picture("EmployeePortrait",OriginalAssetStore.LoadEmployeePortrait(employee.PortraitId),new(64,64)),new(4,4,64,64));
        NativeText(host,prefix+"Name",EmployeeName(employee),new(4,72,210,20),10);
        NativeText(host,prefix+"Level",$"{Tr("LEVEL")}: {employee.Level?.ToString()??"?"}/10",new(4,95,190,18),10);
        NativeText(host,prefix+"Fee",$"{Tr("CONTRACT_FEE")}: ¥{employee.ContractFee?.ToString("N0")??"?"}",new(4,116,bounds.Size.X-8,20),10);
        NativePlace(host,new EmployeeAbilityChart {Name=prefix+"AbilityChart",Employee=employee,Compact=true,MouseFilter=MouseFilterEnum.Ignore},new(216,3,150,124));
        NativeText(host,prefix+"Speed",$"{Tr("STAT_DEVELOPMENT_SPEED")}: {employee.DevelopmentSpeed?.ToString()??"?"}",new(4,139,206,18),10);
        NativeText(host,prefix+"Inspiration",$"{Tr("STAT_INSPIRATION")}: {employee.Inspiration?.ToString()??"?"}",new(216,139,bounds.Size.X-220,18),10);
        NativeText(host,prefix+"Salary",$"{Tr("MONTHLY_SALARY")}: {employee.MonthlySalary?.ToString("N0")??"?"}",new(76,4,134,45),10).AutowrapMode=TextServer.AutowrapMode.WordSmart;
    }
    private void ShowNativeRecruitment(GameState state,RecruitmentState roster)
    {
        var canvas=ManagementCanvas("RecruitmentWindow",Tr("HIRE_EMPLOYEES"),new(478,378));
        NativeText(canvas,"RecruitmentFunds",$"{Tr("FUNDS")}: ¥{state.Money:N0}",new(4,3,470,20));
        var table=ManagementTable(canvas,"RecruitmentTable",["EMPLOYEE_NAME","EMPLOYEE_LEVEL","SHORT_FUN","SHORT_GRAPHICS","SHORT_NOVELTY","SHORT_MUSIC","SHORT_ACCURACY","SHORT_CONTRACT_FEE"],[125,27,24,24,24,24,24,135],new(4,27,470,136));
        var preview=NativePlace(canvas,new Control {Name="RecruitmentPreview"},new(4,168,470,163));
        _status=NativeText(canvas,"RecruitmentStatus","",new(4,334,470,16),10);
        EmployeeState? selected=null;
        var hire=NativeAction(canvas,"HireSelectedCandidate","HIRE_CONFIRM",new(262,351,106,25),()=>{},false);
        NativeAction(canvas,"CancelRecruitment","CANCEL",new(374,351,100,25),()=>ShowOfficeReconstruction(state));
        foreach(var candidate in roster.Candidates)
        {
            var row=table.CreateItem(table.GetRoot());row.SetMetadata(0,roster.Candidates.IndexOf(candidate));
            string[] values=[EmployeeName(candidate),candidate.Level?.ToString()??"?",candidate.Fun?.ToString()??"?",candidate.Graphics?.ToString()??"?",candidate.Novelty?.ToString()??"?",candidate.Music?.ToString()??"?",candidate.Accuracy?.ToString()??"?",candidate.ContractFee?.ToString("N0")??"?"];
            for(int c=0;c<values.Length;c++){row.SetText(c,values[c]);if(c>0)row.SetTextAlignment(c,HorizontalAlignment.Right);}
        }
        void Select()
        {
            if(table.GetSelected() is not { } row)return;int index=row.GetMetadata(0).AsInt32();if(index<0||index>=roster.Candidates.Count)return;selected=roster.Candidates[index];
            foreach(Node child in preview.GetChildren()){preview.RemoveChild(child);child.QueueFree();}
            EmployeePreview(preview,selected,"Recruitment",new(0,0,470,163));hire.Disabled=selected.ContractFee is not long fee||state.Money is not long money||money<fee;
        }
        table.ItemSelected+=Select;table.GetRoot().GetFirstChild()?.Select(0);Select();
        if(roster.Candidates.Count==0)_status.Text=Tr("NO_APPLICANTS");
        hire.Pressed+=()=>{if(hire.Disabled||selected is null)return;var result=_session.Hire(selected);if(result==HireResult.Success)ShowOfficeReconstruction(state);else _status.Text=Tr("HIRE_FAILED")+" "+Tr("HIRE_RESULT_"+result);};
    }
}
