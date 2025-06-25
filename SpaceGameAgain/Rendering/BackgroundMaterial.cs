using Newtonsoft.Json;
using SpaceGame.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Rendering;
internal class BackgroundMaterial : AssetPrototype
{
    public required string TextureFile { get; set; }
    public required string NormalMapFile { get; set; }

    [JsonIgnore]
    public ITexture Texture = null!;
    [JsonIgnore]
    public ITexture? NormalMap = null;

    public override void LoadAssets(string prototypeDirectory)
    {
        Texture = Graphics.LoadTexture(Path.Combine(prototypeDirectory, TextureFile));
        Graphics.GenerateMipmaps(Texture);
        Texture.Filter = TextureFilter.MipmapPoint;

        if (NormalMapFile != null)
        {
            NormalMap = Graphics.LoadTexture(prototypeDirectory + NormalMapFile, TextureOptions.Constant);
            Graphics.GenerateMipmaps(NormalMap);
            NormalMap.Filter = TextureFilter.MipmapPoint;
        }
    }
}
