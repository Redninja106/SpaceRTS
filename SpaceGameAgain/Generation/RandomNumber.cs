namespace SpaceGame.Generation;

public class RandomNumber
{
    public static readonly RandomNumber Zero = new RandomNumber() { Minimum = 0, Maximum = 0 };

    public double Minimum { get; init; }
    public double Maximum { get; init; }

    public double Get(Random random)
    {
        return Minimum + (Maximum - Minimum) * random.NextDouble();
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

