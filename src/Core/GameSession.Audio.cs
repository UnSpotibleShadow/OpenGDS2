using OpenGDS2.Domain;
namespace OpenGDS2.Core;

public sealed partial class GameSession
{
    // Transient presentation notifications: no new saved cursor, draws or game transactions.
    public event Action<OriginalSoundCue>? SoundRequested;
    public event Action? SoundResetRequested;
    private CasinoResult SoundCommand(Func<GameState,CasinoResult> command)
    {
        var state=Current ?? throw new InvalidOperationException("No current session.");long coins=state.Casino.Coins;
        var result=command(state);
        if(result==CasinoResult.Success && state.Casino.Coins<coins)SoundRequested?.Invoke(OriginalSoundCue.Gold);
        return result;
    }
    public CasinoResult PokerCommand(int command)=>SoundCommand(s=>CasinoPokerService.Command(s,command,_random));
    public CasinoResult BlackjackCommand(int command)=>SoundCommand(s=>CasinoBlackjackService.Command(s,command,_random));
    public CasinoResult CardChainCommand(int command,bool direct=false)=>SoundCommand(s=>CasinoCardChainService.Command(s,command,_random,direct));
    public void TickPoker()
    {
        var state=Current ?? throw new InvalidOperationException("No current session.");var p=state.Casino.Poker;
        int phase=p.Phase,revealed=p.Revealed.Count(x=>x);bool tie=p.TieRedeal;long coins=state.Casino.Coins;
        CasinoPokerService.Tick(state,_random);
        if(state.Casino.Coins>coins)SoundRequested?.Invoke(OriginalSoundCue.Gold);
        // Native replacement and dealer-only reveal branches have no sound call.
        if(state.Casino.PokerOpen && (phase==1 || phase==6 && !tie) && p.Revealed.Count(x=>x)>revealed)
            SoundRequested?.Invoke(OriginalSoundCue.Up);
    }
    public void TickBlackjack()
    {
        var state=Current ?? throw new InvalidOperationException("No current session.");long coins=state.Casino.Coins;
        CasinoBlackjackService.Tick(state,_random);
        if(state.Casino.Coins>coins)SoundRequested?.Invoke(OriginalSoundCue.Gold);
    }
    public void TickCardChain()
    {
        var state=Current ?? throw new InvalidOperationException("No current session.");long coins=state.Casino.Coins;
        CasinoCardChainService.Tick(state,_random);
        if(state.Casino.Coins>coins)SoundRequested?.Invoke(OriginalSoundCue.Gold);
    }
    public bool SelectChainCard(int index,bool direct=false)
    {
        var state=Current ?? throw new InvalidOperationException("No current session.");
        bool cue=state.Casino.CardChainOpen && state.Casino.CardChain.Round.Phase==1 && index is >=0 and <5 &&
            (direct || CasinoCardChainService.CanSelect(state,index));
        if(cue)SoundRequested?.Invoke(OriginalSoundCue.Up);
        return CasinoCardChainService.SelectCard(state,index,direct);
    }
}
