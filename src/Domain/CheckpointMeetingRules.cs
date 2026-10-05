namespace OpenGDS2.Domain;

// Stable presentation IDs for modern saves; no original queue format is implied.
public enum CheckpointMeetingMessage
{
    TypeSuggestion=0,TypeFits=1,TypeHit=2,TypeAudience=3,TypeReaders=4,
    TypeChosen=5,Publicity=6,BetaComplete=7,RecognitionLow=8,RecognitionMedium=9,
    RecognitionHigh=10,ChooseGenre=11,TypeReminder=12,GenreMatch=13,GenreChosen=14,FunBonus=15
}
public sealed record CheckpointMeetingLine(CheckpointMeetingMessage Message,int? TypeSourceIndex=null);
public sealed class CheckpointMeetingState
{
    public DevelopmentMilestone Milestone {get;set;}
    public bool IsConfirmation {get;set;}
    public List<CheckpointMeetingLine> Lines {get;set;}=[];
    public int MessageIndex {get;set;}
}
public static class CheckpointMeetingRules
{
    public static bool HasPending(DevelopmentProjectState project)
        =>project.CheckpointMeeting is { } meeting && meeting.MessageIndex<meeting.Lines.Count;
    public static string MessageKey(CheckpointMeetingMessage message)
        =>Enum.IsDefined(message) ? $"CHECKPOINT_MEETING_{message}" : throw new ArgumentOutOfRangeException(nameof(message));
    public static bool IsSecretary(CheckpointMeetingMessage message)=>message==CheckpointMeetingMessage.FunBonus;
    public static void Prepare(GameState state,DevelopmentProjectState project,OriginalCatalog catalog,Random random)
    {
        if (!state.DevelopmentProjects.Contains(project) || project.OriginalMode is not (0 or 2) ||
            project.PendingDecision is not (DevelopmentMilestone.SelectType or DevelopmentMilestone.SelectGenre))
            throw new InvalidOperationException("No pending checkpoint meeting.");
        if (project.CheckpointMeeting is not null) return;
        var meeting=new CheckpointMeetingState {Milestone=project.PendingDecision.Value};
        if (meeting.Milestone==DevelopmentMilestone.SelectType)
        {
            var candidates=DevelopmentSimulation.PrepareTypeCandidates(state,project,catalog,random);
            foreach (int source in candidates)
            {
                // +060D..064F evaluates the random roll even for a short list.
                int speak=random.Next(3);
                if (candidates.Count>4 && speak>1) continue;
                int variant=random.Next(6);
                CheckpointMeetingMessage? message=variant switch
                {0=>CheckpointMeetingMessage.TypeSuggestion,1=>CheckpointMeetingMessage.TypeFits,2=>CheckpointMeetingMessage.TypeHit,
                    4=>CheckpointMeetingMessage.TypeAudience,5=>CheckpointMeetingMessage.TypeReaders,_=>null};
                // Source variant 3 is silent; duplicate candidates remain duplicated.
                if (message.HasValue) meeting.Lines.Add(new(message.Value,source));
            }
        }
        else
        {
            meeting.Lines=[new(CheckpointMeetingMessage.BetaComplete),
                new(project.Recognition<=10 ? CheckpointMeetingMessage.RecognitionLow : project.Recognition<=30 ? CheckpointMeetingMessage.RecognitionMedium : CheckpointMeetingMessage.RecognitionHigh),
                new(CheckpointMeetingMessage.ChooseGenre),new(CheckpointMeetingMessage.TypeReminder,project.GameTypeSourceIndex)];
        }
        project.CheckpointMeeting=meeting;
    }
    public static bool Acknowledge(GameState state,DevelopmentProjectState project)
    {
        if (!state.DevelopmentProjects.Contains(project) || !HasPending(project)) return false;
        var meeting=project.CheckpointMeeting!;meeting.MessageIndex++;
        if (meeting.IsConfirmation && !HasPending(project)) project.CheckpointMeeting=null;
        return true;
    }
    public static DevelopmentSelectionResult TrySelectType(GameState state,DevelopmentProjectState project,OriginalCatalog catalog,int source)
    {
        if (!state.DevelopmentProjects.Contains(project) || HasPending(project) || project.PendingDecision!=DevelopmentMilestone.SelectType ||
            project.TypeCandidates?.Contains(source)!=true) return DevelopmentSelectionResult.WrongStage;
        var result=DevelopmentSelectionRules.TrySelectType(state,project,catalog,source,state.OriginalTypeRosterMap!);
        if (result==DevelopmentSelectionResult.Success) project.CheckpointMeeting=new()
        {Milestone=DevelopmentMilestone.SelectType,IsConfirmation=true,Lines=[new(CheckpointMeetingMessage.TypeChosen,source),new(CheckpointMeetingMessage.Publicity)]};
        return result;
    }
    public static DevelopmentSelectionResult TrySelectGenre(GameState state,DevelopmentProjectState project,OriginalCatalog catalog,int source)
    {
        if (!state.DevelopmentProjects.Contains(project) || HasPending(project) || project.PendingDecision!=DevelopmentMilestone.SelectGenre)
            return DevelopmentSelectionResult.WrongStage;
        var genre=catalog.Genres.SingleOrDefault(x=>x.SourceIndex==source) ?? throw new ArgumentOutOfRangeException(nameof(source));
        if (state.Company.OriginalMember90 is not int gate || !DevelopmentSelectionRules.IsGenreEligible(genre,gate)) return DevelopmentSelectionResult.WrongStage;
        var type=catalog.GameTypes.Single(x=>x.SourceIndex==project.GameTypeSourceIndex);
        bool affinity=catalog.Audiences.Single(x=>x.SourceIndex==project.AudienceSourceIndex).Identifier>1 && type.RawTable13CC.Contains(genre.Identifier);
        var result=DevelopmentSelectionRules.TrySelectGenre(state,project,catalog,source);
        if (result==DevelopmentSelectionResult.Success) project.CheckpointMeeting=new()
        {Milestone=DevelopmentMilestone.SelectGenre,IsConfirmation=true,Lines=affinity
            ? [new(CheckpointMeetingMessage.GenreMatch,project.GameTypeSourceIndex),new(CheckpointMeetingMessage.FunBonus)]
            : [new(CheckpointMeetingMessage.GenreChosen,project.GameTypeSourceIndex)]};
        return result;
    }
    public static void Validate(DevelopmentProjectState project)
    {
        if (project.CheckpointMeeting is not { } meeting) return;
        if (project.OriginalMode is not (0 or 2) || meeting.Milestone is not (DevelopmentMilestone.SelectType or DevelopmentMilestone.SelectGenre) ||
            meeting.Lines is null || meeting.Lines.Count>65 || meeting.MessageIndex<0 || meeting.MessageIndex>meeting.Lines.Count ||
            meeting.IsConfirmation && (!HasPending(project) || project.PendingDecision.HasValue || project.OriginalStage!=(meeting.Milestone==DevelopmentMilestone.SelectType ? 1 : 2)) ||
            !meeting.IsConfirmation && project.PendingDecision!=meeting.Milestone ||
            meeting.Lines.Any(x=>x is null || !Enum.IsDefined(x.Message) || x.TypeSourceIndex is <0 or >50 ||
                (NeedsType(x.Message)!=x.TypeSourceIndex.HasValue))) throw new InvalidDataException("Invalid checkpoint meeting.");
        if (meeting.IsConfirmation)
        {
            var expected=meeting.Milestone==DevelopmentMilestone.SelectType
                ? new[]{CheckpointMeetingMessage.TypeChosen,CheckpointMeetingMessage.Publicity}
                : meeting.Lines.FirstOrDefault()?.Message==CheckpointMeetingMessage.GenreMatch
                    ? new[]{CheckpointMeetingMessage.GenreMatch,CheckpointMeetingMessage.FunBonus} : new[]{CheckpointMeetingMessage.GenreChosen};
            if (!meeting.Lines.Select(x=>x.Message).SequenceEqual(expected) || meeting.Lines.Any(x=>x.TypeSourceIndex.HasValue && x.TypeSourceIndex!=project.GameTypeSourceIndex))
                throw new InvalidDataException("Invalid checkpoint confirmation.");
        }
        else if (meeting.Milestone==DevelopmentMilestone.SelectType)
        {
            if (project.TypeCandidates is null || meeting.Lines.Any(x=>(int)x.Message>4 || !project.TypeCandidates.Contains(x.TypeSourceIndex!.Value)))
                throw new InvalidDataException("Invalid type meeting suggestions.");
        }
        else if (meeting.Lines.Count!=4 || meeting.Lines[0].Message!=CheckpointMeetingMessage.BetaComplete ||
            meeting.Lines[1].Message is not (CheckpointMeetingMessage.RecognitionLow or CheckpointMeetingMessage.RecognitionMedium or CheckpointMeetingMessage.RecognitionHigh) ||
            meeting.Lines[2].Message!=CheckpointMeetingMessage.ChooseGenre || meeting.Lines[3]!=new CheckpointMeetingLine(CheckpointMeetingMessage.TypeReminder,project.GameTypeSourceIndex))
            throw new InvalidDataException("Invalid genre meeting discussion.");
    }
    private static bool NeedsType(CheckpointMeetingMessage message)=>(int)message<=5 || message is
        CheckpointMeetingMessage.TypeReminder or CheckpointMeetingMessage.GenreMatch or CheckpointMeetingMessage.GenreChosen;
}
