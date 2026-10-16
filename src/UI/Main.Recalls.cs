using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowRecall(GameState state,ReleasedGameState product)
    {
        var recall=product.Recall ?? throw new InvalidOperationException("Unknown recall.");
        var pending=recall.PendingEvent;
        string Text(string key)=>Tr(key).Replace("{title}",product.Title);
        var representative=Tr("RECALL_REPRESENTATIVE");
        var secretary=SecretaryCaption(state.SecretarySourceIndex);
        var portrait=SecretaryAdviceRules.PortraitId(state.SecretarySourceIndex);
        var pages=new List<ConversationPage>();
        if(pending==RecallEvent.Investigation)
        {
            pages.Add(new(representative,Text("RECALL_DIALOGUE_INVESTIGATION")));
            pages.Add(new(secretary,Text("RECALL_DIALOGUE_WAIT"),portrait));
        }
        else if(recall.OutcomeRoll==3)
            pages.Add(new(secretary,Text("RECALL_DIALOGUE_DISMISSED"),portrait));
        else
        {
            long rate=recall.OutcomeRoll==0 ? product.Price : 1000;
            // The saved result has already withdrawn stock. Recover quantity from
            // its immutable charge, never from the now-zero inventory.
            string quantity=rate>0 && recall.RefundAmount is long refund && refund%rate==0
                ? DisplayNumbers.Format(refund/rate) : Tr("RECALL_QUANTITY_UNKNOWN");
            pages.Add(new(representative,Text("RECALL_DIALOGUE_RESULT")));
            pages.Add(new(representative,Text(recall.OutcomeRoll==0 ? "RECALL_DIALOGUE_FULL" : "RECALL_DIALOGUE_FIXED")
                .Replace("{price}",DisplayNumbers.Format(rate))));
            pages.Add(new(secretary,Text("RECALL_DIALOGUE_REACTION"),portrait));
            pages.Add(new(secretary,Text("RECALL_DIALOGUE_QUANTITY").Replace("{quantity}",quantity)
                .Replace("{price}",DisplayNumbers.Format(rate)),portrait));
            pages.Add(new(secretary,Text("RECALL_DIALOGUE_COST").Replace("{refund}",DisplayNumbers.Format(recall.RefundAmount)),portrait));
            pages.Add(new(secretary,Text("RECALL_DIALOGUE_CAUTION"),portrait));
        }
        ShowConversation(Tr("CONSUMER_INVESTIGATION"),pages,()=>
        {
            if(product.Recall==recall && recall.PendingEvent==pending)ShowRecallReceipt(state,product);
        });
    }

    private void ShowRecallReceipt(GameState state,ReleasedGameState product)
    {
        var recall=product.Recall ?? throw new InvalidOperationException("Unknown recall.");
        ClearContent();IdentifyManagementWindow("RecallNoticeWindow");AddHeading(Tr("CONSUMER_INVESTIGATION"),28);AddHeading(product.Title,22);
        if (recall.PendingEvent==RecallEvent.Investigation)
        {
            AddHeading(Tr("RECALL_INVESTIGATION_STARTED"),18);
            AddHeading($"{Tr("RECALL_WEEKS_REMAINING")}: {recall.WeeksRemaining}",18);
        }
        else
        {
            AddHeading(Tr($"RECALL_OUTCOME_{recall.OutcomeRoll}"),18);
            AddHeading($"{Tr("RECALL_REFUND")}: ¥{DisplayNumbers.Format(recall.RefundAmount)}",18);
            AddHeading($"{Tr("FUNDS")}: ¥{DisplayNumbers.Format(state.Money)}",18);
        }
        _status=new Label();_content.AddChild(_status);
        AddAction("SAVE_GAME",()=>
        {
            try {Persistence.SaveStore.Save(SavePath,state);_status.Text=Tr("SAVE_OK");}
            catch (Exception ex) {_status.Text=$"{Tr("SAVE_FAILED")}: {ex.Message}";}
        });
        AddAction("LOAD_GAME",LoadSession,System.IO.File.Exists(SavePath));
        var acknowledge=new Button {Name="AcknowledgeRecall",Text=Tr("CONTINUE")};_content.AddChild(acknowledge);
        acknowledge.Pressed+=()=>{RecallService.Acknowledge(state,product);ShowSales(state);};
    }
}
