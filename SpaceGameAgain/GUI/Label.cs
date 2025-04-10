using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.GUI;
//internal class Label : Element
//{
//    public string text;
//    public float size;
//    private readonly TextStyle textStyle;

//    public Label(string text, float size = Element.TextSize, Alignment alignment = Alignment.CenterLeft, TextStyle textStyle = TextStyle.Regular)
//    {
//        this.text = text;
//        this.size = size;
//        this.textStyle = textStyle;
//        this.Alignment = alignment;
//    }

//    public override void UpdateSize(float containerWidth, float containerHeight)
//    {
//        (Width, Height) = Program.font.MeasureText(text, size).Size;
//    }

//    public override void Render(ICanvas canvas)
//    {
//        DrawShadowedText(canvas, text, size, Vector2.Zero, textStyle);
//    }
//}
