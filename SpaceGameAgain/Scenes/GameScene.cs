using SpaceGame.GUI;
using SpaceGame.Planets;
using SpaceGame.Ships.Modules;
using SpaceGame.Ships;
using SpaceGame.Structures;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Scenes;
internal class GameScene : Scene
{
    ITexture view;
    public static Vector2 ViewportMousePosition;

    public GameScene()
    {
        view = Graphics.CreateTexture(640, 480);
        World = new(null);
        new WorldGenerator().CreateActors();

        int nextId = 1;


    }

    public override void Tick()
    {
        World.Tick();
    }

    public override void Update(float tickProgress)
    {
        World.Camera.Update(view.Width, view.Height);
        Rectangle vp = new(0, 0, view.Width, view.Height);

        var canvas = Graphics.GetOutputCanvas();

        float vpScaleY = canvas.Height / (float)view.Height;
        float vpScaleX = (canvas.Width - (World.LeftSidebar.MinWidth + World.RightSidebar.MinWidth)) / (float)view.Width;

        MatrixBuilder viewMatrix = new MatrixBuilder()
            .Translate(canvas.Width / 2f, canvas.Height / 2f)
            .Scale(MathF.Min(vpScaleX, vpScaleY))
            .Translate(-view.Width / 2f, -view.Height / 2f);

        ViewportMousePosition = Vector2.Transform(Mouse.Position, viewMatrix.InverseMatrix);
        World.Update(tickProgress, ViewportMousePosition, vp.ContainsPoint(ViewportMousePosition));
        World.LeftSidebar.Update(canvas.Width, canvas.Height);
        World.RightSidebar.Update(canvas.Width, canvas.Height);
    }

    public override void Render(ICanvas canvas)
    {
        float vpScaleY = canvas.Height / (float)view.Height;
        float vpScaleX = (canvas.Width - (World.LeftSidebar.MinWidth + World.RightSidebar.MinWidth)) / (float)view.Width;

        MatrixBuilder viewMatrix = new MatrixBuilder()
            .Translate(canvas.Width / 2f, canvas.Height / 2f)
            .Scale(MathF.Min(vpScaleX, vpScaleY))
            .Translate(-view.Width / 2f, -view.Height / 2f);

        if (vpScaleY < vpScaleX)
        {
            float leftGap = canvas.Width / 2f - (view.Width * vpScaleY) / 2f - World.LeftSidebar.MinWidth;
            World.LeftSidebar.Width = MathF.Min(World.LeftSidebar.MinWidth + leftGap, World.LeftSidebar.MaxWidth);

            float rightGap = canvas.Width / 2f - (view.Width * vpScaleY) / 2f - World.RightSidebar.MinWidth;
            World.RightSidebar.Width = MathF.Min(World.RightSidebar.MinWidth + rightGap, World.RightSidebar.MaxWidth);
        }
        else
        {
            World.LeftSidebar.Width = World.LeftSidebar.MinWidth;
            World.RightSidebar.Width = World.RightSidebar.MinWidth;
        }

        if (canvas.Width is 0 && canvas.Height is 0)
            return;

        RenderView();

        canvas.Clear(Color.FromHSV(0, 0, .1f));

        canvas.Font(Program.font);
        canvas.PushState();
        World.LeftSidebar.Render(canvas);
        canvas.PopState();

        canvas.PushState();
        World.RightSidebar.Render(canvas);
        canvas.PopState();

        canvas.Transform(viewMatrix.Matrix);
        canvas.DrawTexture(view);
    }

    private void RenderView()
    {
        var canvas = view.GetCanvas();

        canvas.PushState();
        canvas.Antialias(false);
        World.Camera.RenderSetup(canvas);
        World.Render(canvas);
        DebugDraw.Flush(canvas);
        canvas.PopState();
    }
}