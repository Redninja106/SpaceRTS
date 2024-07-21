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
        canvas.Fill(ShadowColor);
        canvas.DrawLine(2, 2, 2+this.Width, 2);
        canvas.Fill(ForegroundColor);
        canvas.DrawLine(0, 0, this.Width, 0);
    }

    public override void UpdateSize(float containerWidth, float containerHeight)
    {
        this.Width = containerWidth;
        this.Height = 1;
    }
}
