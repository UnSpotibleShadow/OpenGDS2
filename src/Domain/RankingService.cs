using System.Text.Json.Serialization;

namespace OpenGDS2.Domain;

public enum RankingKind { Sales250000, Sales500000, Sales1000000, OwnHardware, GrandPrize, NumberOneSales }
public sealed class RankingAchievement
{
    [JsonRequired] public RankingKind Kind {get;set;}
    [JsonRequired] public string Company {get;set;}=string.Empty;
    [JsonRequired] public GameDate AchievedOn {get;set;}
}
public sealed class RankingRecord
{
    public string Company {get;set;}=string.Empty;
    public GameDate? AchievedOn {get;set;}
    public string OriginalGameDateText {get;set;}=string.Empty;
    public string RecordedOn {get;set;}=string.Empty;
}
public sealed class RankingBook
{
    [JsonRequired] public int SchemaVersion {get;set;}=1;
    [JsonRequired] public List<RankingRecord> Records {get;set;}=Enumerable.Range(0,6).Select(_=>new RankingRecord()).ToList();
}
/// <summary>Six local cross-company earliest-achievement records, independent of session saves.</summary>
public static class RankingService
{
    public static bool Earlier(GameDate date,GameDate previous)=>date.Year<previous.Year || date.Year==previous.Year &&
        (date.Month<previous.Month || date.Month==previous.Month && date.Week<previous.Week);
    public static void Queue(GameState state,RankingKind kind)
    {
        if (state.Year is not int y || state.Month is not int m || state.Week is not int w) return;
        var date=new GameDate(y,m,w);
        if (state.PendingRankings.Any(p=>p.Kind==kind && !Earlier(date,p.AchievedOn))) return;
        state.PendingRankings.RemoveAll(p=>p.Kind==kind);
        state.PendingRankings.Add(new() {Kind=kind,Company=state.Company.Name,AchievedOn=date});
    }
    public static bool Record(RankingBook book,RankingAchievement achievement,string recordedOn)
    {
        Validate(book);Validate(achievement);
        var record=book.Records[(int)achievement.Kind];
        if (record.AchievedOn is GameDate previous && !Earlier(achievement.AchievedOn,previous)) return false;
        record.Company=achievement.Company;record.AchievedOn=achievement.AchievedOn;
        record.OriginalGameDateText=string.Empty;record.RecordedOn=recordedOn;return true;
    }
    public static void Validate(RankingAchievement achievement)
    {
        if (achievement is null || !Enum.IsDefined(achievement.Kind) || string.IsNullOrWhiteSpace(achievement.Company)) throw new InvalidDataException("Invalid ranking achievement.");
        AnnualCalendarValidation.Date(achievement.AchievedOn);
    }
    public static void Validate(RankingBook book)
    {
        if (book.SchemaVersion!=1 || book.Records is null || book.Records.Count!=6 || book.Records.Any(r=>r is null || r.Company is null || r.RecordedOn is null || r.OriginalGameDateText is null))
            throw new InvalidDataException("Invalid ranking profile.");
        foreach (var record in book.Records)
            if (record.AchievedOn is GameDate date)
            {AnnualCalendarValidation.Date(date);if (string.IsNullOrWhiteSpace(record.Company)) throw new InvalidDataException("Missing ranking company.");}
    }
}
