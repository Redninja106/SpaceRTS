using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpaceGame.GUI;
//internal class ElementStack : Element
//{
//    public IEnumerable<Element> Elements => elements;

//    private readonly List<Element> elements = [];
//    private readonly List<Vector2> localPositions = [];

//    public bool DrawBorder { get; set; } = false;

//    public ElementStack() : this([])
//    {
//    }

//    public ElementStack(IEnumerable<Element> elements)
//    {
//        AddRange(elements);
//    }

//    public void Clear()
//    {
//        elements.Clear();
//        localPositions.Clear();
//    }

//    public void Add(Element element)
//    {
//        elements.Add(element);
//        localPositions.Add(Vector2.Zero);
//    }

//    public void AddRange(IEnumerable<Element> elements)
//    {
//        foreach (var elem in elements)
//        {
//            Add(elem);
//        }
//    }

//    public override void Render(ICanvas canvas)
//    {
//        if (DrawBorder)
//        {
//            canvas.Stroke(ShadowColor);
//            canvas.DrawRect(2, 2, Width, Height);
//            canvas.Stroke(ForegroundColor);
//            canvas.DrawRect(0, 0, Width, Height);
//        }

//        for (int i = 0; i < elements.Count; i++)
//        {
//            canvas.PushState();
//            canvas.Translate(localPositions[i]);
//            elements[i].Render(canvas);
//            canvas.PopState();
//        }
//    }

//    public override void UpdateSize(float containerWidth, float containerHeight)
//    {
//        foreach (var element in elements)
//        {
//            element.UpdateSize(containerWidth - Margin * 2, containerHeight - Margin * 2);
//        }

//        float cursorX, cursorY;
//        cursorX = cursorY = Margin;
//        for (int i = 0; i < elements.Count; i++)
//        {
//            var element = elements[i];
//            var areaWidth = containerWidth - 2 * Margin;
//            float xOffset = 0;
//            if (element.Alignment is Alignment.Center or Alignment.TopCenter or Alignment.BottomCenter)
//            {
//                xOffset = (areaWidth - element.Width) / 2f;
//            }
//            else if (element.Alignment is Alignment.TopRight or Alignment.CenterRight or Alignment.BottomRight)
//            {
//                xOffset = areaWidth - element.Width;
//            }

//            localPositions[i] = new(cursorX + xOffset, cursorY);

//            cursorY += element.Height + (i == elements.Count - 1 ? 0 : element.Margin);
//        }
//        Height = cursorY + Margin;
//        Width = containerWidth;
//    }

//    public override void Update(float locationX, float locationY)
//    {
//        for (int i = 0; i < elements.Count; i++)
//        {
//            var pos = localPositions[i];
//            elements[i].Update(locationX + pos.X, locationY + pos.Y);
//        }

//        base.Update(locationX, locationY);
//    }

//    public void RemoveAt(int index)
//    {
//        elements.RemoveAt(index);
//    }
//}
