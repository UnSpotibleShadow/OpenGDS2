using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowHallCelebration(GameState state,ReleasedGameState product)
    {
        var scene=product.HallCelebration!;
        ClearContent();CompactManagementWindow("HallCelebrationWindow",new(370,310),350);
        AddHeading(Tr("REVIEW_HALL_OF_FAME"),22);AddHeading(product.Title,16);
        var picture=Picture("HallCelebrationPicture",null,new(256,160));_content.AddChild(picture);
        var fallback=new Label {Name="HallCelebrationFallback",Text=Tr("REVIEW_ARTWORK_REIMPORT"),AutowrapMode=TextServer.AutowrapMode.WordSmart};_content.AddChild(fallback);
        var close=new Button {Name="AcknowledgeHallCelebration",Text=Tr("CONTINUE")};_content.AddChild(close);
        close.Pressed+=()=>
        {
            if(!ReferenceEquals(_session.Current,state) || !ReferenceEquals(product.HallCelebration,scene))return;
            HallCelebrationRules.Acknowledge(product);
            if(!ShowPendingProductEvent(state))ShowSales(state);
        };
        void Refresh()
        {
            picture.Texture=scene.Frame==0 ? null : OriginalAssetStore.LoadTexture($"Graphic/Embedded/HallCelebration{scene.Frame}.bmp");
            fallback.Visible=scene.Frame!=0 && picture.Texture is null;
        }
        Refresh();AdvanceHallCelebration(state,product,scene,_dialog!,Refresh);
    }
    private async void AdvanceHallCelebration(GameState state,ReleasedGameState product,HallCelebrationState scene,DesktopWindow window,Action refresh)
    {
        while(ReferenceEquals(product.HallCelebration,scene))
        {
            await ToSignal(GetTree().CreateTimer(.025),SceneTreeTimer.SignalName.Timeout);
            if(!ReferenceEquals(_session.Current,state) || !ReferenceEquals(product.HallCelebration,scene) ||
                !ReferenceEquals(_dialog,window) || !GodotObject.IsInstanceValid(window) || !window.IsInsideTree())return;
            HallCelebrationRules.Advance(state,product);refresh();
        }
    }
}
