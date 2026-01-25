using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Generation;

public class RandomInt
{
    public static readonly RandomInt Zero = new RandomInt() { Minimum = 0, Maximum = 0 };
    public static readonly RandomInt One = new RandomInt() { Minimum = 1, Maximum = 1 };

    public int Minimum { get; set; }
    public int Maximum { get; set; }

    public int Get(Random random)
    {
        return random.Next(Minimum, Maximum + 1);
    }
}
