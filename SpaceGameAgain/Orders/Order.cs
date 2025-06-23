using ImGuiNET;
using SpaceGame.Commands;
using SpaceGame.Debugging;
using SpaceGame.Extensions;
using SpaceGame.Ships;
using SpaceGame.Stations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;

[Serializable(Abstract = true)]
internal abstract class Order
{
    [field: Serialize]
    public Ship Ship { get; set; }

    public bool IsCompleted { get; private set; } = false;

    public abstract void Tick();

    public virtual void RenderOverlay(ICanvas canvas, ref Transform startTransform, ref Transform forecastedStartTransform)
    {
    }

    public virtual void OnEnqueued(Ship ship)
    {
        this.Ship = ship;
    }

    public void Complete()
    {
        IsCompleted = true;
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ID);
    //    writer.Write(Unit);
    //}

}
