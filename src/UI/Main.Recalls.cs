using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowRecall(GameState state,ReleasedGameState product)
    {
        var recall=product.Recall ?? throw new InvalidOperationException("Unknown recall.");
        var pending=recall.PendingEvent;
        var native=TranslationServer.GetLocale().StartsWith("en",StringComparison.OrdinalIgnoreCase)?null:_importer.LoadDialogue();
        string Text(string key)=>(native?.Entries.GetValueOrDefault(key)??Tr(key)).Replace("{title}",product.Title);
        var representative=Text("RECALL_REPRESENTATIVE");
        var secretary=SecretaryCaption(state.SecretarySourceIndex);
        var portrait=SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex);
        var pages=new List<ConversationPage>();
        if(pending==RecallEvent.Investigation)
        {
            pages.Add(new(secretary,Text("RECALL_DIALOGUE_ALERT"),portrait));
            pages.Add(new(representative,Text("RECALL_DIALOGUE_INVESTIGATION")));
            pages.Add(new(representative,Text("RECALL_DIALOGUE_WAIT")));
        }
        else
        {
            pages.Add(new(secretary,Text("RECALL_DIALOGUE_RESULT_INTRO"),portrait));
            if(recall.OutcomeRoll==3)
                pages.Add(new(secretary,Text("RECALL_DIALOGUE_DISMISSED"),portrait));
            else
            {
                long rate=recall.OutcomeRoll==0 ? product.Price : 1000;
                // The saved result already withdrew stock. Recover quantity from
                // its immutable charge, never from the now-zero inventory.
                string quantity=rate>0 && recall.RefundAmount is long refund && refund%rate==0
                    ? DisplayNumbers.Format(refund/rate) : Tr("RECALL_QUANTITY_UNKNOWN");
                pages.Add(new(representative,Text("RECALL_DIALOGUE_RESULT")));
                pages.Add(new(representative,Text(recall.OutcomeRoll==0 ? "RECALL_DIALOGUE_FULL" : "RECALL_DIALOGUE_FIXED")
                    .Replace("{price}",DisplayNumbers.Format(rate))));
                pages.Add(new(secretary,Text(recall.OutcomeRoll==0 ? "RECALL_DIALOGUE_REACTION_FULL":"RECALL_DIALOGUE_REACTION"),portrait));
                pages.Add(new(secretary,Text("RECALL_DIALOGUE_QUANTITY").Replace("{quantity}",quantity)
                    .Replace("{price}",DisplayNumbers.Format(rate)),portrait));
                pages.Add(new(secretary,Text("RECALL_DIALOGUE_COST").Replace("{refund}",DisplayNumbers.Format(recall.RefundAmount)),portrait));
                pages.Add(new(secretary,Text("RECALL_DIALOGUE_GROAN"),portrait));
                pages.Add(new(secretary,Text("RECALL_DIALOGUE_CAUTION"),portrait));
            }
        }
        ShowConversation(Tr("CONSUMER_INVESTIGATION"),pages,()=>
        {
            if(!ReferenceEquals(_session.Current,state)||product.Recall!=recall||recall.PendingEvent!=pending)return;
            if(RecallService.Acknowledge(state,product))ShowSales(state);
        });
    }
}
