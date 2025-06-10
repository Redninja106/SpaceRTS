using ImGuiNET;
using Silk.NET.OpenGL;
using SimulationFramework.Desktop;
using SpaceGame.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class SpriteModel : ModelPrototype, IInspectable
{

    protected ITexture[] sprites;
    public string SpritesFolder { get; set; }
    public int SpriteCount { get; set; }

    public override void InitializePrototype()
    {
        sprites = new ITexture[SpriteCount];
        Load();

        base.InitializePrototype();
    }

    public override Actor Deserialize(BinaryReader reader)
    {
        throw new NotSupportedException();
    }
    
    public virtual void Render(ICanvas canvas, Transform transform, ColorF tint)
    {
        int sprite = (int)MathF.Round((Angle.Normalize(transform.Rotation) / MathF.Tau) * SpriteCount) % SpriteCount;
        canvas.DrawTexture(sprites[sprite], new Rectangle(0, 0, Width, Height, Alignment.Center), tint);
    }

    public virtual void DebugLayout()
    {
        ImGui.Text(this.SpritesFolder);
        if (ImGui.Button("Reload"))
        {
            Load();
        }
        LayoutSpriteArray(this.sprites, "Sprites");
    }

    public virtual void Load()
    {
        for (int i = 0; i < SpriteCount; i++)
        {
            sprites[i]?.Dispose();
            sprites[i] = Graphics.LoadTexture($"./Assets/Sprites/{SpritesFolder}/{i}.png");
            sprites[i].Filter = TextureFilter.Point;
            Graphics.GenerateMipmaps(sprites[i]);
        }
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

    public override void Load()
    {
        for (int i = 0; i < SpriteCount; i++)
        {
            spriteNormalMaps[i]?.Dispose();
            spriteNormalMaps[i] = Graphics.LoadTexture($"./Assets/Sprites/{SpritesFolder}/{i}_normal.png");
            spriteNormalMaps[i].Filter = TextureFilter.Point;
            Graphics.GenerateMipmaps(spriteNormalMaps[i]);
        }

        base.Load();
    }

    public override void Render(ICanvas canvas, Transform transform, ColorF tint)
    {
        int sprite = (int)MathF.Round((Angle.Normalize(transform.Rotation) / MathF.Tau) * SpriteCount) % SpriteCount;

        shader.texture = sprites[sprite];
        shader.normalMap = spriteNormalMaps[sprite];
        shader.tint = tint;

        Vector2 lightDir2 = transform.Position.ToVector2().Normalized();
        shader.lightDirection = new Vector3(0, -1, 0).Normalized();
        Vector2 v = (transform.Position).ToVector2().Normalized();
        shader.lightDirection = new Vector3(v.X, -v.Y, -1).Normalized();
        shader.size = new(Width, Height);
        canvas.Fill(shader);
        canvas.DrawRect(new Rectangle(0, 0, Width, Height, Alignment.Center));

        //canvas.DrawTexture(sprites[sprite], new Rectangle(0, 0, Width, Height, Alignment.Center), tint);
    }

    public override void DebugLayout()
    {
        base.DebugLayout();
        LayoutSpriteArray(this.spriteNormalMaps, "Normals");
    }
}
