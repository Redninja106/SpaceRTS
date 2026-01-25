using System.Diagnostics.CodeAnalysis;

namespace SpaceGame.Generation;

/// <summary>
/// A weighted set of T objects for selecting a random choice. Deserializes as an array of tuples (ie. <c>[[k, v], [k, v]]</c>).
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class RandomPrototype<T>
{
    public RandomInt? Count { get; init; }
    public WeightedObject[] Choices { get; init; }
    private int totalWeight = -1;

    public RandomPrototype(WeightedObject[] choices)
    {
        this.Choices = choices;
    }

    public T Get(Random random)
    {
        if (totalWeight < 0)
        {
            totalWeight = Choices.Sum(tuple => tuple.Weight);
        }

        int choice = random.Next(0, totalWeight);
        foreach (var c in Choices)
        {
            choice -= c.Weight;
            if (choice < 0)
            {
                return c.Choice;
            }
        }
        return Choices.Last().Choice;
    }

    public class WeightedObject
    {
        public WeightedObject(T choice)
        {
            ArgumentNullException.ThrowIfNull(choice);
            Choice = choice;
        }

        public int Weight { get; set; } = 1;
        public T Choice { get; set; } = default!;
    }
}

/*json



{
    count: 5,
    choices: [
        [12, {
        }]
        }
    ]
}








 */

