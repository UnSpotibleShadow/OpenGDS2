namespace OpenGDS2.Domain;
public sealed class LevelUpPresentationState
{
    public int EmployeeSourceIndex {get;set;}
    public string Name {get;set;}=string.Empty;
    public int? PortraitId {get;set;}
    public int Level {get;set;}
    public int[] Gains {get;set;}=[];
    public bool SuperBonus {get;set;}
}
