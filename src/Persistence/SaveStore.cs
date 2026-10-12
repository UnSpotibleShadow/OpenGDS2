using System.Text.Json;
using OpenGDS2.Domain;
namespace OpenGDS2.Persistence;

/// <summary>Modern save format. Original VB6 save compatibility remains unresolved.</summary>
public static class SaveStore
{
    private const int MaxBytes = 4 * 1024 * 1024;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, MaxDepth = 48 };
    private sealed class Envelope
    {
        public Envelope() { }
        public string? Format { get; set; }
        public int SchemaVersion { get; set; }
        public GameState? State { get; set; }
    }
    public static string Serialize(GameState state)
    {
        Validate(state);
        string text = JsonSerializer.Serialize(new Envelope { Format = "OpenGDS2", SchemaVersion = 1, State = state }, Options);
        // A successful write must remain readable by the same format's loader.
        // Reject before Save creates a temporary file or replaces an old slot.
        if (System.Text.Encoding.UTF8.GetByteCount(text) > MaxBytes)
            throw new InvalidDataException("Save is too large.");
        return text;
    }
    public static GameState Deserialize(string text)
    {
        if (System.Text.Encoding.UTF8.GetByteCount(text) > MaxBytes) throw new InvalidDataException("Save is too large.");
        Envelope save;
        try { save = JsonSerializer.Deserialize<Envelope>(text, Options) ?? throw new InvalidDataException("Empty save."); }
        catch (JsonException ex) { throw new InvalidDataException("Malformed save.", ex); }
        if (save.Format != "OpenGDS2" || save.SchemaVersion != 1 || save.State is null)
            throw new InvalidDataException("Unsupported save format or version.");
        Validate(save.State);
        // Derived mode rules cannot be overwritten by a malformed/stale save.
        save.State.Rules = GameModeRules.For(save.State.Mode);
        return save.State;
    }
    public static void Save(string path, GameState state)
    {
        if (FinancialFailureService.IsComplete(state)) throw new InvalidOperationException("Saving is disabled after financial failure.");
        string text = Serialize(state);
        string absolute = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);
        string temp = absolute + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(text); stream.Write(bytes); stream.Flush(true);
            }
            if (File.Exists(absolute) && IsReadableSave(absolute))
                File.Replace(temp, absolute, BackupPath(absolute));
            else
                // Keep a valid previous save when replacing a damaged current slot.
                File.Move(temp, absolute, true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
    public static GameState Load(string path)
    {
        if (new FileInfo(path).Length > MaxBytes) throw new InvalidDataException("Save is too large.");
        return Deserialize(File.ReadAllText(path));
    }
    public static string BackupPath(string path) => Path.GetFullPath(path) + ".bak";
    public static GameState LoadBackup(string path) => Load(BackupPath(path));
    private static bool IsReadableSave(string path)
    {
        try { Load(path); return true; }
        catch (InvalidDataException) { return false; }
    }
    private static void Validate(GameState state)
    {
        if (state.RetainedDevelopmentEligibility is not (null or 0 or 1))
            throw new InvalidDataException("Invalid retained development eligibility.");
        if (!Enum.IsDefined(state.Mode) || state.Company is null || state.Initial is null || state.Recruitment is null ||
            state.Employees is null || state.EmployeeSources is null || state.Recruitment.Candidates is null || state.DevelopmentProjects is null || state.ReleasedGames is null ||
            string.IsNullOrWhiteSpace(state.Company.Name) || string.IsNullOrWhiteSpace(state.Company.PresidentName))
            throw new InvalidDataException("Invalid session state.");
        if (state.PendingProjectCancellation is { } cancellation) ProjectCancellationDialogueRules.Validate(cancellation);
        CasinoPokerService.Validate(state);
        FinancialFailureService.Validate(state);
        MysteryLabService.Validate(state);
        OwnHardwareService.Validate(state);
        if (state.PendingFacilityReceipt is { } receipt) FacilityService.ValidateReceipt(receipt);
        if (state.PendingFeeNegotiation is { } negotiation)
        {
            FeeNegotiationService.Validate(negotiation);
            if (FeeNegotiationService.CurrentFee(state,negotiation.Target)!=negotiation.After ||
                state.Calendar?.OriginalSlots0444.Contains((int)FacilityKind.Lawyer)!=true || state.PendingFacilityReceipt is not null)
                throw new InvalidDataException("Fee negotiation disagrees with company state.");
        }
        if (state.PendingHireGreeting is HireGreetingState greeting &&
            (greeting.Variant is <0 or >9 || greeting.EmployeeSourceIndex is <0 or >59 ||
             !state.Employees.Any(e => e is not null && e.SourceIndex == greeting.EmployeeSourceIndex && e.PortraitId is >=1 and <=10)))
            throw new InvalidDataException("Invalid pending hire greeting.");

        if (state.Year is < 1 || state.Month is < 1 or > 12 || state.Week is < 1 or > 4)
            throw new InvalidDataException("Invalid calendar.");
        if (state.OriginalPayrollScratchIndex<0)
            throw new InvalidDataException("Invalid original payroll scratch index.");
        if (state.DevelopmentComputerIndex is < 0 or > 3)
            throw new InvalidDataException("Invalid development computer.");
        if ((state.HardwareLicenses?.Any(x => x is < 0 or > 11) ?? false) ||
            (state.AvailableHardware?.Any(x => x is < 0 or > 11) ?? false))
            throw new InvalidDataException("Invalid hardware state.");
        if (state.TechnologyOwnershipLevels?.Any(x=>x.Key is <0 or >7 || x.Value is <0 or >3)==true)
            throw new InvalidDataException("Invalid technology ownership.");
        if (state.OriginalAccuracyCells?.Any(x=>x.Key is <0 or >512 || x.Value is <0 or >99)==true)
            throw new InvalidDataException("Invalid original accuracy cells.");
        if (state.SalesTicks is <0 or >=SalesService.TicksPerBatch || state.SalesFee1<0 || state.SalesFee2<0 ||
            state.CompanyActivityTicks is <0 or >=CompanyActivityService.TicksPerWeek || state.Company.ActivityCost<0 ||
            state.Company.Fame is <0 or >100 || state.Company.OriginalMember90>100 ||
            state.Company.ProductionCapacity<0 || state.Company.OriginalMember90<0 || state.Company.HallOfFameCount<0 ||
            (state.OriginalTypeRosterMap is not null && (state.OriginalTypeRosterMap.Count!=65 ||
             state.OriginalTypeRosterMap.Any(x=>x.Key is <0 or >64 || x.Value is <0 or >50))))
            throw new InvalidDataException("Invalid core-loop clock or original cells.");
        if (!state.ReleasedGames.Select(x=>x?.ProductIndex).SequenceEqual(
                Enumerable.Range(0,state.ReleasedGames.Count).Select(x=>(int?)x)) ||
            state.ReleasedGames.Select(x=>x.Title).Distinct().Count()!=state.ReleasedGames.Count)
            throw new InvalidDataException("Invalid product records.");
        foreach (var product in state.ReleasedGames)
        {
            if (product is null || string.IsNullOrWhiteSpace(product.Title) || product.Title.Length>128 ||
                product.HardwareSourceIndex is <0 or >11 || product.AudienceSourceIndex is <0 or >6 ||
                product.GameTypeSourceIndex is <0 or >50 || product.GenreSourceIndex is <0 or >17 || product.Bugs<0 ||
                !double.IsFinite(product.Recognition) || product.Recognition<0 || !double.IsFinite(product.DataSize) || product.DataSize<0 ||
                !double.IsFinite(product.MarketFactor) || product.MarketFactor<0 || product.Demand<0 || product.Price<0 ||
                product.Stock<0 || product.ProductionSpending<0)
                throw new InvalidDataException("Invalid released game.");
            DevelopmentWorkRules.ValidateQuality(product.Quality);
            if (product.ReleasedOn is GameDate released) AnnualCalendarValidation.Date(released);
            if (product.OriginalAwardCode is <0 or >5) throw new InvalidDataException("Invalid original award marker.");
            if (product.WeeklySalesHistory is not null) SalesHistoryRules.Validate(product.WeeklySalesHistory);
            RecallService.Validate(product);
            ShipmentDialogueRules.Validate(product);
            if (product.SalesMilestoneStage is <0 or >4 || product.PendingSalesMilestones is null ||
                (product.SalesMilestoneStage is null && product.PendingSalesMilestones.Count>0) ||
                product.PendingSalesMilestones.Count>4 ||
                !product.PendingSalesMilestones.SequenceEqual(Enumerable.Range((product.SalesMilestoneStage ?? 0)+1,product.PendingSalesMilestones.Count)) ||
                product.PendingSalesMilestones.Any(x=>x is <1 or >4))
                throw new InvalidDataException("Invalid sales milestone queue.");
            if ((product.ReviewScores is not null && (product.ReviewScores.Count!=4 || product.ReviewScores.Any(x=>x is <1 or >10))) ||
                (product.ReviewPending && (product.ReviewScores is null || product.HallOfFame)) ||
                (product.HallOfFame && (product.ReviewScores is null || product.ReviewScores.Sum()<32)) ||
                (!product.ReviewPending && product.ReviewScores?.Sum()>=32 && !product.HallOfFame))
                throw new InvalidDataException("Invalid magazine reviews.");
        }
        if (state.ReleasedGames.Count(x=>x.ReviewPending)>1) throw new InvalidDataException("Multiple pending review ceremonies.");
        if (state.DevelopmentProjects.Count>2 ||
            state.DevelopmentProjects.Select(x=>x?.RoomIndex).Distinct().Count()!=state.DevelopmentProjects.Count)
            throw new InvalidDataException("Invalid development rooms.");
        foreach (var project in state.DevelopmentProjects) ValidateProject(project);
        SequelDevelopmentRules.ValidateLineage(state);
        if (state.EmployeeSources.Any(x => x is null || x.Employee is null || x.SourceIndex < 0 ||
                string.IsNullOrWhiteSpace(x.Employee.Name) || x.Employee.OriginalAttributes is null || x.Employee.ContractFee < 0) ||
            state.EmployeeSources.Select(x => x.SourceIndex).Distinct().Count() != state.EmployeeSources.Count ||
            state.Employees.Concat(state.Recruitment.Candidates).Concat(state.EmployeeSources.Select(x=>x.Employee)).Any(x => x is null || string.IsNullOrWhiteSpace(x.Name) || x.OriginalAttributes is null || x.ContractFee < 0 || x.Experience < 0 || x.Stress < 0 || (x.Experience.HasValue && !double.IsFinite(x.Experience.Value)) || (x.Stress.HasValue && !double.IsFinite(x.Stress.Value)) || x.Level is < 1 or > 10) ||
            state.Employees.Where(x => x.SourceIndex.HasValue).Select(x => x.SourceIndex).Distinct().Count() != state.Employees.Count(x => x.SourceIndex.HasValue))
            throw new InvalidDataException("Invalid employee records.");
        foreach (var employee in state.Employees.Concat(state.EmployeeSources.Select(x=>x.Employee)).Concat(state.Recruitment.Candidates))
        {
            if (employee.TechnologyLevels?.Any(x=>x.Key is <0 or >7 || x.Value is <0 or >3)==true || employee.MaximumTechnologies<0)
                throw new InvalidDataException("Invalid employee technologies.");
            if (employee.Office?.TechnologyApplication is TechnologyApplicationState application &&
                (application.TechnologySourceIndex is <0 or >7 || application.RemainingUnits<0 ||
                (!application.IsSpeedBoost && application.RemainingUnits==0) ||
                (application.IsSpeedBoost && employee.Office!.SpeedBoostTicks<=0) ||
                employee.TechnologyLevels is null || !employee.TechnologyLevels.TryGetValue(application.TechnologySourceIndex,out int learned) || learned is <1 or >3))
                throw new InvalidDataException("Invalid active technology application.");
            if (employee.Office?.Training is EmployeeTrainingState training && (training.TechnologySourceIndex is <0 or >7 ||
                training.Progress<0 || employee.TechnologyLevels is null ||
                !employee.TechnologyLevels.TryGetValue(training.TechnologySourceIndex,out int level) || level>=3))
                throw new InvalidDataException("Invalid active training.");
        }
        var workspaces = state.Employees.Where(x => x.Office is not null).ToArray();
        if (workspaces.Any(x => x.PortraitId is not (>= 1 and <= 10) || !ValidOffice(x.Office!)) ||
            workspaces.Select(x => x.Office!.SlotIndex).Distinct().Count() != workspaces.Length ||
            workspaces.Where(x => x.Office!.LoungeSeat.HasValue).Select(x => x.Office!.LoungeSeat).Distinct().Count() !=
                workspaces.Count(x => x.Office!.LoungeSeat.HasValue) ||
            workspaces.Where(x => x.Office!.Action is >=1 and <=17).Select(x => x.Office!.Action).Distinct().Count() !=
                workspaces.Count(x => x.Office!.Action is >=1 and <=17))
            throw new InvalidDataException("Invalid office workspaces.");
        if (state.MinimumSourceIndex < 0 || state.SourceUpperBound < state.MinimumSourceIndex ||
            state.MaximumSourceIndex<state.MinimumSourceIndex || state.MaximumSourceIndex>=state.SourceUpperBound ||
            state.MaximumSourceIndex is not null && (state.MinimumSourceIndex is null || state.SourceUpperBound is null) ||
            (state.Recruitment.CandidateDatabaseDecoded && (state.MinimumSourceIndex is null || state.SourceUpperBound is null ||
             state.EmployeeSources.Count == 0 || state.EmployeeSources.Any(x => x.SourceIndex >= state.SourceUpperBound))))
            throw new InvalidDataException("Invalid recruitment bounds.");
        CompetitionService.Validate(state);
        SecretaryAdviceRules.Validate(state);
        SecretaryManagementRules.Validate(state);
        SecretaryPolicyRules.Validate(state);
        if (state.PendingRankings is null || state.PendingRankings.Select(p=>p?.Kind).Distinct().Count()!=state.PendingRankings.Count) throw new InvalidDataException("Invalid ranking queue.");
        foreach (var achievement in state.PendingRankings) RankingService.Validate(achievement);
        CalendarService.Validate(state);
        BankLoanService.Validate(state);
        OfficeBurstFlameRules.Validate(state);
        OfficeRoomPresentationRules.Validate(state);
    }

    private static void ValidateProject(DevelopmentProjectState project)
    {
        if (project?.DecisionIntroductionAcknowledged is DevelopmentMilestone introduction && !Enum.IsDefined(introduction))
            throw new InvalidDataException("Invalid checkpoint introduction.");
        if (project is null || project.RoomIndex is <0 or >1 || project.HardwareSourceIndex is <0 or >11 ||
            project.AudienceSourceIndex is <0 or >6 || project.GameTypeSourceIndex is <0 or >50 ||
            project.GenreSourceIndex is <0 or >17 || project.OriginalMode is not (0 or 1 or 2) ||
            project.OriginalRoomState is not (1 or 2) || project.OriginalStage is <0 or >3 ||
            string.IsNullOrWhiteSpace(project.Title) || project.Title.Length>128 ||
            !double.IsFinite(project.RequiredPoints) || project.RequiredPoints<=0 ||
            !double.IsFinite(project.Progress) || project.Progress<0 || project.Progress>project.RequiredPoints ||
            !double.IsFinite(project.Recognition) || project.Recognition<0 ||
            !double.IsFinite(project.DataSize) || project.DataSize<0 || project.Bugs<0 || project.Focus is null ||
            (project.OriginalStage>=1 && project.GameTypeSourceIndex is null) ||
            (project.OriginalStage>=2 && project.GenreSourceIndex is null))
            throw new InvalidDataException("Invalid development project.");
        if (project.TechnologyApplicationCounts is not null &&
            (!project.TechnologyApplicationCounts.Keys.Order().SequenceEqual(TechnologyApplicationService.ProjectTechnologyIdentifiers) ||
                project.TechnologyApplicationCounts.Any(x=>x.Value<0)))
            throw new InvalidDataException("Invalid project technology counters.");
        DevelopmentWorkRules.ValidateQuality(project.Quality);
        CheckpointMeetingRules.Validate(project);
        if (project.PendingDecision is DevelopmentMilestone decision &&
            (decision switch
            {
                DevelopmentMilestone.SelectType=>project.OriginalStage!=0 || DevelopmentWorkRules.PendingMilestone(project)!=decision,
                DevelopmentMilestone.SelectGenre=>project.OriginalStage!=1 || DevelopmentWorkRules.PendingMilestone(project)!=decision,
                DevelopmentMilestone.StartDebugging=>project.OriginalStage!=3 || project.Progress<project.RequiredPoints,
                _=>true
            })) throw new InvalidDataException("Invalid pending development decision.");
        if (project.TypeCandidates is not null && (project.TypeCandidates.Count is <5 or >65 ||
            project.TypeCandidates.Any(x=>x is <0 or >50))) throw new InvalidDataException("Invalid type roster.");
        if (project.OriginalRoomState==2 && project.PolishingTargets is null)
            throw new InvalidDataException("Missing polishing targets.");
        if (project.PolishingTargets is not null) DevelopmentWorkRules.ValidateQuality(project.PolishingTargets);
        try {DevelopmentWorkRules.SelectCategory(project.Focus,0);}
        catch (ArgumentException ex) {throw new InvalidDataException("Invalid planning focus.",ex);}
    }

    private static bool ValidOffice(OfficeEmployeeState office)
        => office.SlotIndex >= 0 && office.SlotIndex <= short.MaxValue &&
            office.X is >= short.MinValue and <= short.MaxValue && office.Y is >= short.MinValue and <= short.MaxValue &&
            office.X % 4 == 0 && office.Y % 2 == 0 &&
            office.Frame is >= 1 and <= 44 && office.MovementDirection is >= 0 and <= 4 &&
            office.WalkingTicks is >= 0 and <= 7 && office.ActionTicks is >= 0 and <= 100 &&
            office.RouteNode is >= 1 and <= 14 &&
            office.MovementState is 0 or 3 or 4 or 6 or 10 && office.Facing is >= 0 and <= 4 &&
            office.IdleAnimation is >= 0 and <= 3 && office.OffsetX is >= -6 and <= 8 &&
            office.TrainingIdleTicks>=0 && office.WorkTicks>=0 && office.WorkIdleTicks>=0 && office.WorkTier is >=0 and <=4 &&
            office.SpeedBoostTicks is >=0 and <=150 &&
            office.SharedWorkUnits is not (<0) &&
            office.BurstTicks is >=0 and <=120 && office.BurstCount is >=0 and <=50 && office.BurstDisplayCount is not (<0 or >50) && Enum.IsDefined(office.WorkCategory) &&
            office.PendingSpeechBubble is >=0 and <=16 && office.SpeechBubble is >=0 and <=16 &&
            office.SpeechBubbleTicks is >=0 and <=OfficeVisualRules.BubbleLifetime &&
            (office.SpeechBubble>0 || office.SpeechBubbleTicks==0) &&
            office.OffsetY is >= -2 and <= 8 && office.DepthAdjustment is 0 or 2 &&
            (office.Action switch
            {
                0 => office.LoungeSeat is null && office.MovementState == 0 && office.SeatedAction == 0 && !office.WorkEngaged,
                >=1 and <=17 => office.LoungeSeat is null &&
                    (office.MovementState == 0 ? office.SeatedAction == office.Action && office.WorkEngaged :
                     office.MovementState is 3 or 4 or 10 && office.SeatedAction == 0 && !office.WorkEngaged),
                >=18 and <=20 => office.LoungeSeat is >=0 and <=11 && office.Action == 18 + office.LoungeSeat / 4 &&
                    office.MovementState is 3 or 4 or 6 or 10 && office.SeatedAction == 0 && !office.WorkEngaged,
                _ => false
            });
}
