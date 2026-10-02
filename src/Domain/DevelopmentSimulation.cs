namespace OpenGDS2.Domain;

/// <summary>Module2 ordinary own-game work and stage-3 debugging. No external jobs/polishing.</summary>
public static class DevelopmentSimulation
{
    public static void TickEmployee(GameState state,EmployeeState employee,OriginalCatalog catalog,Random random)
    {
        var office=employee.Office;
        if (office is null || !office.WorkEngaged || office.MovementState!=0 || office.SeatedAction is <6 or >17) return;
        var project=state.DevelopmentProjects.SingleOrDefault(x=>x.RoomIndex==(office.SeatedAction-6)/6);
        if (project is null || project.PendingDecision.HasValue || project.OriginalRoomState!=1) return;
        if (employee.Stress is not double stress || employee.DevelopmentSpeed is not int speed ||
            employee.Inspiration is not int inspiration || employee.Experience is null || employee.Level is null)
            return; // Unknown imported/legacy employee fields cannot be guessed.
        if (project.OriginalStage<3) employee.Stress=Math.Min(100,stress+(random.Next(3)+1)/60.0);
        if (office.WorkTicks==0 && !office.IdleBreak && random.Next(130)==0)
        {
            office.IdleBreak=true;office.ActionTicks=0;office.IdleAnimation=random.Next(3);
            employee.Stress=Math.Max(0,employee.Stress.Value-2);
        }
        if (office.IdleBreak)
        {
            int limit=office.IdleAnimation switch {0=>50,1=>80,_=>100};
            if (office.ActionTicks>=limit)
            {
                office.ActionTicks=office.IdleAnimation==2 ? 12 : 6;
                office.IdleBreak=false;office.WorkIdleTicks=0;
            }
            office.ActionTicks++;
        }
        else
        {
            if (office.ActionTicks==14) office.Frame=14+3*(office.Facing-1);
            if (office.ActionTicks>=28) {office.Frame=15+3*(office.Facing-1);office.ActionTicks=0;}
            office.ActionTicks++;
        }
        double idleThreshold=project.OriginalStage==3 ? 2*(10+(100-speed)/10.0) : DevelopmentWorkRules.RequiredIdleTicks(speed);
        if (!office.IdleBreak && office.WorkTicks==0 && office.WorkIdleTicks>=idleThreshold)
        {
            if (project.OriginalStage==3)
            {
                if (project.Bugs>0) employee.Stress=Math.Min(100,employee.Stress!.Value+(random.Next(3)+1)/20.0);
                if (random.Next(50)==0 && project.Bugs>0)
                    Start(DevelopmentWorkCategory.Debugging);
            }
            else Start(DevelopmentWorkRules.SelectCategory(project.Focus!,random.Next(300)));
        }
        else office.WorkIdleTicks=checked(office.WorkIdleTicks+1);
        bool complete=false;
        if (office.WorkTier is 1 or 2 && office.WorkTicks==16) complete=true;
        if (office.WorkTier==3 && office.WorkTicks>=32)
        {
            office.BurstTicks++;
            if (office.BurstCount<50 && office.BurstTicks<=100 &&
                DevelopmentWorkRules.IncreasesBurstCounter(office.WorkCategory,Stat(office.WorkCategory),office.BurstCount,office.BurstTicks,random.NextDouble()))
                office.BurstCount++;
            if (office.BurstTicks>=120) complete=true;
        }
        if (complete)
        {
            DevelopmentWorkRules.ApplyContribution(project,office.WorkCategory,(DevelopmentWorkTier)office.WorkTier,office.BurstCount);
            if (office.WorkCategory!=DevelopmentWorkCategory.Debugging)
                project.DataSize+=DevelopmentWorkRules.DataSizeGain(office.WorkCategory,
                    catalog.Hardware.Single(x=>x.SourceIndex==project.HardwareSourceIndex).Identifier,random.NextDouble());
            office.WorkTicks=0;
        }
        if (office.WorkTicks>=1) office.WorkTicks++;
        var milestone=DevelopmentWorkRules.PendingMilestone(project);
        if (milestone.HasValue)
        {
            project.PendingDecision=milestone;
            double bonus=milestone switch {DevelopmentMilestone.SelectType=>10,DevelopmentMilestone.SelectGenre=>20,_=>40};
            employee.Experience=EmployeeProgressionRules.AccrueExperience(employee.Experience!.Value,bonus,employee.Level!.Value);
            if (milestone==DevelopmentMilestone.StartDebugging) project.OriginalStage=3;
        }

        int Stat(DevelopmentWorkCategory category)=>category switch
        {
            DevelopmentWorkCategory.Fun=>employee.Fun ?? throw new InvalidDataException("Unknown Fun."),
            DevelopmentWorkCategory.Novelty=>employee.Novelty ?? throw new InvalidDataException("Unknown Novelty."),
            DevelopmentWorkCategory.Graphics=>employee.Graphics ?? throw new InvalidDataException("Unknown Graphics."),
            DevelopmentWorkCategory.Music=>employee.Music ?? throw new InvalidDataException("Unknown Music."),
            DevelopmentWorkCategory.Speed=>speed,
            _=>employee.Accuracy ?? throw new InvalidDataException("Unknown Accuracy.")
        };
        void Start(DevelopmentWorkCategory category)
        {
            int? accuracy=null;
            if (category==DevelopmentWorkCategory.Bugs)
            {
                if (employee.SourceIndex is not int source || state.OriginalAccuracyCells is null ||
                    !state.OriginalAccuracyCells.TryGetValue(source+99,out int value)) return;
                accuracy=value;
            }
            office.WorkCategory=category;office.WorkTicks=1;office.WorkIdleTicks=0;
            office.BurstTicks=0;office.BurstCount=0;office.WorkTier=0;
            // Ordinary work sets tier zero at stress 100; debugging has its own branch.
            if (employee.Stress<100)
            {
                office.WorkTier=(int)DevelopmentWorkRules.Classify(category,Stat(category),inspiration,random.NextDouble(),accuracy);
                employee.Experience=EmployeeProgressionRules.AccrueExperience(employee.Experience!.Value,3.7,employee.Level!.Value);
            }
        }
    }
    public static IReadOnlyList<int> PrepareTypeCandidates(GameState state,DevelopmentProjectState project,OriginalCatalog catalog,Random random)
    {
        if (project.PendingDecision!=DevelopmentMilestone.SelectType) throw new InvalidOperationException("No pending type selection.");
        if (state.OriginalTypeRosterMap is null) throw new InvalidOperationException("Original roster cells are unknown in this legacy save. Start a new company.");
        if (project.TypeCandidates is null)
        {
            project.TypeCandidates=DevelopmentTypeRosterRules.Build(catalog,
                catalog.Audiences.Single(x=>x.SourceIndex==project.AudienceSourceIndex),random).ToList();
            for (int i=0;i<project.TypeCandidates.Count;i++) state.OriginalTypeRosterMap[i]=project.TypeCandidates[i];
        }
        return project.TypeCandidates;
    }
}
