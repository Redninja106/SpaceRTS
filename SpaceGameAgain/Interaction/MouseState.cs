namespace SpaceGame.Interaction;

internal class MouseState(MouseButton button)
{
    public bool Holding;
    public bool Dragging;
    public bool Pressed;
    public bool Released;
    public bool Dragged;
    public Vector2 DragStart;

    public void Update()
    {
        Dragged = false;
        Pressed = World.HasFocus && Mouse.IsButtonPressed(button);
        Released = Holding && Mouse.IsButtonReleased(button);
        
        if (Pressed)
        {
            DragStart = World.MousePosition;
            Holding = true;
        }
        if (Released)
        {
            Holding = false;
            if (Dragging)
            {
                Dragged = true;
            }
            Dragging = false;
        }
        if (Holding && Vector2.Distance(DragStart, World.MousePosition) > World.Camera.ScreenDistanceToWorldDistance(25f))
        {
            Dragging = true;
        }
    }

    public void Tick()
    {
        var soi = World.GetSphereOfInfluence(DragStart);
        if (soi != null)
        {
            soi.ApplyTo(ref DragStart);
        }
    }
}