using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal interface ITargetable : IActor, IDamagable
{
    public DoubleVector Velocity { get; }
    public DoubleVector CurrentAcceleration { get; }
    public DoubleVector LastAcceleration { get; }
}
