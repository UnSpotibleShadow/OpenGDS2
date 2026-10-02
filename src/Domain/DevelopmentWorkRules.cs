namespace OpenGDS2.Domain;

/// <summary>Verified ordinary-work formulas from Module2 00A0283C; scheduling is separate.</summary>
public static class DevelopmentWorkRules
{
    public static double RequiredIdleTicks(int developmentSpeed)
    {
        Stat(developmentSpeed);
        return 2*(10+(150-developmentSpeed*1.5)/10);
    }
    public static DevelopmentWorkCategory SelectCategory(DevelopmentFocus focus,int roll)
    {
        ArgumentNullException.ThrowIfNull(focus);
        if (roll is <0 or >=300) throw new ArgumentOutOfRangeException(nameof(roll));
        if (focus.Fun<0 || focus.Novelty<0 || focus.Graphics<0 || focus.Music<0 ||
            (long)focus.Fun+focus.Novelty+focus.Graphics+focus.Music!=100)
            throw new ArgumentException("Original development focus must sum to 100.");
        int boundary=focus.Fun;
        if (roll<=boundary) return DevelopmentWorkCategory.Fun;
        boundary+=focus.Novelty;
        if (roll<=boundary) return DevelopmentWorkCategory.Novelty;
        boundary+=focus.Graphics;
        if (roll<=boundary) return DevelopmentWorkCategory.Graphics;
        boundary+=focus.Music;
        if (roll<=boundary) return DevelopmentWorkCategory.Music;
        return roll<=110 ? DevelopmentWorkCategory.Bugs : DevelopmentWorkCategory.Speed;
    }
    public static DevelopmentWorkTier Classify(DevelopmentWorkCategory category,int relevantStat,
        int inspiration,double draw,int? originalOffsetAccuracy=null)
    {
        if (!Enum.IsDefined(category)) throw new ArgumentOutOfRangeException(nameof(category));
        Stat(relevantStat);Stat(inspiration);Draw(draw);
        double range,burstThreshold;
        if (category<=DevelopmentWorkCategory.Music)
        {
            range=(relevantStat>=80 ? 100 : 130)-relevantStat/2.0;
            burstThreshold=1+inspiration/17.0;
        }
        else
        {
            burstThreshold=1+inspiration/20.0;
            range=category switch
            {
                DevelopmentWorkCategory.Speed=>165-relevantStat/2.0,
                // +643A adds 99 to the source index. Preserve the original cell.
                DevelopmentWorkCategory.Bugs=>88-RequiredOffset(originalOffsetAccuracy)/2.0,
                _=>120-relevantStat/2.0
            };
        }
        double roll=Math.Floor(draw*range);
        if (category==DevelopmentWorkCategory.Debugging)
            return roll<=burstThreshold ? DevelopmentWorkTier.Medium : DevelopmentWorkTier.Small;
        if (roll<=burstThreshold) return DevelopmentWorkTier.Burst;
        return roll<=35 ? DevelopmentWorkTier.Medium : DevelopmentWorkTier.Small;
    }
    public static bool IncreasesBurstCounter(DevelopmentWorkCategory category,int relevantStat,
        int currentCount,int accumulationTicks,double draw)
    {
        if (!Enum.IsDefined(category) || category==DevelopmentWorkCategory.Debugging)
            throw new ArgumentOutOfRangeException(nameof(category));
        Stat(relevantStat);Draw(draw);
        if (currentCount is <0 or >50 || accumulationTicks<0) throw new ArgumentOutOfRangeException(nameof(currentCount));
        if (currentCount>=50 || accumulationTicks>100) return false;
        return category==DevelopmentWorkCategory.Bugs
            ? Math.Floor(draw*relevantStat)<=5
            : Math.Floor(draw*(60-relevantStat/2.0)*2)<=10;
    }
    public static void ApplyContribution(DevelopmentProjectState project,DevelopmentWorkCategory category,
        DevelopmentWorkTier tier,int? burstCount=null)
    {
        ArgumentNullException.ThrowIfNull(project);
        if (!Enum.IsDefined(category) || !Enum.IsDefined(tier)) throw new ArgumentOutOfRangeException(nameof(category));
        if (project.OriginalRoomState is not (1 or 2) || project.RequiredPoints<=0 ||
            !double.IsFinite(project.RequiredPoints) || !double.IsFinite(project.Progress) ||
            project.Progress<0 || project.Bugs<0) throw new InvalidDataException("Invalid development project.");
        if (project.OriginalRoomState==2 && project.PolishingTargets is null)
            throw new InvalidOperationException("Original polishing targets are required.");
        ValidateQuality(project.Quality);
        if (project.PolishingTargets is not null) ValidateQuality(project.PolishingTargets);
        int gain;
        if (tier==DevelopmentWorkTier.Burst)
        {
            if (category==DevelopmentWorkCategory.Debugging) throw new ArgumentException("Debugging has no burst tier.");
            gain=burstCount ?? throw new InvalidOperationException("Original accumulated burst count is required.");
            if (gain is <0 or >50) throw new ArgumentOutOfRangeException(nameof(burstCount));
        }
        else gain=category==DevelopmentWorkCategory.Debugging ? (int)tier : 2*(int)tier;
        var quality=project.Quality;
        quality=category switch
        {
            DevelopmentWorkCategory.Fun=>quality with {Fun=quality.Fun+gain},
            DevelopmentWorkCategory.Novelty=>quality with {Novelty=quality.Novelty+gain},
            DevelopmentWorkCategory.Graphics=>quality with {Graphics=quality.Graphics+gain},
            DevelopmentWorkCategory.Music=>quality with {Music=quality.Music+gain},
            _=>quality
        };
        double progress=category==DevelopmentWorkCategory.Debugging ? 0 :
            tier==DevelopmentWorkTier.Burst && category!=DevelopmentWorkCategory.Speed ? gain/3.0 : gain;
        int bugs=category switch
        {
            DevelopmentWorkCategory.Bugs=>checked(project.Bugs+gain),
            DevelopmentWorkCategory.Debugging=>Math.Max(0,project.Bugs-gain),
            _=>project.Bugs
        };
        if (project.OriginalRoomState==2)
        {
            var targets=project.PolishingTargets!;
            quality=new(Math.Min(quality.Fun,targets.Fun),Math.Min(quality.Novelty,targets.Novelty),
                Math.Min(quality.Graphics,targets.Graphics),Math.Min(quality.Music,targets.Music));
        }
        project.Quality=quality;
        project.Progress=Math.Min(project.RequiredPoints,project.Progress+progress);
        project.Bugs=bugs;
    }
    public static DevelopmentMilestone? PendingMilestone(DevelopmentProjectState project)
    {
        if (project.RequiredPoints<=0 || !double.IsFinite(project.RequiredPoints) ||
            project.Progress<0 || !double.IsFinite(project.Progress)) throw new InvalidDataException("Invalid project progress.");
        if (project.OriginalRoomState!=1) return null;
        return project.OriginalStage switch
        {
            0 when project.Progress/project.RequiredPoints*100>=50=>DevelopmentMilestone.SelectType,
            1 when project.Progress/project.RequiredPoints*100>=81=>DevelopmentMilestone.SelectGenre,
            2 when project.Progress>=project.RequiredPoints=>DevelopmentMilestone.StartDebugging,
            _=>null
        };
    }
    public static double DataSizeGain(DevelopmentWorkCategory category,int hardwareIdentifier,double draw)
    {
        if (!Enum.IsDefined(category)) throw new ArgumentOutOfRangeException(nameof(category));
        Draw(draw);
        double gain=category switch
        {
            DevelopmentWorkCategory.Fun or DevelopmentWorkCategory.Novelty=>.03+Math.Floor(draw*8)/1000,
            DevelopmentWorkCategory.Graphics=>.08+Math.Floor(draw*16)/1000,
            DevelopmentWorkCategory.Music=>.05+Math.Floor(draw*12)/1000,
            DevelopmentWorkCategory.Speed=>.1+Math.Floor(draw*5)/1000,
            DevelopmentWorkCategory.Bugs=>.05+Math.Floor(draw*8)/1000,
            _=>0
        };
        return gain*(hardwareIdentifier switch {1 or 4 or 8 or 10=>1.1,2=>4,3=>2,5=>.6,6 or 7=>1.2,_=>1});
    }
    internal static void ValidateQuality(DevelopmentQuality quality)
    {
        if (quality is null || new[] {quality.Fun,quality.Novelty,quality.Graphics,quality.Music}
            .Any(x=>!double.IsFinite(x) || x<0)) throw new InvalidDataException("Invalid development quality.");
    }
    private static int RequiredOffset(int? value)
    {
        if (value is not int stat) throw new InvalidOperationException("Original Accuracy[sourceIndex + 99] is required.");
        Stat(stat);return stat;
    }
    private static void Stat(int value) {if (value is <0 or >99) throw new ArgumentOutOfRangeException(nameof(value));}
    private static void Draw(double value) {if (!double.IsFinite(value) || value<0 || value>=1) throw new ArgumentOutOfRangeException(nameof(value));}
}
