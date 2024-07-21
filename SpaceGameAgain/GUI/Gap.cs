using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
internal class Gap(float height) : Element
{
    public override void Render(ICanvas canvas)
    {
    }

    public override void UpdateSize(float containerWidth, float containerHeight)
    {
        Width = 1;
        Height = height;
    }
}
