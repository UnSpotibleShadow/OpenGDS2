using Godot;

namespace OpenGDS2.Tests;

// Debug checks create thousands of temporary managed resource wrappers. Drain
// their finalizers while Godot's C# bindings still exist, before test shutdown.
public partial class ManagedTestCleanup : Node
{
    public void Drain()
    {
        if (DisplayServer.GetName() != "headless") return;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
