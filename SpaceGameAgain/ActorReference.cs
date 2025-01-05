using System.Diagnostics.CodeAnalysis;

namespace SpaceGame;

// used for late binding during object serialization
struct ActorReference<TActor>
    where TActor : WorldActor
{
    public static ActorReference<TActor> Null => default;

    private ulong id;
    private TActor? actor;

    [MemberNotNullWhen(false, nameof(Actor))]
    public bool IsNull => id == 0;
    public ulong ID => id;

    public TActor? Actor
    {
        get
        {
            if (actor != null)
            {
                return actor;
            }
            if (id == 0)
            {
                return null;
            }
            return actor = (TActor)World.Actors[id];
        }
    }

    public static ActorReference<TActor> Create(ulong id)
    {
        return new ActorReference<TActor>() { id = id, actor = null };
    }

    public static ActorReference<TActor> Create(TActor actor)
    {
        return new ActorReference<TActor>() { id = actor.ID, actor = actor };
    }

    public static bool operator ==(ActorReference<TActor> a, ActorReference<TActor> b)
    {
        return a.id == b.id;
    }

    public static bool operator !=(ActorReference<TActor> a, ActorReference<TActor> b)
    {
        return a.id != b.id;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ActorReference<TActor> act && act == this;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    public ActorReference<T> Cast<T>()
        where T : WorldActor
    {
        if (actor == null)
        {
            return ActorReference<T>.Create(id);
        }
        
        if (actor is T result)
        {
            return ActorReference<T>.Create(result);
        }

        throw new InvalidCastException();
    }
}
