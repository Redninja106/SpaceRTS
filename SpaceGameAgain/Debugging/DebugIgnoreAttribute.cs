using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Debugging;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
internal class DebugIgnoreAttribute : Attribute
{
}
