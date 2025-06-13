namespace SpaceGame;

class ActorList<TActor> : List<TActor>
    where TActor : Actor
{
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
        for (int i = 0; i < Count; i++)
        {
            var actor = this[i];
            actor.Update(tickProgress);
        }
    }

    public void Tick()
    {
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