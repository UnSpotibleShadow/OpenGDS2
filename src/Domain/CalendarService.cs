namespace OpenGDS2.Domain;

/// <summary>Resumable projection of 009DB4F4. Unmapped modal branches stop explicitly.</summary>
public static class CalendarService
{
    public static CompanyCalendarState CreateOpening(OriginalCatalog catalog)
    {
        // Reset 009DD234: stage/release counter, countdowns, three loans and two
        // tenant cells are Empty. Initialization 009F651C sets stage 0 and 444=-1.
        return new()
        {
            Hardware=catalog.Hardware.OrderBy(x=>x.SourceIndex).Select(h=>new HardwareCalendarState
            {
                SourceIndex=h.SourceIndex,Availability=checked((int)h.RawMembers["0F1C"]),Market=h.RawMembers["1050"],
                GrowthWeeks=checked((int)h.RawMembers["10A4"]),GrowthBoost=checked((int)h.RawMembers["10C0"])
            }).ToList(),
            Loans=[new(),new(),new()],Tenants=[new(),new()],OriginalSlots0444=[-1,-1,-1],ExhibitionVisitorRecord=0
        };
    }
    public static bool HasPending(GameState state)=>state.Calendar?.Pending is not null;
    public static bool BeginWeek(GameState state,OriginalCatalog? catalog,Random random)
    {
        if (catalog is not null) catalog=OwnHardwareService.Catalog(state,catalog);
        var c=state.Calendar ?? throw new InvalidOperationException("Unknown calendar state.");
        if (c.Pending is not null) throw new InvalidOperationException("A calendar transaction is already pending.");
        if (state.Year is not int y || state.Month is not int m || state.Week is not int w)
            throw new InvalidOperationException("Unknown calendar date.");
        var date=new GameDate(y,m,w);CalendarRules.Validate(date);
        // Resolve overflow before any boundary mutation.
        _=CalendarRules.AdvanceWeek(date);
        if (catalog is not null) CompetitionService.AdvanceWeek(state,catalog);
        SalesHistoryRules.AdvanceWeek(state);
        state.CompanyActivityTicks=0;state.CompanyActivityPerformed=false;state.ProductionPerformedThisWeek=false;
        c.Pending=new() {ClosingDate=date,Phase=CalendarPhase.EarlyEvents};
        // Week 5 is an original transient. Keep it inside the transaction's
        // closing date rather than writing an invalid modern calendar date.
        if (w<4) state.Week=w+1;
        return Resume(state,catalog,random);
    }
    public static bool Resume(GameState state,OriginalCatalog? catalog,Random random)
    {
        if (catalog is not null) catalog=OwnHardwareService.Catalog(state,catalog);
        var c=state.Calendar ?? throw new InvalidOperationException("Unknown calendar state.");
        var t=c.Pending;if (t is null) return true;
        if (t.PendingEvent is not null || RecallService.HasPending(state)) return false;
        t.Block=null;
        if (catalog is null || catalog.Hardware.Any(h=>!h.RawMembers.ContainsKey("0EC8") || !h.RawMembers.ContainsKey("0F54")) ||
            !c.Hardware.Select(x=>x.SourceIndex).SequenceEqual(catalog.Hardware.OrderBy(x=>x.SourceIndex).Select(x=>x.SourceIndex)))
        {t.Block=CalendarBlock.CatalogUnknown;return false;}
        while (true)
        {
            switch (t.Phase)
            {
                case CalendarPhase.EarlyEvents:
                    // fGMGameKaishi writes -1 for Normal; every Beginner
                    // tutorial step remains >= 0, which selects the early exit.
                    if (CalendarRules.IsBeginnerEnding(state.Mode,t.ClosingDate)) return Event(CalendarEvent.BeginnerEnding);
                    if (t.ClosingDate.Month==12 && t.ClosingDate.Week==3)
                    {
                        if (state.ReleasedGames.Any(p=>p.ReleasedOn is null)) return Block(CalendarBlock.Awards);
                        t.Awards=new() {Year=t.ClosingDate.Year,RivalEntries=state.Competition is null ? null : CompetitionService.DrawContestEntries(state,random),Entries=state.ReleasedGames
                            .Where(p=>p.ReleasedOn!.Value.Year==t.ClosingDate.Year).OrderBy(p=>p.ProductIndex).Select(p=>p.ProductIndex).ToList()};
                        return Event(CalendarEvent.Awards);
                    }
                    t.Phase=CalendarPhase.TenantsAndPayroll;
                    if (ContractRenewalService.IsDueAtEndOfWeek(t.ClosingDate)) return Event(CalendarEvent.ContractRenewal);
                    break;
                case CalendarPhase.TenantsAndPayroll:
                    if (t.ClosingDate.Week==4 && state.EmployeeSources.Any(x=>x.GateValue==1) &&
                        (state.SourceUpperBound is not int upper || state.EmployeeSources.Count!=upper ||
                         !state.EmployeeSources.Select(x=>x.SourceIndex).Order().SequenceEqual(Enumerable.Range(0,upper))))
                        return Block(CalendarBlock.PayrollUnknown);
                    long? debit=t.ClosingDate.Week==4 ? OriginalPayrollRules.MonthlyDebit(state) : 0;
                    if (debit is null || state.Money is null) return Block(CalendarBlock.PayrollUnknown);
                    var next=CalendarRules.AdvanceWeek(t.ClosingDate);
                    // Monthly/yearly resets precede payroll. Prepare all sums
                    // before mutation; original signed debit has no funds guard.
                    long delta=checked(-debit.Value);
                    var finance=new FinancialRules.Change(checked(state.Money.Value+delta),delta,
                        t.ClosingDate.Week==4 ? delta : state.Company.MonthlyNet,
                        t.ClosingDate.Week==4 ? (t.ClosingDate.Month==12 ? delta :
                            state.Company.YearlyNet is long yearly ? checked(yearly+delta) : null) : state.Company.YearlyNet);
                    foreach (var tenant in c.Tenants)
                        if (tenant.Weeks>0 && --tenant.Weeks==0) tenant.OriginalState=0;
                    state.Year=next.Year;state.Month=next.Month;state.Week=next.Week;finance.Commit(state);
                    t.Phase=CalendarPhase.HardwareTriggers;t.Index=0;
                    break;
                case CalendarPhase.HardwareTriggers:
                    if (t.Index==c.Hardware.Count) {t.Phase=CalendarPhase.HardwareLaunch;t.Index=0;break;}
                    var trigger=c.Hardware[t.Index++];
                    if (HardwareCalendarRules.TryTrigger(state,catalog,trigger)) return Event(CalendarEvent.HardwareDevelopment);
                    break;
                case CalendarPhase.HardwareLaunch:
                    if (t.Index==c.Hardware.Count) {t.Phase=CalendarPhase.Recalls;t.Index=0;break;}
                    var launch=c.Hardware[t.Index];t.Phase=CalendarPhase.HardwareRetirement;
                    if (launch.LaunchWeeks>0)
                    {
                        if (launch.LaunchWeeks==1) _=checked(c.HardwareStage+1);
                        launch.LaunchWeeks--;
                        if (launch.LaunchWeeks==4) return Event(CalendarEvent.HardwareSoon);
                        if (launch.LaunchWeeks==0)
                        {
                            c.HardwareStage++;return Event(CalendarEvent.HardwareLaunch);
                        }
                    }
                    break;
                case CalendarPhase.HardwareRetirement:
                    var retired=c.Hardware[t.Index++];t.Phase=CalendarPhase.HardwareLaunch;
                    if (retired.RetirementWeeks>0)
                    {
                        retired.RetirementWeeks--;
                        if (retired.RetirementWeeks==12) return Event(CalendarEvent.HardwareRetiring);
                        if (retired.RetirementWeeks==0)
                        {
                            retired.Availability=2;state.AvailableHardware?.Remove(retired.SourceIndex);
                            state.HardwareLicenses?.Remove(retired.SourceIndex);return Event(CalendarEvent.HardwareRetired);
                        }
                    }
                    break;
                case CalendarPhase.Recalls:
                    if (t.Index==state.ReleasedGames.Count) {t.Phase=CalendarPhase.Markets;t.Index=0;break;}
                    var product=state.ReleasedGames.OrderBy(x=>x.ProductIndex).ElementAt(t.Index);
                    if (product.Recall?.OriginalState==1 && product.Recall.WeeksRemaining>0 && state.Money is null)
                        return Block(CalendarBlock.PayrollUnknown);
                    RecallService.AdvanceProductWeek(state,product,random);t.Index++;
                    if (RecallService.HasPending(state)) return false;
                    break;
                case CalendarPhase.Markets:
                    if (t.Index==c.Hardware.Count) {t.Phase=CalendarPhase.Exhibition;t.Index=0;break;}
                    var market=c.Hardware[t.Index];
                    double value=HardwareCalendarRules.NextMarket(market.Market,market.GrowthWeeks,market.GrowthBoost,
                        catalog.Hardware.Single(x=>x.SourceIndex==market.SourceIndex).RawMembers["0F54"]);
                    market.Market=value;
                    if (market.GrowthWeeks>0)
                    {market.GrowthWeeks--;if (market.GrowthBoost>0) market.GrowthBoost--;}
                    t.Index++;
                    break;
                case CalendarPhase.Exhibition:
                    if (state.Week==1 && state.Month is 4 or 10)
                    {t.Exhibition=new() {EligibleProducts=ExhibitionService.Eligible(state)};return Event(CalendarEvent.Exhibition);}
                    t.Phase=CalendarPhase.Loans;t.Index=0;break;
                case CalendarPhase.Loans:
                    if (t.Index==c.Loans.Count) {t.Phase=CalendarPhase.Graduates;t.Index=0;break;}
                    var loan=c.Loans[t.Index];
                    if (loan.OriginalType>0 && loan.BorrowedOn is GameDate borrowed &&
                        state.Year==(long)borrowed.Year+1 && state.Month==borrowed.Month && state.Week==borrowed.Week)
                    {t.Amount=LoanRepayment(loan.OriginalType);return Event(CalendarEvent.LoanRepayment);}
                    t.Index++;break;
                case CalendarPhase.Graduates:
                    if (state.Month==4 && state.Week==2 && c.OriginalSlots0444.Contains(0))
                    {
                        if (state.MinimumSourceIndex is null || state.MaximumSourceIndex is null || state.SourceUpperBound is not int sourceCount ||
                            !state.EmployeeSources.Select(p=>p.SourceIndex).Order().SequenceEqual(Enumerable.Range(0,sourceCount))) return Block(CalendarBlock.Graduates);
                        var candidates=GraduateService.DrawCandidates(state,random);
                        if (candidates.Count>0)
                        {t.Graduates=new() {Candidates=candidates};return Event(CalendarEvent.Graduates);}
                    }
                    t.Phase=CalendarPhase.Complete;break;
                case CalendarPhase.Complete:
                    c.Pending=null;return true;
                default:throw new InvalidDataException("Unknown calendar phase.");
            }
        }
        bool Event(CalendarEvent kind)
        {
            t.PendingEvent=kind;
            t.DialoguePage=CalendarDialogueRules.PageCount(kind)>0 ? 0 : null;
            return false;
        }
        bool Block(CalendarBlock kind) {t.Block=kind;return false;}
    }
    public static long LoanRepayment(int type)=>type switch
    {
        1=>13_000_000,2=>65_000_000,3=>130_000_000,4=>260_000_000,
        _=>throw new InvalidDataException("Unknown original loan type.")
    };
    public static bool Acknowledge(GameState state)
    {
        var c=state.Calendar;var t=c?.Pending;
        if (t?.PendingEvent is not CalendarEvent kind || kind is CalendarEvent.ContractRenewal or CalendarEvent.Exhibition or
            CalendarEvent.Awards or CalendarEvent.Graduates or CalendarEvent.BeginnerEnding) return false;
        if (kind==CalendarEvent.HardwareLaunch)
        {
            var hardware=c!.Hardware[t.Index];hardware.Availability=1;state.AvailableHardware?.Add(hardware.SourceIndex);
        }
        if (kind==CalendarEvent.LoanRepayment)
        {
            FinancialRules.Apply(state,checked(-t.Amount!.Value));
            c!.Loans[t.Index].OriginalType=0;t.Index++;
        }
        t.DialoguePage=null;t.PendingEvent=null;t.Amount=null;return true;
    }
    public static ContractRenewalResult RenewContracts(GameState state,IReadOnlySet<int> selected)
    {
        var t=state.Calendar?.Pending;
        if (t?.PendingEvent!=CalendarEvent.ContractRenewal) throw new InvalidOperationException("No renewal is pending.");
        var result=ContractRenewalService.TryRenew(state,selected);
        if (result==ContractRenewalResult.Success) {t.DialoguePage=null;t.PendingEvent=null;}
        return result;
    }
    public static void Validate(GameState state)
    {
        var c=state.Calendar;if (c is null) return;
        if (state.Year is null || state.Month is null || state.Week is null || state.CompanyActivityTicks is null ||
            state.AvailableHardware is null || state.HardwareLicenses is null || c.HardwareStage<0 || c.ShipmentsSinceHardwareTrigger<0 ||
            c.Hardware is null || c.Hardware.Count is <1 or >12 || c.Hardware.Any(h=>h is null || h.SourceIndex is <0 or >11 ||
                h.Availability is <0 or >2 || h.LaunchWeeks is <0 or >24 || h.RetirementWeeks is <0 or >17 ||
                !double.IsFinite(h.Market) || h.Market<0 || h.GrowthWeeks<0 || h.GrowthBoost<0) ||
            !c.Hardware.Select(h=>h.SourceIndex).SequenceEqual(c.Hardware.Select(h=>h.SourceIndex).Distinct().Order()) ||
            !state.AvailableHardware.SetEquals(c.Hardware.Where(h=>h.Availability==1).Select(h=>h.SourceIndex)) ||
            c.Hardware.Any(h=>h.Availability==2 && state.HardwareLicenses.Contains(h.SourceIndex)) ||
            c.Loans is null || c.Loans.Count!=3 || c.Loans.Any(l=>l is null || l.OriginalType is <0 or >4 || l.OriginalType>0 && l.BorrowedOn is null) ||
            c.Tenants is null || c.Tenants.Count!=2 || c.Tenants.Any(t=>t is null || t.Weeks<0) ||
            c.OriginalSlots0444 is null || c.OriginalSlots0444.Count!=3 || c.OriginalSlots0444.Any(x=>x is <-1 or >8))
            throw new InvalidDataException("Invalid company calendar cells.");
        foreach (var loan in c.Loans)
            if (loan.BorrowedOn is GameDate borrowed)
                try {CalendarRules.Validate(borrowed);} catch (ArgumentOutOfRangeException ex) {throw new InvalidDataException("Invalid borrowing date.",ex);}
        AnnualCalendarValidation.Validate(state);
        CalendarDialogueRules.Validate(state);
        var p=c.Pending;if (p is null) return;
        try {CalendarRules.Validate(p.ClosingDate);} catch (ArgumentOutOfRangeException ex) {throw new InvalidDataException("Invalid closing date.",ex);}
        if (!Enum.IsDefined(p.Phase) || p.PendingEvent is not null && !Enum.IsDefined(p.PendingEvent.Value) ||
            p.Block is not null && !Enum.IsDefined(p.Block.Value) || p.PendingEvent is not null && p.Block is not null ||
            state.CompanyActivityTicks!=0 || p.Index<0 ||
            p.Phase is CalendarPhase.HardwareTriggers or CalendarPhase.HardwareLaunch or CalendarPhase.Markets && p.Index>c.Hardware.Count ||
            p.Phase==CalendarPhase.HardwareRetirement && p.Index>=c.Hardware.Count ||
            p.Phase==CalendarPhase.Recalls && p.Index>state.ReleasedGames.Count ||
            p.Phase==CalendarPhase.Loans && p.Index>c.Loans.Count ||
            p.Phase is CalendarPhase.EarlyEvents or CalendarPhase.TenantsAndPayroll or CalendarPhase.Exhibition or CalendarPhase.Graduates or CalendarPhase.Complete && p.Index!=0)
            throw new InvalidDataException("Invalid calendar transaction cursor.");
        GameDate displayed=p.Phase is CalendarPhase.EarlyEvents or CalendarPhase.TenantsAndPayroll && p.ClosingDate.Week==4 ?
            p.ClosingDate : CalendarRules.AdvanceWeek(p.ClosingDate);
        if (displayed!=new GameDate(state.Year.Value,state.Month.Value,state.Week.Value))
            throw new InvalidDataException("Calendar cursor and displayed date disagree.");
        bool validEvent=p.PendingEvent switch
        {
            null=>true,
            CalendarEvent.ContractRenewal=>p.Phase==CalendarPhase.TenantsAndPayroll && ContractRenewalService.IsDueAtEndOfWeek(p.ClosingDate),
            CalendarEvent.HardwareDevelopment=>p.Phase==CalendarPhase.HardwareTriggers && p.Index>0 && c.Hardware[p.Index-1].LaunchWeeks==24,
            CalendarEvent.HardwareSoon=>p.Phase==CalendarPhase.HardwareRetirement && c.Hardware[p.Index].LaunchWeeks==4,
            CalendarEvent.HardwareLaunch=>p.Phase==CalendarPhase.HardwareRetirement && c.Hardware[p.Index].LaunchWeeks==0,
            CalendarEvent.HardwareRetiring=>p.Phase==CalendarPhase.HardwareLaunch && p.Index>0 && c.Hardware[p.Index-1].RetirementWeeks==12,
            CalendarEvent.HardwareRetired=>p.Phase==CalendarPhase.HardwareLaunch && p.Index>0 && c.Hardware[p.Index-1].RetirementWeeks==0 && c.Hardware[p.Index-1].Availability==2,
            CalendarEvent.LoanRepayment=>p.Phase==CalendarPhase.Loans && p.Index<c.Loans.Count && p.Amount==LoanRepayment(c.Loans[p.Index].OriginalType) &&
                c.Loans[p.Index].BorrowedOn is GameDate date && displayed.Year==(long)date.Year+1 && displayed.Month==date.Month && displayed.Week==date.Week,
            CalendarEvent.BeginnerEnding=>p.Phase==CalendarPhase.EarlyEvents && CalendarRules.IsBeginnerEnding(state.Mode,p.ClosingDate),
            CalendarEvent.Awards=>p.Phase==CalendarPhase.EarlyEvents && p.ClosingDate.Month==12 && p.ClosingDate.Week==3 && p.Awards is not null,
            CalendarEvent.Exhibition=>p.Phase==CalendarPhase.Exhibition && displayed.Month is 4 or 10 && displayed.Week==1 && p.Exhibition is not null,
            CalendarEvent.Graduates=>p.Phase==CalendarPhase.Graduates && displayed.Month==4 && displayed.Week==2 && c.OriginalSlots0444.Contains(0) && p.Graduates is not null,
            _=>false
        };
        if (!validEvent || p.PendingEvent!=CalendarEvent.LoanRepayment && p.Amount is not null)
            throw new InvalidDataException("Invalid pending calendar event.");
        bool validBlock=p.Block switch
        {
            null or CalendarBlock.CatalogUnknown=>true,
            CalendarBlock.PayrollUnknown=>p.Phase is CalendarPhase.TenantsAndPayroll or CalendarPhase.Recalls,
            CalendarBlock.BeginnerEnding=>p.Phase==CalendarPhase.EarlyEvents && p.ClosingDate==CalendarRules.BeginnerEndingClosingDate,
            CalendarBlock.Awards=>p.Phase==CalendarPhase.EarlyEvents && p.ClosingDate.Month==12 && p.ClosingDate.Week==3,
            CalendarBlock.Exhibition=>p.Phase==CalendarPhase.Exhibition && displayed.Month is 4 or 10 && displayed.Week==1,
            CalendarBlock.Graduates=>p.Phase==CalendarPhase.Graduates && displayed.Month==4 && displayed.Week==2 && c.OriginalSlots0444.Contains(0),
            _=>false
        };
        if (!validBlock) throw new InvalidDataException("Invalid unresolved calendar branch.");
    }
}
