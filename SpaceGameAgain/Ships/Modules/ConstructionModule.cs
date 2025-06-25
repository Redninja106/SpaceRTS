using SpaceGame.Extensions;
using SpaceGame.GUI;
using SpaceGame.Rendering;
using SpaceGame.Stations;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Modules;

[Serializable]
internal class ConstructionModule(ConstructionModulePrototype prototype, GameWorld world, ulong id) : Module(prototype, world, id)
{
    public override ConstructionModulePrototype Prototype => (ConstructionModulePrototype)base.Prototype;

    static ConstructionModule()
    {
    }

    public override void Layout(GUIWindow window)
    {
        base.Layout(window);

        window.Text("click to open construction menu", color: Color.Gray);

        //foreach (var group in Prototype.BuildableStructuresByCategory)
        //{
        //    // window.Text(group.Key, size: 20);
        //    // window.Separator();
        //    foreach (var proto in group)
        //    {
        //        window.BeginScope(LayoutMode.Column);
        //        proto.Layout(window);
        //        window.EndScope(LayoutMode.Column);
        //        if (window.LastItemHovered())
        //        {
        //            window.AddCommand(new DrawCommand.Rectangle(window.LastItemBounds, Color.Red, false));
        //        }
        //        window.Separator();

        //        continue;
        //        bool canAfford = World.PlayerTeam.Money >= proto.Cost;
        //        window.Text(proto.Title, size: 16, color: canAfford ? Color.Gray : Color.Red);
        //        if (window.LastItemHovered())
        //        {
        //            World.GUIViewport.SetTooltip(proto.Layout);
        //        }
        //        if (window.LastItemClicked(MouseButton.Left) && canAfford)
        //        {
        //            World.ConstructionInteractionContext.BeginPlacing(proto, Ship);
        //            World.GUIViewport.ClosePopup();
        //        }
        //    }
        //}
    }

    public override void Tick()
    {
    }

    public override void Render(ICanvas canvas)
    {
    }

    public override void RenderSelected(ICanvas canvas)
    {
    }

    public override void Activate()
    {
        base.Activate();
        ConstructionMenu menu = new(this);

        Program.World.GUIViewport.SetPopup(menu.Layout, new(40, 10), Alignment.TopLeft);
    }
}

class ConstructionMenu(ConstructionModule module)
{
    GUIScrollBar scrollBar = new(10);
    IGrouping<ConstructionCategory, UnitPrototype>? currentCategory;

    public void Layout(GUIWindow window)
    {
        scrollBar.Update(window);

        currentCategory ??= module.Prototype.BuildablesByCategory.First();

        using (window.Row())
        {
            using (window.Column())
            {
                foreach (var category in module.Prototype.BuildablesByCategory)
                {
                    window.Image(category.Key.Icon.Texture32x32);
                    if (window.LastItemHovered())
                    {
                        window.Viewport.SetTooltip(w => w.Text(category.Key.Title));
                    }
                    if (window.LastItemClicked(MouseButton.Left))
                    {
                        currentCategory = category;
                        window.HasNewLayout = true;
                    }
                }
            }

            using (window.Column())
            {
                foreach (var prototype in currentCategory)
                {
                    prototype.Layout(window);
                    if (window.LastItemHovered())
                    {
                        window.AddCommand(new DrawCommand.Rectangle(window.LastItemBounds, Color.White with { A = 25 }, true));
                    }
                    if (window.LastItemClicked(MouseButton.Left))
                    {
                        if (prototype is StructurePrototype structurePrototype)
                        {
                            Program.World.ConstructionInteractionContext.BeginPlacing(structurePrototype, module.Ship);
                        }
                    }
                    window.Separator();
                }
            }
        }
    }
}

struct GUIScrollBar
{
    public const float Speed = 25;
    public const float Width = 10;

    private float margin;
    private float scrollAmount;
    private float interpolatedScrollAmount;
    private bool isDragging;
    private float dragOffset;

    public GUIScrollBar(float margin)
    {
        this.margin = margin;
    }

    public void Update(GUIWindow window)
    {
        float regionY = margin + GUIWindow.CornerRadius;
        float regionHeight = window.Viewport.EffectiveHeight - 2 * (margin + GUIWindow.CornerRadius);

        // Y coordinate at which the bottom of the window is at the bottom of the screen
        float maxScroll = float.Max(0, window.GetPredictedSize().Y - regionHeight - regionY);
        float scrollBarSizePercent = regionHeight / (regionHeight + maxScroll);
        float scrollPercent = interpolatedScrollAmount / maxScroll;

        Rectangle bounds = new(
            window.Offset.X + window.GetPredictedSize().X - window.Margin,
            regionY + regionHeight * (1 - scrollBarSizePercent) * scrollPercent,
            Width,
            regionHeight * scrollBarSizePercent,
            Alignment.TopRight
            );

        Color scrollBarColor = Color.White with { A = 50 };

        if (isDragging || window.AreaHovered(bounds))
        {
            scrollBarColor = scrollBarColor with { A = 150 };
            if (Mouse.IsButtonPressed(MouseButton.Left))
            {
                isDragging = true;
                dragOffset = bounds.Y - window.Viewport.MousePosition.Y;
            }
        }

        if (Mouse.IsButtonReleased(MouseButton.Left))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            float newDragY = dragOffset + (window.Viewport.MousePosition.Y - regionY);
            // (window.Viewport.MousePosition.Y / window.Viewport.EffectiveHeight) + scrollDragOffset;
            float newFac = newDragY / (regionHeight * (1 - scrollBarSizePercent));
            scrollAmount = newFac * maxScroll;
        }
        if (window.Hovered)
        {
            scrollAmount -= Mouse.ScrollWheelDelta * Speed;
        }

        if (scrollAmount < 0)
        {
            scrollAmount = 0;
        }
        if (scrollAmount > maxScroll)
        {
            scrollAmount = maxScroll;
        }

        interpolatedScrollAmount = float.Lerp(interpolatedScrollAmount, scrollAmount, 1 - MathF.Pow(0.001f, Time.DeltaTime));
        window.Offset.Y = this.margin + -interpolatedScrollAmount;

        window.AddCommand(new DrawCommand.RoundedRectangle(bounds, Width / 2f, scrollBarColor, true));
    }
}

class ConstructionModulePrototype : ModulePrototype
{
    public override Type ActorType => typeof(ConstructionModule);

    public StructurePrototype[] BuildableStructures { get; set; } = [];
    public StationPrototype[] BuildableStations { get; set; } = [];

    [JsonIgnore]
    public IGrouping<ConstructionCategory, UnitPrototype>[] BuildablesByCategory { get; set; } = [];

    public override void InitializePrototype()
    {
        base.InitializePrototype();
        BuildablesByCategory = BuildableStructures
            .Cast<UnitPrototype>()
            .Concat(BuildableStations)
            .OrderBy(s => s.Title)
            .GroupBy(s => s.Category ?? Prototypes.Get<ConstructionCategory>("default_category"))
            .ToArray();
    }
}