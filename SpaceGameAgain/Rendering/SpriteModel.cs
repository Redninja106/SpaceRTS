using ImGuiNET;
using Silk.NET.OpenGL;
using SimulationFramework.Desktop;
using SimulationFramework.Drawing.Shaders;
using SimulationFramework.Drawing.Shaders.Compiler;
using SpaceGame.Planets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Rendering;
internal class SpriteModel : ModelPrototype, IInspectable
{
    private ModelOrientation[] orientations;
    
    public int SpriteCount { get; set; }
    public int ShadowCount { get; set; }
    public bool HasNormalMaps { get; set; }
    public bool HasShadowMaps { get; set; }

    private NormalMappedShader shader = new();

    public override void InitializePrototype()
    {
        base.InitializePrototype();
    }

    public virtual void Render(ICanvas canvas, Transform transform, ColorF tint)
    {
        int index = IndexFromRotation(transform.Rotation, SpriteCount);
        ModelOrientation orientation = orientations[index];

        if (Program.World.GetStar(transform.Position) is Star star)
        {
            Vector2 dir = (transform.Position - star.InterpolatedTransform.Position).ToVector2().Normalized();
            dir.Y = -dir.Y;
            shader.lightDirection = Vector3.Normalize(new(dir, -.5f));
            float angle = float.Atan2(dir.Y, dir.X);
            shader.shadowIndex = Angle.Normalize(angle) / float.Tau * ShadowCount;

            if (tint == ColorF.White)
            {
                tint = ColorF.Lerp(tint, star.Prototype.Color, .25f);
            }
        }

        shader.size = new(Width, Height);
        shader.normalMap = orientation.normalMap;
        shader.texture = orientation.diffuseMap;
        shader.shadowMap = orientation.shadowMap;
        shader.tint = tint;
        canvas.Fill(shader);
        canvas.DrawRect(0, 0, Width, Height, Alignment.Center);
    }

    public void DebugLayout()
    {
        // ImGui.Text(SpritesFolder);
        if (ImGui.Button("Reload"))
        {
            LoadAssets(Prototypes.GetPrototypeDirectory(this.Name!));
        }

        for (int i = 0; i < SpriteCount; i++)
        {
            orientations[i].Layout();
        }
    }

    public override void LoadAssets(string prototypeDirectory)
    {
        orientations = new ModelOrientation[SpriteCount];

        for (int i = 0; i < orientations.Length; i++)
        {
            LoadSprite(prototypeDirectory, $"{i}.png", ref orientations[i].diffuseMap);
            if (HasNormalMaps)
            {
                LoadSprite(prototypeDirectory, $"{i}_normal.png", ref orientations[i].normalMap);
            }
            if (HasNormalMaps)
            {
                LoadSprite(prototypeDirectory, $"{i}_shadow.png", ref orientations[i].shadowMap);
            }
        }
    }

    private static void LoadSprite(string directory, string name, [NotNull] ref ITexture? texture)
    {
        texture?.Dispose();
        texture = Graphics.LoadTexture(Path.Combine(directory, name), TextureOptions.Constant);
        texture.Filter = TextureFilter.Point;
        Graphics.GenerateMipmaps(texture);
    }

    protected void LayoutSpriteArray(ITexture[] spriteArray, string name)
    {
        ImGui.SeparatorText(name);
        ImGui.PushID(name);

        int sprite = (int)(Time.TotalTime * .5f * SpriteCount) % SpriteCount;
        ImGui.Image(spriteArray[sprite].GetImGuiID(), new(spriteArray[sprite].Width, spriteArray[sprite].Height), new(0, 0), new(1, 1), new(1, 1, 1, 1), new(1, 1, 1, 1));

        for (int i = 0; i < spriteArray.Length; i++)
        {
            ImGui.TextDisabled(i.ToString());
            if (ImGui.BeginItemTooltip())
            {
                ImGui.Image(spriteArray[i].GetImGuiID(), new(spriteArray[i].Width, spriteArray[i].Height));
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            if (ImGui.GetContentRegionAvail().X < 5)
                ImGui.NewLine();
        }
        ImGui.NewLine();
        ImGui.PopID();
    }

    struct ModelOrientation
    {
        public ITexture diffuseMap;
        public ITexture? normalMap;
        public ITexture? shadowMap;

        public void Layout()
        {
            ImGui.Image(diffuseMap.GetImGuiID(), new(200, 200));
            if (normalMap != null)
            {
                ImGui.SameLine();
                ImGui.Image(normalMap.GetImGuiID(), new(200, 200));
            }
            if (shadowMap != null)
            {
                ImGui.SameLine();
                ImGui.Image(shadowMap.GetImGuiID(), new(200, 200));
            }
        }
    }

    public static int IndexFromRotation(float rotation, int count)
    {
        return (int)MathF.Round(Angle.Normalize(rotation) / MathF.Tau * count) % count;
    }
}

class NormalMappedSpriteModel : SpriteModel
{
    NormalMappedShader shader = new();
    protected ITexture[] spriteNormalMaps;

    public override void InitializePrototype()
    {
        spriteNormalMaps = new ITexture[SpriteCount];
        base.InitializePrototype();
    }

    public override void LoadAssets(string prototypePath)
    {
        for (int i = 0; i < SpriteCount; i++)
        {
            spriteNormalMaps[i]?.Dispose();
            spriteNormalMaps[i] = Graphics.LoadTexture(Path.Combine(prototypePath, $"{i}_normal.png"), TextureOptions.Constant);
            spriteNormalMaps[i].Filter = TextureFilter.Point;
            Graphics.GenerateMipmaps(spriteNormalMaps[i]);
        }
    }

    public override void Render(ICanvas canvas, Transform transform, ColorF tint)
    {
        int sprite = (int)MathF.Round(Angle.Normalize(transform.Rotation) / MathF.Tau * SpriteCount) % SpriteCount;

        // shader.texture = diffuse[sprite];
        shader.normalMap = spriteNormalMaps[sprite];
        shader.tint = tint;

        Star? star = Program.World.GetStar(transform.Position)!;
        if (star != null)
        {
            shader.tint *= ColorF.Lerp(ColorF.White, star.Prototype.Color, .2f);
            Vector2 v = (transform.Position - star.Transform.Position).ToVector2().Normalized();
            shader.lightDirection = new Vector3(v.X, -v.Y, -.2f).Normalized();
        }

        shader.size = new(Width, Height);
        canvas.Fill(shader);
        canvas.DrawRect(new Rectangle(0, 0, Width, Height, Alignment.Center));

        //canvas.DrawTexture(sprites[sprite], new Rectangle(0, 0, Width, Height, Alignment.Center), tint);
    }

    // public override void DebugLayout()
    // {
    //     base.DebugLayout();
    //     LayoutSpriteArray(spriteNormalMaps, "Normals");
    // }
}

//class NormalMappedShadowedSpriteModel : NormalMappedSpriteModel
//{
//    private ITexture[,] shadowTextures;
//    private int shadowCount = 8;

//    public override void InitializePrototype()
//    {
//        shadowTextures = new ITexture[SpriteCount, shadowCount];
//        base.InitializePrototype();
//    }

//    public override void LoadAssets(string prototypePath)
//    {
//        for (int i = 0; i < this.SpriteCount; i++)
//        {
//            for (int j = 0; j < shadowCount; j++)
//            {
//                shadowTextures[i, j] = Graphics.LoadTexture($"{prototypePath}/{i}_shadow_{j}.png" , TextureOptions.Constant | TextureOptions.NonRenderTarget);
//            }
//        }

//        base.LoadAssets(prototypePath);
//    }

//    public override void Render(ICanvas canvas, Transform transform, ColorF tint)
//    {
//        Star? star = Program.World.GetStar(transform.Position)!;
//        if (star != null)
//        {
//            if (tint == ColorF.White) 
//            {
//                tint = ColorF.Lerp(ColorF.White, star.Prototype.Color, .2f);
//            }
//            Vector2 v = (transform.Position - star.InterpolatedTransform.Position).ToVector2().Normalized();

//            int sprite = (int)MathF.Round(Angle.Normalize(transform.Rotation) / MathF.Tau * SpriteCount) % SpriteCount;
//            int shadow = (int)MathF.Round(Angle.Normalize(Angle.FromVector(new(v.X, -v.Y))) / MathF.Tau * shadowCount) % shadowCount;
//            ITexture texture = shadowTextures[sprite, shadow];
//            canvas.DrawTexture(texture, new Rectangle(0, 0, this.Width, this.Height, Alignment.Center), tint);
//        }

//        base.Render(canvas, transform, tint);
//    }

//    public override void DebugLayout()
//    {
//        for (int x = 0; x < SpriteCount; x++)
//        {
//            for (int y = 0; y < shadowCount; y++)
//            {
//                ImGui.Image(shadowTextures[x, y].GetImGuiID(), new(50, 50));
//                ImGui.SameLine();
//            }
//        }

//        base.DebugLayout();
//    }

//}