using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;

internal class TextWidget(TextWidgetPrototype prototype, ulong id, Transform transform, string text) : WorldActor(prototype, id, transform)
{
    public override TextWidgetPrototype Prototype => (TextWidgetPrototype)base.Prototype;

    string text = text;

    public override void Serialize(BinaryWriter writer)
    {
        throw new InvalidOperationException();
    }

    public override void Render(ICanvas canvas)
    {
        canvas.DrawText(text, Prototype.Size, Vector2.Zero, TextStyle.Regular);
        base.Render(canvas);
    }

    public override void Tick()
    {
        base.Tick();
    }
}

class TextWidgetPrototype : WorldActorPrototype
{
    public float Size { get; set; }
    public Color Color { get; set; }

    public override WorldActor Deserialize(BinaryReader reader)
    {
        throw new InvalidOperationException();
    }
}
