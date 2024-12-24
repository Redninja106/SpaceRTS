using SpaceGame.Combat;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class TurretPrototype : StructurePrototype
{
    public string WeaponSystemPrototype { get; set; }

    public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    {
        var turret = new Turret(this, id, grid, location, rotation, team);
        var weaponSystem = Prototypes.Get<WeaponSystemPrototype>(WeaponSystemPrototype).CreateWeapon(World.NewID(), ((Unit)turret).AsReference());
        World.Add(weaponSystem);

        return turret;
    }

    public TurretPrototype(string title, int price, Model model, string? presetModel, HexCoordinate[] footprint) : base(title, price, model, presetModel, footprint)
    {
    }

    public override Actor? Deserialize(BinaryReader reader)
    {
        base.DeserializeArgs(reader, out var id, out var team, out var grid, out var location, out var rotation);
        ActorReference<WeaponSystem> weaponSystem = reader.ReadActorReference<WeaponSystem>();

        return new Turret(this, id, grid, location, rotation, team)
        {
            weaponSystem = weaponSystem,
        };
    }
}
