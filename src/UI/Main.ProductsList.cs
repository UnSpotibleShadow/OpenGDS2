using Godot;
using OpenGDS2.Domain;
namespace OpenGDS2.UI;
public partial class Main
{
    private void ShowProductList(GameState state)
    {ShowOfficeReconstruction(state);if(_dialog is null)_windows["ProductWindow"].Reopen();}
    private ReleasedGameState? SelectedProduct(GameState state)=>_productTable?.GetSelected() is { } selected?state.ReleasedGames.SingleOrDefault(p=>p.ProductIndex==selected.GetMetadata(0).AsInt32()):state.ReleasedGames.OrderByDescending(p=>p.ProductIndex).FirstOrDefault();
    private void ShowSelectedProductHistory(GameState state){if(SelectedProduct(state) is { } product)ShowSalesHistory(state,product);}
    private void ShowSelectedProductReviews(GameState state){if(SelectedProduct(state) is {ReviewScores:not null} product)ShowReviews(state,product);}
    private static void StyleSelection(Tree table,Color color)
    {
        table.SelectMode=Tree.SelectModeEnum.Row;
        foreach(string name in new[]{"selected","selected_focus"})table.AddThemeStyleboxOverride(name,new StyleBoxFlat {BgColor=color});
    }
}
