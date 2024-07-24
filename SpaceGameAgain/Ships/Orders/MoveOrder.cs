using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Orders;
internal class MoveOrder : Order
{
    public List<Vector2> targets = [];

    public MoveOrder(Vector2 target)
    {
        targets.Add(target);
    }

    public override bool Complete(Ship ship)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            var soi = World.GetSphereOfInfluence(targets[i]);
            Transform t = Transform.Default.Translated(targets[i]);
            soi?.ApplyTo(ref t);
            targets[i] = t.Position;
        }

        if (MoveTo(ship, targets[0]))
        {
            targets.RemoveAt(0);
        }

        return targets.Count == 0;
    }


    public override void Render(Ship ship, ICanvas canvas)
    {
        canvas.PopState();
        canvas.Stroke(new Color(200, 200, 200, 200));
        Vector2 shipPos = ship.Transform.Position;
        foreach (var target in targets)
        {
            canvas.DrawLine(shipPos, target);
            shipPos = target;
        }
        canvas.PushState();
    }
}
