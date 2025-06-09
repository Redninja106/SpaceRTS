namespace SpaceGame;

class WorldActorList<TActor> : List<TActor>
    where TActor : WorldActor
{
    public bool AddIfApplicable(WorldActor actor)
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
        for (int i = 0; i < Count; i++)
        {
            this[i].Update(tickProgress);
        }
    }

    public void Tick()
    {
        for (int i = 0; i < Count; i++)
        {
            this[i].Tick();

            if (this[i] is IDestructable destructable && destructable.IsDestroyed)
            {
                destructable.OnDestroyed();
                World.Actors.Remove(this[i].ID);
                this.RemoveAt(i);
                i--;
            }
        }
    }

    public void Render(ICanvas canvas, Camera camera)
    {
        foreach (var actor in this)
        {
            if (actor is Unit u && u.ClientVisible)
            {
                continue;
            }

            canvas.PushState();
            actor.InterpolatedTransform.ApplyTo(canvas, camera);
            actor.Render(canvas);
            canvas.PopState();
        }
    }
}