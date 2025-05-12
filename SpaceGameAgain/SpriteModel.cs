using ImGuiNET;
using SimulationFramework.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class SpriteModel : ModelPrototype, IInspectable
{

    private ITexture[] sprites;
    public string SpritesFolder { get; set; }
    public int SpriteCount { get; set; }

    public override void InitializePrototype()
    {
        sprites = new ITexture[SpriteCount];
        for (int i = 0; i < SpriteCount; i++)
        {
            sprites[i] = Graphics.LoadTexture($"./Assets/Sprites/{SpritesFolder}/{i}.png");
            sprites[i].Filter = TextureFilter.MipmapPoint;
            Graphics.GenerateMipmaps(sprites[i]);
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
        canvas.DrawTexture(sprites[sprite], new Rectangle(0, 0, Width, Height, Alignment.Center), tint);
    }

    public void DebugLayout()
    {
        ImGui.Text(this.SpritesFolder);
        int sprite = (int)(Time.TotalTime * .5f * SpriteCount) % SpriteCount;
        ImGui.Image(this.sprites[sprite].GetImGuiID(), new(this.sprites[sprite].Width, this.sprites[sprite].Height), new(0, 0), new(1, 1), new(1,1,1,1), new(1,1,1,1));

        for (int i = 0; i < sprites.Length; i++)
        {
            ImGui.TextDisabled(i.ToString());
            if (ImGui.BeginItemTooltip())
            {
                ImGui.Image(this.sprites[i].GetImGuiID(), new(this.sprites[i].Width, this.sprites[i].Height));
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            if (ImGui.GetContentRegionAvail().X < 5)
                ImGui.NewLine();
        }
    }
}
