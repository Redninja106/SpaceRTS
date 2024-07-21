using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal class ProgressBar(Func<float> progress) : Element
{
    public override void Render(ICanvas canvas)
    {
        canvas.Fill(ShadowColor);
        canvas.DrawRect(new(0, 0, Width+2, Height+2));
        canvas.Fill(ForegroundColor);
        canvas.DrawRect(new(0, 0, Width, Height));
        canvas.Fill(Color.Red);
        canvas.DrawRect(new(0, 0, Width * progress(), Height));
    }

    public override void UpdateSize(float containerWidth, float containerHeight)
    {
        this.Width = containerWidth;
        this.Height = 4;
    }

    public override void Update(float locationX, float locationY)
    {
    }
}
