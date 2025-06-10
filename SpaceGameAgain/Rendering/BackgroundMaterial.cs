using SpaceGame.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Rendering;
internal class BackgroundMaterial : Prototype
{
    public string TextureFile { get; set; }
    public string NormalMapFile { get; set; }
    public ITexture Texture;
    public ITexture? NormalMap;

    public override void InitializePrototype()
    {
        Texture = Graphics.LoadTexture("Assets/Textures/" + TextureFile);
        Graphics.GenerateMipmaps(Texture);
        Texture.Filter = TextureFilter.MipmapPoint;

        if (NormalMapFile != null)
        {
            NormalMap = Graphics.LoadTexture("Assets/Textures/" + NormalMapFile);
            Graphics.GenerateMipmaps(NormalMap);
            NormalMap.Filter = TextureFilter.MipmapPoint;
        }

        base.InitializePrototype();
    }

    public override Actor Deserialize(BinaryReader reader)
    {
        throw new NotSupportedException();
    }
}
