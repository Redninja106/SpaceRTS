//using SpaceGame.Combat;
//using SpaceGame.GUI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SpaceGame.Structures;
//internal class MissileTurretBehavior : StructureBehavior
//{
//    private MissileSystem system;

//    public MissileTurretBehavior(Structure instance) : base(instance)
//    {
//        system = new(new(), World.NewID(), Transform.Default, instance);
//        SelectGUI = [new DynamicLabel(() => $"rounds: {system.MissilesRemaining}/{system.SalvoSize}", Element.TextSize)];
//    }

//    public override Element[] SelectGUI { get; } = [];

//    public override void Update()
//    {
//        system.Update();
//    }
//}

//internal class ChaingunTurretBehavior : StructureBehavior
//{
//    private ChaingunSystem system;

//    public ChaingunTurretBehavior(Structure instance) : base(instance)
//    {
//        system = new(instance);
//        SelectGUI = [new DynamicLabel(() => $"rounds: {system.ammo}/{system.ammoCapacity}", Element.TextSize)];
//    }

//    public override Element[] SelectGUI { get; }

//    public override void Update()
//    {
//        system.Update();
//    }
//}

