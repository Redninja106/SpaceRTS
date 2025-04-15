using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class NSpriteModel : ModelPrototype
{

    private ITexture[] models;
    public string SpritesFolder { get; set; }
    public int SpriteCount { get; set; }

    public override void InitializePrototype()
    {
        models = new ITexture[SpriteCount];
        for (int i = 0; i < SpriteCount; i++)
        {
            models[i] = Graphics.LoadTexture($"./Assets/Sprites/{SpritesFolder}/{i}.png");
            models[i].Filter = TextureFilter.MipmapPoint;
            Graphics.GenerateMipmaps(models[i]);
        }

        base.InitializePrototype();
    }

    public override Actor Deserialize(BinaryReader reader)
    {
        throw new NotSupportedException();
    }

    public virtual void Render(ICanvas canvas, Transform transform, ColorF tint)
    {
        int sprite = (int)MathF.Round((Angle.Normalize(transform.Rotation) / MathF.Tau) * SpriteCount) % SpriteCount;
        canvas.DrawTexture(models[sprite], new Rectangle(0, 0, Width, Height, Alignment.Center), tint);
    }
}
