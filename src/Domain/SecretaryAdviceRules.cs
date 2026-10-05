namespace OpenGDS2.Domain;

// Stable modern presentation IDs, not the original save/queue representation.
public enum SecretaryAdviceMessage
{
    DevelopGame=0,HotSpring=1,BuyTechnology=2,ContractsNextMonth=3,
    ContractsThisMonth=4,Contest=5,LicenseUrgent=6,MoreLicenses=7,
    AnotherLicense=8,UpgradeComputer=9,ExcessStock=10,FundsLow=11,
    FundsContracts=12,FundsHealthy=13
}
public enum SecretaryAdviceResult { Success, StateUnknown, EventPending }
public sealed class SecretaryAdviceState
{
    public List<SecretaryAdviceMessage> Messages {get;set;}=[];
    public int MessageIndex {get;set;}
}

/// <summary>Module1.46 009B15F0: manual advice, in source branch/draw order.</summary>
public static class SecretaryAdviceRules
{
    // Setter 009814E4 +0050..0062; five initializer records, documented in M44.
    public static int? PortraitId(int? sourceIndex)=>sourceIndex switch
    {null=>null,0=>1,1=>2,2=>4,3=>5,4=>6,_=>throw new ArgumentOutOfRangeException(nameof(sourceIndex))};
    public static string MessageKey(SecretaryAdviceMessage message)=>Enum.IsDefined(message)
        ? $"SECRETARY_ADVICE_{message}" : throw new ArgumentOutOfRangeException(nameof(message));
    public static SecretaryAdviceResult Prepare(GameState state,OriginalCatalog? catalog,Random random)
    {
        ArgumentNullException.ThrowIfNull(state);ArgumentNullException.ThrowIfNull(random);
        if (state.PendingSecretaryAdvice is not null) return SecretaryAdviceResult.Success;
        if (state.PendingSecretaryReplacement is not null || state.PendingHireGreeting is not null || CalendarService.HasPending(state) ||
            state.DevelopmentProjects.Any(p=>p.PendingDecision.HasValue || CheckpointMeetingRules.HasPending(p)) ||
            state.ReleasedGames.Any(p=>p.ReviewPending) || RecallService.HasPending(state) || SalesMilestoneService.HasPending(state))
            return SecretaryAdviceResult.EventPending;
        // Resolve all required projections before consuming any randomness.
        if (state.Money is not long money || state.Month is not int month || state.Week is not int week ||
            state.Calendar is not { } calendar || calendar.OriginalSlots0444.Count!=3 ||
            state.TechnologyOwnershipLevels is null || catalog is null || catalog.Technologies.Count==0 ||
            catalog.Technologies.Any(t=>!state.TechnologyOwnershipLevels.ContainsKey(t.SourceIndex)) ||
            state.DevelopmentComputerIndex is not int computerIndex || state.AvailableHardware is null || state.HardwareLicenses is null ||
            state.Employees.Any(e=>e.ContractFee is null)) return SecretaryAdviceResult.StateUnknown;
        var computer=catalog.DevelopmentComputers.SingleOrDefault(c=>c.SourceIndex==computerIndex);
        if (computer is null) return SecretaryAdviceResult.StateUnknown;
        long contracts=0,stock=0;
        foreach (var employee in state.Employees) contracts=checked(contracts+employee.ContractFee!.Value);
        // Every supported own shipment writes product member 1DEC=2. Other
        // original product states/external workflows are not represented here.
        foreach (var product in state.ReleasedGames) stock=checked(stock+product.Stock);
        int technologyCount=catalog.Technologies.Count(t=>state.TechnologyOwnershipLevels[t.SourceIndex]>=1);
        int licenses=catalog.Hardware.Count(h=>state.AvailableHardware.Contains(h.SourceIndex) && state.HardwareLicenses.Contains(h.SourceIndex));
        var messages=new List<SecretaryAdviceMessage>();
        void Add(SecretaryAdviceMessage m)=>messages.Add(m);
        if (!state.DevelopmentProjects.Any(p=>p.OriginalRoomState!=0)) Add(SecretaryAdviceMessage.DevelopGame);
        // Tenant setter 0097D918 +0014..0026; definition 009F651C +9999
        // stores 60m in 04B4[1], and this predicate adds another 60m.
        if (!calendar.OriginalSlots0444.Contains(1) && money>=120_000_000 && random.Next(2)==0) Add(SecretaryAdviceMessage.HotSpring);
        bool technology=(money>=150_000_000 && technologyCount<=0) ||
            (money>=250_000_000 && technologyCount<=2) || (money>=350_000_000 && technologyCount<=4);
        if (technology && random.Next(2)==0) Add(SecretaryAdviceMessage.BuyTechnology);
        if (month==2) Add(SecretaryAdviceMessage.ContractsNextMonth);
        if (month==3) Add(SecretaryAdviceMessage.ContractsThisMonth);
        if (month==12 && week<=3) Add(SecretaryAdviceMessage.Contest);
        // Unconditional roll, even with more than two available licenses.
        if (random.Next(2)==0)
        {
            if (licenses==0) Add(SecretaryAdviceMessage.LicenseUrgent);
            if (licenses==1) Add(SecretaryAdviceMessage.MoreLicenses);
            if (licenses==2) Add(SecretaryAdviceMessage.AnotherLicense);
        }
        bool upgrade=(computer.BitWidth==8 && calendar.HardwareStage>=1) ||
            (computer.BitWidth==16 && calendar.HardwareStage>=3) || (computer.BitWidth==32 && calendar.HardwareStage>=5);
        if (upgrade && random.Next(2)==0) Add(SecretaryAdviceMessage.UpgradeComputer);
        if (stock>=200_000 && random.Next(2)==0) Add(SecretaryAdviceMessage.ExcessStock);
        Add(money<=50_000_000 ? SecretaryAdviceMessage.FundsLow :
            money<contracts ? SecretaryAdviceMessage.FundsContracts : SecretaryAdviceMessage.FundsHealthy);
        state.PendingSecretaryAdvice=new() {Messages=messages};
        return SecretaryAdviceResult.Success;
    }
    public static bool Acknowledge(GameState state)
    {
        if (state.PendingSecretaryAdvice is not { } advice) return false;
        if (++advice.MessageIndex==advice.Messages.Count) state.PendingSecretaryAdvice=null;
        return true;
    }
    public static void Validate(GameState state)
    {
        if (state.SecretarySourceIndex is <0 or >4) throw new InvalidDataException("Invalid secretary identity.");
        if (state.PendingSecretaryAdvice is not { } advice) return;
        if (advice.Messages is null || advice.Messages.Count is <1 or >8 || advice.MessageIndex<0 || advice.MessageIndex>=advice.Messages.Count ||
            advice.Messages.Any(m=>!Enum.IsDefined(m)) || advice.Messages.Last()<SecretaryAdviceMessage.FundsLow ||
            advice.Messages.Take(advice.Messages.Count-1).Any(m=>m>=SecretaryAdviceMessage.FundsLow) ||
            advice.Messages.Zip(advice.Messages.Skip(1)).Any(pair=>pair.First>=pair.Second) ||
            advice.Messages.Count(m=>m is SecretaryAdviceMessage.ContractsNextMonth or SecretaryAdviceMessage.ContractsThisMonth or SecretaryAdviceMessage.Contest)>1 ||
            advice.Messages.Count(m=>m is SecretaryAdviceMessage.LicenseUrgent or SecretaryAdviceMessage.MoreLicenses or SecretaryAdviceMessage.AnotherLicense)>1)
            throw new InvalidDataException("Invalid secretary advice queue.");
    }
}
