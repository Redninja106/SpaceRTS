namespace SpaceGame.GUI;

/// <summary>
/// Helper class that calls window.EndScope(mode) when disposed.
/// </summary>
struct GUIScope(GUIWindow window, LayoutMode mode) : IDisposable
{
    //
    public void Dispose()
    {
        window.EndScope(mode);
    }
}
