using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Economy;
internal static class PowerLevelUtil
{
    public static PowerLevel Max(PowerLevel a, PowerLevel b)
    {
        return (PowerLevel)Math.Max((int)a, (int)b);
    }
}
