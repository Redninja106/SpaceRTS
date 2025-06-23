using SpaceGame.Data;

namespace SpaceGameAgain.Tests;

[TestClass]
public static class TestSetup
{
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        Prototypes.Load(true);
    }
}