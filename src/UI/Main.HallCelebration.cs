using Godot;
using OpenGDS2.Core;
using OpenGDS2.Domain;

namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowHallCelebration(GameState state,ReleasedGameState product)
    {
        var scene=product.HallCelebration!;
        ClearContent();var canvas=NativeWindow("HallCelebrationWindow",Tr("NATIVE_CONGRATULATIONS"),new(268,192));
        var picture=NativePlace(canvas,Picture("HallCelebrationPicture",null,new(256,160)),new(6,4,256,160));
        var fallback=NativeText(canvas,"HallCelebrationFallback",Tr("REVIEW_ARTWORK_REIMPORT"),new(6,4,256,160));fallback.AutowrapMode=TextServer.AutowrapMode.WordSmart;
        var close=NativeAction(canvas,"AcknowledgeHallCelebration","CONTINUE",new(170,166,92,23),()=>{});
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
