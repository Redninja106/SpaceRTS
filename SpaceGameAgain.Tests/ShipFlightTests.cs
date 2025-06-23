using SimulationFramework;
using SpaceGame;
using SpaceGame.Data;
using SpaceGame.Orders;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameAgain.Tests;

[TestClass]
public class ShipFlightTests
{
    public ShipFlightTests()
    {
    }

    // test on distances of various orders of magnitudes
    [DataRow(.1)]
    [DataRow(.5)]
    [DataRow(1.0)]
    [DataRow(10.0)]
    [DataRow(100.0)]
    [DataRow(1000.0)]
    [DataRow(10000.0)]
    [TestMethod]
    public void CalculateTravelTime_SingleDimension_Accurate(double distance)
    {
        DoubleVector targetPosition = new(distance, 0);

        TestWorld world = new();
        var ship = world.AddShip(new ShipPrototype()
        {
            FlySpeed = 10,
        });
        ship.EnqueueOrder(new MoveOrder()
        {
            TargetPosition = targetPosition,
            ForecastedTargetPosition = targetPosition,
        });

        double time = ShipNavigator.CalculateTravelTime(ship.Prototype, ship.Transform.Position, targetPosition);

        world.TickWhile(() => DoubleVector.Distance(ship.Transform.Position, targetPosition) >= 0.001, 5000);

        Assert.AreEqual((int)(time * Program.TickRate), (int)world.tick, 1);
    }

    [DataRow(0)]
    [DataRow(.25f)]
    [DataRow(.5f)]
    [DataRow(.75f)]
    [DataRow(1)]
    [TestMethod]
    public void CalculateTurnTime_Accurate(float angle)
    {
        float radians = angle * float.Tau;

        TestWorld world = new();

        Ship ship = world.AddShip(new ShipPrototype() { TurnSpeed = 1, FlySpeed = 0 });
        DoubleVector targetPosition = DoubleVector.FromVector2(Angle.ToVector(radians));
        ship.EnqueueOrder(new MoveOrder() { ForecastedTargetPosition = targetPosition, TargetPosition = targetPosition, });

        double time = ShipNavigator.TurnTime(ship.Prototype, 0, radians);

        world.TickWhile(() => Angle.Distance(ship.Transform.Rotation, radians) >= 0.0001);

        Assert.AreEqual((int)(time * Program.TickRate), (int)world.tick, 1);
    }
}
