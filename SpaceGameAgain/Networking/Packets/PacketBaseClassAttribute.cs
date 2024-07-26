using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking.Packets;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal class PacketBaseClassAttribute : Attribute
{
}
