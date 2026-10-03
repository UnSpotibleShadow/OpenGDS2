using Godot;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowRecall(GameState state,ReleasedGameState product)
    {
        var recall=product.Recall ?? throw new InvalidOperationException("Unknown recall.");
        ClearContent();AddHeading(Tr("CONSUMER_INVESTIGATION"),28);AddHeading(product.Title,22);
        if (recall.PendingEvent==RecallEvent.Investigation)
        {
            AddHeading(Tr("RECALL_INVESTIGATION_STARTED"),18);
            AddHeading($"{Tr("RECALL_WEEKS_REMAINING")}: {recall.WeeksRemaining}",18);
        }
        else
        {
            AddHeading(Tr($"RECALL_OUTCOME_{recall.OutcomeRoll}"),18);
            AddHeading($"{Tr("RECALL_REFUND")}: ¥{recall.RefundAmount:N0}",18);
            AddHeading($"{Tr("FUNDS")}: ¥{state.Money:N0}",18);
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
