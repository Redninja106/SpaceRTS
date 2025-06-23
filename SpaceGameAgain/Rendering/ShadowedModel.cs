using SpaceGame.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Rendering;
internal class ShadowedModel : SpriteModel
{
    const int ShadowCount = 16;

    private ITexture[][] shadowMaps;

    public override void InitializePrototype()
    {
        base.InitializePrototype();

        shadowMaps = new ITexture[SpriteCount][];

        for (int i = 0; i < SpriteCount; i++)
        {
            shadowMaps[i] = new ITexture[ShadowCount];
            for (int j = 0; j < ShadowCount; j++)
            {
                var tex = Graphics.LoadTexture($"./Assets/Sprites/{SpritesFolder}/{i}_{j}.png");
                tex.Filter = TextureFilter.MipmapPoint;
                Graphics.GenerateMipmaps(tex);
                shadowMaps[i][j] = tex;
            }
        }
    }

    public override void Render(ICanvas canvas, Transform transform, ColorF tint)
    {
        // transform.Position = World.MousePosition;
        Vector2 v = -transform.Position.Normalized().ToVector2();
        float shadowAngle = Angle.FromVector(-transform.Position.Normalized().ToVector2());
        DebugDraw.Line(Vector2.Zero, Vector2.UnitX.Rotated(shadowAngle), transform with { Rotation = 0 });
        shadowAngle = Angle.Normalize(-shadowAngle + MathF.PI / 2);
        if (!float.IsNaN(shadowAngle))
        {
            int shadowIdx = (int)(MathF.Round(shadowAngle / float.Tau * ShadowCount) % ShadowCount);

            int sprite = (int)MathF.Round(Angle.Normalize(transform.Rotation) / MathF.Tau * SpriteCount) % SpriteCount;

            float d = Angle.SignedDistance(shadowAngle, shadowIdx * MathF.Tau / ShadowCount);
            canvas.PushState();
            // canvas.DrawArc(0, 0, 1, 1, shadowAngle, shadowAngle + d, true);
            // canvas.Transform(Matrix3x2.CreateRotation(-d));
            // if (shadowAngle < MathF.PI/2 || shadowAngle > 3f/2*MathF.PI)
            // {
            //     canvas.Transform(Matrix3x2.CreateSkew(0, d));
            // }
            // else
            // {
            //     canvas.Transform(Matrix3x2.CreateSkew(-d, 0));
            // }
            canvas.DrawTexture(shadowMaps[sprite][shadowIdx], new Rectangle(0, 0, Width, Height, Alignment.Center), tint);
            canvas.PopState();
        }

        base.Render(canvas, transform, tint);
    }
}
