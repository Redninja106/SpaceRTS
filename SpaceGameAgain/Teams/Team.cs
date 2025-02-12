using SpaceGame.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Teams;
internal class Team : WorldActor
{
    public static readonly Color PlayerColor = Color.SkyBlue;
    public static readonly Color AllyColor = Color.LawnGreen;
    public static readonly Color NeutralColor = Color.LightGray;
    public static readonly Color EnemyColor = Color.Red;

    private Dictionary<ActorReference<Team>, TeamRelation> relationships = [];
    private Dictionary<string, int> resources = [];

    public ICommandProcessor CommandProcessor;

    public Dictionary<string, int> Resources => resources;

    public Team(TeamPrototype prototype, ulong id, Transform transform, Dictionary<ActorReference<Team>, TeamRelation>? relationships = null, Dictionary<string, int>? resources = null) : base(prototype, id, transform)
    {
        if (relationships != null)
        {
            this.relationships = relationships;
        }
        else
        {
            this.relationships = new()
            {
                [this.AsReference()] = TeamRelation.Self,
            };
        }

        if (resources != null)
        {
            this.resources = resources;
        }
        else
        {
            this.resources = new()
            {
                ["metals"] = 100000,
            };
        }
    }

    public void MakeEnemies(Team other)
    {
        this.relationships.Add(other.AsReference(), TeamRelation.Enemies);
        other.relationships.Add(this.AsReference(), TeamRelation.Enemies);
    }

    public TeamRelation GetRelation(Team other)
    {
        return relationships.TryGetValue(other.AsReference(), out var result) ? result : TeamRelation.Neutral;
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

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);

        writer.Write(relationships.Count);
        foreach (var (team, relation) in relationships)
        {
            writer.Write(team);
            writer.Write((int)relation);
        }

        writer.Write(resources.Count);
        foreach (var resource in resources)
        {
            writer.Write(resource.Key);
            writer.Write(resource.Value);
        }
    }

    public int GetResource(string resource)
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