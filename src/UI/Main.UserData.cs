using Godot;
using OpenGDS2.Persistence;

namespace OpenGDS2.UI;

public partial class Main
{
    private void ShowDataMigrationFailure(UserDataMigrationResult result)
    {
        AddHeading(Tr("DATA_MIGRATION_TITLE"),22);
        var text=new Label {Name="DataMigrationError",Text=Tr("DATA_MIGRATION_ERROR").Replace("{reason}",result.Error??Tr("UNKNOWN")),
            AutowrapMode=TextServer.AutowrapMode.WordSmart,CustomMinimumSize=new(600,0)};
        _content.AddChild(text);
        AddAction("DATA_MIGRATION_RETRY",()=>GetTree().ReloadCurrentScene());
        AddAction("QUIT",()=>GetTree().Quit());
    }
}
