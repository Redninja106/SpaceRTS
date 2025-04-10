using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal static class Icons
{
    public static readonly ITexture Ship= Graphics.LoadTexture("Assets/Icons/ship.png");
    public static readonly ITexture Structure = Graphics.LoadTexture("Assets/Icons/structure.png");
    public static readonly ITexture Construction = Graphics.LoadTexture("Assets/Icons/construction.png");
    public static readonly ITexture Defensive = Graphics.LoadTexture("./Assets/Icons/defensive.png");
    public static readonly ITexture Industrial = Graphics.LoadTexture("./Assets/Icons/industrial.png");
    public static readonly ITexture Economic = Graphics.LoadTexture("./Assets/Icons/economic.png");
    public static readonly ITexture Research = Graphics.LoadTexture("./Assets/Icons/research.png");
}
