
using SpaceGame.Planets;
using SpaceGame.Structures;

namespace SpaceGame;

public class ActorList<TActor> : List<TActor>
    where TActor : Actor
{
    public GameWorld World { get; }

    public ActorList(GameWorld world)
    {
        this.World = world;
    }

    public bool AddIfApplicable(Actor actor)
    {
        if (actor is TActor tActor)
        {
            Add(tActor);
            return true;
        }

        return false;
    }

    public void Update(float tickProgress)
    {
        DebugMenu.PushMetric($"ActorList<{typeof(TActor)?.Name}>.Update");
        for (int i = 0; i < Count; i++)
        {
            var actor = this[i];
            actor.Update(tickProgress);
        }
        DebugMenu.PopMetric();
    }

    public void Tick()
    {
        DebugMenu.PushMetric($"ActorList<{typeof(TActor)?.Name}>.Tick");
        for (int i = 0; i < Count; i++)
        {
            var actor = this[i];
            actor.Tick();

            if (actor is IDestructable destructable && destructable.IsDestroyed)
            {
                destructable.OnDestroyed();
                World.Actors.Remove(actor.ID);
                this.RemoveAt(i);
                i--;
            }
        }
        DebugMenu.PopMetric();
    }

    public void Render(ICanvas canvas, Camera camera)
    {
        DebugMenu.PushMetric($"ActorList<{typeof(TActor)?.Name}>.Render");
        
        foreach (var actor in this)
        {
            float radius = actor switch
            {
                Unit unit => (float)unit.GetCollisionRadius(),
                Planet p => p.Radius,
                Grid g => ((Planet)g.parent).Radius,
                _ => 0,
            };

            if (camera.QuickDiscard(actor.InterpolatedTransform.Position, radius))
            {
                continue;
            }

            if (actor is Unit u && !World.Collision.IsClientVisible(actor.Transform.Position))
            {
                continue;
            }

            canvas.PushState();
            actor.InterpolatedTransform.ApplyTo(canvas, camera);
            actor.Render(canvas);
            canvas.PopState();
        }
        DebugMenu.PopMetric();
    }
}
