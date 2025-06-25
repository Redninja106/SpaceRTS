using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Rendering;
internal class Icon : AssetPrototype
{
    [JsonIgnore]
    public ITexture Texture64x64 { get; private set; } = null!;
    [JsonIgnore]
    public ITexture Texture32x32 { get; private set; } = null!;
    [JsonIgnore]
    public ITexture Texture16x16 { get; private set; } = null!;

    public static Icon Default => Get("default_icon");

    public override void LoadAssets(string prototypePath)
    {
        Texture64x64 = Graphics.LoadTexture(Path.Combine(prototypePath, "64x64.png"));
        Texture32x32 = Graphics.LoadTexture(Path.Combine(prototypePath, "32x32.png"));
        Texture16x16 = Graphics.LoadTexture(Path.Combine(prototypePath, "16x16.png"));
    }

    public static Icon Get(string name)
    {
        return Prototypes.Get<Icon>(name);
    }
}
