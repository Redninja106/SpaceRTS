using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal static class ActorExtensions
{
    public static ActorReference<TActor> AsReference<TActor>(this TActor actor)
        where TActor : Actor
    {
        return ActorReference<TActor>.Create(actor);
    } 
}
