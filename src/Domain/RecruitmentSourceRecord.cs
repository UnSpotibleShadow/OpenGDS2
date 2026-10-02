namespace OpenGDS2.Domain;

/// <summary>
/// One source row from the original global recruitment arrays. SourceIndex is retained
/// because the VB6 game stores each field in a separate parallel array.
/// GateValue is deliberately named structurally until its wider game meaning is decoded.
/// </summary>
public sealed class RecruitmentSourceRecord
{
    public required int SourceIndex { get; init; }
    public required int GateValue { get; set; }
    public required EmployeeState Employee { get; init; }
}
