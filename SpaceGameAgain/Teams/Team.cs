using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Teams;
internal class Team : Actor
{
    public static readonly Color PlayerColor = Color.SkyBlue;
    public static readonly Color AllyColor = Color.LawnGreen;
    public static readonly Color NeutralColor = Color.LightGray;
    public static readonly Color EnemyColor = Color.Red;

    private Dictionary<ActorReference<Team>, TeamRelation> relationships = [];

    public int Credits;
    public int ZoneSizeLimit = 3;

    public Team(TeamPrototype prototype, ulong id, Transform transform, int credits = 10000, Dictionary<ActorReference<Team>, TeamRelation>? relationships = null) : base(prototype, id, transform)
    {
        this.Credits = credits;

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

        writer.Write(Credits);

        writer.Write(relationships.Count);
        foreach (var (team, relation) in relationships)
        {
            writer.Write(team);
            writer.Write((int)relation);
        }
    }
}

enum TeamRelation
{
    Neutral,
    Self,
    Allies,
    Enemies,
}