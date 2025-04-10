using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Networking;
internal class NetworkSettings
{
    public const int MaxPacketSize = 64 * 1024;
    public const int DefaultPort = 45454;

    [DebugOverlay]
    public static bool LogIncomingPackets;
    [DebugOverlay]
    public static bool LogOutgoingPackets;

}
