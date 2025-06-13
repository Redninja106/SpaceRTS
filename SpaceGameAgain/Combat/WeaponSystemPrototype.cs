using SpaceGame.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal abstract class WeaponSystemPrototype : Prototype
{
    public float Range { get; set; } = 1;
    public SpriteModel? Model { get; set; }

    public override WeaponSystem CreateActor(ulong id) => (WeaponSystem)base.CreateActor(id);
}
