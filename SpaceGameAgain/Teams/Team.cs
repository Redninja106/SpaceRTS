using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Teams;
internal class Team
{
    public static readonly Color PlayerColor = Color.SkyBlue;
    public static readonly Color AllyColor = Color.LawnGreen;
    public static readonly Color NeutralColor = Color.LightGray;
    public static readonly Color EnemyColor = Color.Red;

    private Dictionary<Team, TeamRelation> relationships = [];

    public int Materials = 10000;

    public Team()
    {
        relationships.Add(this, TeamRelation.Self);
    }

    public void MakeEnemies(Team other)
    {
        this.relationships.Add(other, TeamRelation.Enemies);
        other.relationships.Add(this, TeamRelation.Enemies);
    }

    public TeamRelation GetRelation(Team other)
    {
        return relationships.TryGetValue(other, out var result) ? result : TeamRelation.Neutral;
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
}

enum TeamRelation
{
    Neutral,
    Self,
    Allies,
    Enemies,
}