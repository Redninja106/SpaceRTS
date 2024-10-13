using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal class Separator : Element
{
    public override void Render(ICanvas canvas)
    {
        canvas.Stroke(ShadowColor);
        canvas.DrawRect(2, 2, 2 + this.Width, .1f);
        canvas.Stroke(ForegroundColor);
        canvas.DrawRect(0, 0, this.Width, .1f);
    }

    public override void UpdateSize(float containerWidth, float containerHeight)
    {
        this.Width = containerWidth;
        this.Height = 1;
    }
}
