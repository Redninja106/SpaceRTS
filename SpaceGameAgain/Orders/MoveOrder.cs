using SpaceGame.Extensions;
using SpaceGame.Ships;
using SpaceGame.Stations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;

[Serializable]
internal class MoveOrder : Order
{

    [field: Serialize]
    public required DoubleVector TargetPosition { get; set; }

    [field: Serialize]
    public DoubleVector ForecastedTargetPosition { get; set; }
    [field: Serialize]
    public WormholeStation? Wormhole { get; set; } = null;

    private ulong decelTick;

    public override void Tick()
    {
        var soi = Ship.World.GetSphereOfInfluence(TargetPosition);
        if (soi != null)
        {
            TargetPosition = soi.ApplyTickTo(TargetPosition);
        }

        //if (Unit is Ship ship && ship.WormholeStation is WormholeStation wormhole)
        //{   
        //    if (wormhole.Link != null && wormhole.ships.Contains(this.Unit))
        //    {
        //        double thisWormholeDistance = DoubleVector.Distance(this.Unit.Transform.Position, TargetPosition);
        //        DoubleVector wormholeOffset = this.Unit.Transform.Position - wormhole.Transform.Position;
        //        double linkWormholeDistance = DoubleVector.Distance(wormhole.Link.Transform.Position + wormholeOffset, TargetPosition);

        //        if (linkWormholeDistance < thisWormholeDistance)
        //        {
        //            wormhole.WarpShip(ship);
        //        }
        //    }
        //}

        if (MoveTo(ForecastedTargetPosition))
        {
            if (Wormhole != null)
            {
                Wormhole.WarpShip((Ship)this.Ship);
            }

            Complete();
        }
    }

    public override void OnEnqueued(Ship ship)
    {
        base.OnEnqueued(ship);
        ForecastTarget();
    }

    //public override void Update(float tickProgress)
    //{
    //    base.Update(tickProgress);
    //}

    public override void RenderOverlay(ICanvas canvas, ref Transform startTransform, ref Transform forecastedStartTransform)
    {
        RenderOverlayLines(canvas, ref startTransform, TargetPosition);
        RenderOverlayLines(canvas, ref forecastedStartTransform, ForecastedTargetPosition);
    }

    private void RenderOverlayLines(ICanvas canvas, ref Transform transform, DoubleVector target)
    {
        canvas.PushState();
        canvas.ResetState();
        transform.PositionOnly().ApplyTo(canvas, Ship.World.Camera);

        canvas.Stroke(Color.White with { A = 200 });
        canvas.DrawLine(Vector2.Zero, (target - transform.Position).ToVector2());
        if (Wormhole != null)
        {
            canvas.DrawLine(Vector2.Zero, (Wormhole.Link!.InterpolatedTransform.Position - transform.Position).ToVector2());
        }
        canvas.PopState();


        if (Wormhole != null)
        {
            transform.Position = Wormhole.Link!.Transform.Position;
        }
        else
        {
            transform.Position = target;
        }
    }

    public bool MoveTo(DoubleVector targetPosition)
    {
        ShipPrototype prototype = Ship.Prototype;
        var s = Ship;

        var delta = targetPosition - s.Transform.Position;
        double dist = delta.Length();
        if (dist == 0 && s.velocity.Length() == 0)
        {
            return true;
        }

        double parallelLength = DoubleVector.Dot(s.velocity, delta) / dist;
        DoubleVector parallel = delta.Normalized() * parallelLength;
        DoubleVector perp = s.velocity - parallel;
        // DebugDraw.Ray(Vector2.Zero, parallel.ToVector2(), s.Transform with { Rotation = 0 });
        // DebugDraw.Ray(Vector2.Zero, perp.ToVector2(), s.Transform with { Rotation = 0 });

        // para     perp    angle
        // 1        0       0
        // 0        1       pi/2
        // .5       .5      pi/4

        //if (!TurnTo(Angle.FromVector(delta.ToVector2()) + double.Sign(DoubleVector.Cross(perp, delta)) * float.Atan2((float)perp.Length(), (float)parallel.Length())))
        //{
        //    return false;
        //}

        float targetAngle = Angle.FromVector(delta.ToVector2());
        if (!TurnTo(targetAngle))
        {
            return false;
        }

        float timeToTarget = (float)delta.Length() / (float)s.velocity.Length();
        float timeToStop = (float)s.velocity.Length() / prototype.FlySpeed;

        DoubleVector targetVelocity;
        if (timeToTarget <= timeToStop + Program.Timestep * 10 || perp.LengthSquared() > 0.0001)
        {
            targetVelocity = DoubleVector.Zero;
        }
        else
        {
            targetVelocity = delta.Normalized() * 1_000_000;
        }

        // DebugDraw.Ray(Vector2.Zero, targetVelocity.ToVector2(), s.Transform with { Rotation = 0 });
        // DebugDraw.Ray(Vector2.Zero, s.velocity.ToVector2(), s.Transform with { Rotation = 0 });
        float accel = s.Prototype.FlySpeed * Program.Timestep;

        DoubleVector oldVel = s.velocity;
        s.velocity = DoubleVector.Step(s.velocity, targetVelocity, accel);
        s.CurrentAcceleration = s.velocity - oldVel;
        //if (DoubleVector.Dot(delta, s.velocity) > 0 && timeToTarget < timeToStop)
        //{
        //    s.Fly(-1);
        //}
        //else
        //{
        //    if (delta.Length() > 1)
        //    {
        //        s.velocity = DoubleVector.Step(s.velocity, delta, accel);
        //    }
        //    else
        //    {
        //        s.velocity = DoubleVector.Step(s.velocity, delta, accel);
        //    }
        //        //DoubleVector.Step(delta.Normalized(),, );
        //        // MathHelper.Step();
        //}

        // DebugDraw.Text(timeToTarget + " < " + timeToStop, 1, default, s.Transform with { Rotation = 0 }, timeToTarget < timeToStop ? Color.Red : Color.Green );
        // DebugDraw.Ray(default, s.velocity.ToVector2(), s.Transform with { Rotation = 0 });

        //if (Vector2.DistanceSquared(targetPosition.ToVector2(), s.Transform.Position.ToVector2()) > 0.01f)
        //{
        //    if (Angle.Distance(transform.Rotation, Angle.FromVector(delta.ToVector2())) > 0f)
        //    {
        //        transform.Rotation = Angle.Step(transform.Rotation, Angle.FromVector(delta.ToVector2()), prototype.TurnSpeed * MathF.Tau * Program.Timestep);
        //        if (Angle.Distance(transform.Rotation, Angle.FromVector(delta.ToVector2())) > .01f)
        //        {
        //            return false;
        //        }
        //    }

        //    transform.Position = Util.Step(transform.Position, targetPosition, prototype.FlySpeed * Program.Timestep);
        //    return false;
        //}

        //if (targetRotation != null && Angle.Distance(transform.Rotation, targetRotation.Value) > 0f)
        //{
        //    transform.Rotation = Angle.Step(transform.Rotation, targetRotation.Value, prototype.TurnSpeed * MathF.Tau * Program.Timestep);
        //    return false;
        //}

        if (DoubleVector.Distance(s.Transform.Position, targetPosition) < 0.01 && s.velocity.Length() <= 0.01)
        {
            s.Transform.Position = targetPosition;
            return true;
        }
        
        return false;
    }

    [DebugOverlay]
    static bool ShowTurningAngles = false;

    public bool TurnTo(float rotation)
    {
        ShipPrototype prototype = (ShipPrototype)Ship.Prototype;
        Ship s = (Ship)Ship!;

        float delta = Angle.SignedDistance(rotation, s.Transform.Rotation);
        float turnSpeedRadians = prototype.TurnSpeed * MathF.Tau;

        float timeToStop = MathF.Abs(s.angularVelocity) / turnSpeedRadians;
        float timeToTarget = MathF.Abs(delta / s.angularVelocity);

        if (timeToTarget < timeToStop)
        {
            //s.DoAngularThrust(-MathF.Sign(s.angularVelocity));
        }
        else
        {
            //s.DoAngularThrust(MathF.Sign(delta));
        }

        if (ShowTurningAngles)
        {
            Color c = timeToTarget <= timeToStop ? Color.Red : Color.Green;
            DebugDraw.Ray(Vector2.Zero, Angle.ToVector(delta), s.Transform, c);
            DebugDraw.Ray(Vector2.Zero, Vector2.UnitX, s.Transform, c);
        }

        s.Transform.Rotation = Angle.Step(s.Transform.Rotation, rotation, s.Prototype.TurnSpeed * float.Tau * Program.Timestep);

        if (Angle.Distance(s.Transform.Rotation, rotation) < 0.01 && float.Abs(s.angularVelocity) < 0.01)
        {
            s.Transform.Rotation = rotation;
            s.angularVelocity = 0;
            return true;
        }

        return false;
    }

    public bool MoveToOld(DoubleVector targetPosition, float? targetRotation = null)
    {
        ShipPrototype prototype = (ShipPrototype)Ship.Prototype;

        ref Transform transform = ref Ship.Transform;

        var delta = targetPosition - transform.Position;

        if (Vector2.DistanceSquared(targetPosition.ToVector2(), transform.Position.ToVector2()) > 0.01f)
        {
            if (Angle.Distance(transform.Rotation, Angle.FromVector(delta.ToVector2())) > 0f)
            {
                transform.Rotation = Angle.Step(transform.Rotation, Angle.FromVector(delta.ToVector2()), prototype.TurnSpeed * MathF.Tau * Program.Timestep);
                if (Angle.Distance(transform.Rotation, Angle.FromVector(delta.ToVector2())) > .01f)
                {
                    return false;
                }
            }

            transform.Position = Util.Step(transform.Position, targetPosition, prototype.FlySpeed * Program.Timestep);
            return false;
        }

        if (targetRotation != null && Angle.Distance(transform.Rotation, targetRotation.Value) > 0f)
        {
            transform.Rotation = Angle.Step(transform.Rotation, targetRotation.Value, prototype.TurnSpeed * MathF.Tau * Program.Timestep);
            return false;
        }

        return true;
    }

    public void ForecastTarget()
    {
        var soi = Ship.World.GetSphereOfInfluence(TargetPosition);

        if (soi != null && soi.planet.orbit != null)
        {
            DoubleVector forecastedPosition = TargetPosition;
            DoubleVector delta = TargetPosition - soi.planet.Transform.Position;
            for (int i = 0; i < 8; i++)
            {
                float time = (float)ShipNavigator.TurnTime((ShipPrototype)Ship.Prototype, Ship.Transform.Rotation, Angle.FromVector(delta.ToVector2())) + (float)ShipNavigator.CalculateTravelTime((ShipPrototype)Ship.Prototype, Ship.Transform.Position, forecastedPosition);
                forecastedPosition = soi.planet.orbit.Forecast(time) + delta;
            }

            this.ForecastedTargetPosition = forecastedPosition;
        }
        else
        {
            this.ForecastedTargetPosition = TargetPosition;
        }
    }


    //private static float CalculateTravelTime(ShipPrototype prototype, Transform currentTransform, DoubleVector currentVelocity, DoubleVector targetPosition)
    //{
    //    static float RotationTime(float currentRotation,)
    //    {

    //    }
    //}


}

struct ShipSnapshot
{
    public Transform transform;
    public DoubleVector velocity;

    public static ulong TimeToTargetVelocity(ShipPrototype prototype, ShipSnapshot current, ShipSnapshot target)
    {
        return (ulong)(Program.Timestep * DoubleVector.Distance(current.velocity, target.velocity) / (double)prototype.FlySpeed);
    }

    //private static float TransferTime(ShipPrototype prototype, ShipSnapshot initial, ShipSnapshot final)
    //{
    //    double d = DoubleVector.Distance(initial.transform.Position, final.transform.Position);
    //    double a = prototype.FlySpeed;

    //    float discriminant = (a * a) * (2 * a * d) + ()
    //}
}

class ShipNavigator(GameWorld World)
{
    PriorityQueue<NavigationNode, float> frontier = new();
    Dictionary<NavigationNode, float> costSoFar = [];
    Dictionary<NavigationNode, NavigationNode> cameFrom= [];

    private NavigationNode targetNode;
    private NavigationNode startNode;

    private List<NavigationNode> wormholeNodes = World.Stations.Select(w => new NavigationNode() { station = (WormholeStation)w }).ToList();

    public List<MoveOrder> GetPath(Ship ship, DoubleVector target)
    {
        startNode = new() { position = ship.Transform.Position };
        targetNode = new() { position = target };

        frontier.Enqueue(startNode, 0);
        costSoFar[startNode] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current.station == null && current.position == target)
            {
                return CreatePath(ship, current);
            }

            foreach (var next in GetNeighbors(current))
            {
                float cost = costSoFar[current] + CalculateCost(ship.Prototype, current, next);
                if (!costSoFar.TryGetValue(next, out var oldCost) || cost < oldCost)
                {
                    costSoFar[next] = cost;
                    cameFrom[next] = current;
                    frontier.Enqueue(next, cost);
                }
            }
        }

        return []; // no path?

        //float baseTravelTime = CalculateTravelTime(ship.Prototype, ship.Transform.Position, target);
        //foreach (var wormhole in wormholeNodes)
        //{
        //    float wormholeTime = CalculateTravelTime(ship.Prototype, ship.Transform.Position, wormhole.position);
        //    if (wormholeTime < baseTravelTime)
        //    {
        //        frontier.Enqueue(wormhole, wormholeTime);
        //        costSoFar[wormhole] = wormholeTime;
        //        cameFrom[wormhole] = new() { position = target };
        //    }
        //}
    }

    private float CalculateCost(ShipPrototype shipPrototype, NavigationNode from, NavigationNode to)
    {
        

        DoubleVector startPosition;
        if (from.station != null)
        {
            startPosition = from.station.Link!.Transform.Position;
        }
        else
        {
            startPosition = from.position;
        }

        DoubleVector endPosition;
        if (to.station != null)
        {
            endPosition = to.station.Transform.Position;
        }
        else
        {
            endPosition = to.position;
        }

        return (float)CalculateTravelTime(shipPrototype, startPosition, endPosition);
    }

    private IEnumerable<NavigationNode> GetNeighbors(NavigationNode node)
    {
        yield return targetNode;
        
        foreach (var wormhole in wormholeNodes)
        {
            yield return wormhole;
        }
    }

    private List<MoveOrder> CreatePath(Ship ship, NavigationNode dest)
    {
        List<MoveOrder> list = [];

        NavigationNode node = dest;
        while (node != startNode)
        {
            MoveOrder order = new MoveOrder()
            {
                TargetPosition = node.GetPosition(),
                Wormhole = node.station,
            };
            list.Add(order);
            node = cameFrom[node];
        }

        return list;
    }
    
    public static double CalculateTravelTime(ShipPrototype prototype, DoubleVector from, DoubleVector to)
    {
        // simple equation for this is sqrt(distance/acceleration) (inverse of .5at^2)
        // this is the inverse but with the error correction factor .5t*timestep

        double a = prototype.FlySpeed;
        double d = DoubleVector.Distance(from, to);

        double discriminant = 4 * a * d + (Program.Timestep * Program.Timestep);

        return 2 * (double.Sqrt(discriminant) - Program.Timestep) / (2 * a);
        // return 2 * (float.Sqrt((float)DoubleVector.Distance(from, to) / prototype.FlySpeed) + .5f * t);
    }

    public static double TurnTime(ShipPrototype prototype, float currentAngle, float targetAngle)
    {
        //double a = prototype.TurnSpeed * float.Tau;
        //double d = (double)Angle.Distance(currentAngle, targetAngle);
        //double discriminant = 4 * a * d + (Program.Timestep * Program.Timestep);
        //return 2 * (double.Sqrt(discriminant) - Program.Timestep) / (2 * a);
        return Angle.Distance(currentAngle, targetAngle) / (prototype.TurnSpeed * float.Tau);

    }

    class NavigationNode
    {
        public DoubleVector position;
        public WormholeStation? station;

        public DoubleVector GetPosition()
        {
            if (station != null)
            {
                return station.Transform.Position;
            }
            return position;
        }
    }
}
