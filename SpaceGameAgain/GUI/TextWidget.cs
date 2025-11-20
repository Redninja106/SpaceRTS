using SpaceGame.Planets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;

internal class TextWidget(Transform transform, string text, float? size = null, Color? color = null, SphereOfInfluence? soi = null)
{
    public Transform Transform = transform;
    public string Text = text;
    public float Size = size ?? .5f;
    public Color Color = color ?? GUIWindow.DefaultTextColor;
    public int Age;
    public SphereOfInfluence? soi = soi;

    public void Render(ICanvas canvas, Camera camera, bool alwaysLegible)
    {
        canvas.PushState();
        Transform.ApplyTo(canvas, camera);
        if (alwaysLegible)
        {
            float scale = Program.World.Camera.SmoothVerticalSize;
            float minZoom = float.Log(2 * float.Min(camera.DisplayWidth, camera.DisplayHeight) / (128 * float.Sqrt(3)), 1.1f);
            canvas.Scale(scale / minZoom);
        }
        canvas.Fill(Color.Black);
        canvas.DrawAlignedText(Text, Size, new Vector2(.015f, .015f), Alignment.Center, TextStyle.Regular);
        canvas.Fill(Color);
        canvas.DrawAlignedText(Text, Size, Vector2.Zero, Alignment.Center, TextStyle.Regular);
        canvas.PopState();
    }

    public void Tick()
    {
        soi?.ApplyTickTo(ref this.Transform);
        Transform.Position.Y -= Program.Timestep;
        Age++;
    }
}
