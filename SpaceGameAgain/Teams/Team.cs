using ImGuiNET;
using SpaceGame.Commands;
using SpaceGame.Economy;
using SpaceGame.Serialization;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Teams;

[Serializable]
internal class Team : Actor
{
    public override TeamPrototype Prototype => (TeamPrototype)base.Prototype;

    public static readonly Color PlayerColor = Color.SkyBlue;
    public static readonly Color AllyColor = Color.LawnGreen;
    public static readonly Color NeutralColor = Color.LightGray;
    public static readonly Color EnemyColor = Color.Red;

    private Dictionary<Team, TeamRelation> relationships = [];
    internal Dictionary<ResourcePrototype, ResourceValues> resources = [];

    private HashSet<UnitPrototype> unlockedUnits = [];

    [field: Serialize]
    public string Name { get; set; }
    [field: Serialize]
    public int Money { get; set; }

    private ICommandProcessor CommandProcessor;

    public void Unlock(UnitPrototype unit)
    {
        unlockedUnits.Add(unit);
    }

    public bool IsUnlocked(UnitPrototype unit)
    {
        return unlockedUnits.Contains(unit);
    }

    public Team(TeamPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
        // this.Money = money;
        this.Name = "Player " + ID;

        if (relationships != null)
        {
            this.relationships = relationships;
        }
        else
        {
            this.relationships = new()
            {
                [this] = TeamRelation.Self,
            };
        }

        this.resources = new();
        foreach (var resource in Prototypes.GetAll<ResourcePrototype>())
        {
            this.resources[resource] = new()
            {
                Capacity = 0,
                Consumption = 0,
            };
        }
    }

    public ICommandProcessor GetCommandProcessor()
    {
        if (CommandProcessor == null)
        {
            CommandProcessor = this.Prototype.CreateCommandProcessor(World, this);
        }

        return CommandProcessor;
    }

    public void MakeEnemies(Team other)
    {
        // this.relationships.Add(other.AsReference(), TeamRelation.Enemies);
        // other.relationships.Add(this.AsReference(), TeamRelation.Enemies);
    }

    public TeamRelation GetRelation(Team other)
    {
        if (other == this)
        {
            return TeamRelation.Self;
        }

        return TeamRelation.Enemies;

        // return relationships.TryGetValue(other.AsReference(), out var result) ? result : TeamRelation.Neutral;
    }

    public Color GetRelationColor(Team team)
    {
        return GetRelation(team) switch
        {
            TeamRelation.Neutral => NeutralColor,
            TeamRelation.Self => PlayerColor,
            TeamRelation.Allies => AllyColor,
            TeamRelation.Enemies => EnemyColor,
        };
    }

    public override void DebugLayout()
    {
        base.DebugLayout();

        ImGui.Text("money: " + Money);

        foreach (var (resource, values) in resources)
        {
            ImGui.Text($"{resource.Name}: {values.Remaining} ({values.Capacity} - {values.Consumption})");
        }
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ID);

    //    writer.Write(Money);
    //    writer.Write(relationships.Count);
    //    foreach (var (team, relation) in relationships)
    //    {
    //        writer.Write(team);
    //        writer.Write((int)relation);
    //    }
    //}

    public ResourceValues GetResource(ResourcePrototype resource)
    {
        return resources[resource];
    }

}

enum TeamRelation
{
    Neutral,
    Self,
    Allies,
    Enemies,
}

class ResourceValues
{
    public int Consumption;
    public int Capacity;

    public int Remaining => Capacity - Consumption;
}