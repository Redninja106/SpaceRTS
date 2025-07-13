using SimulationFramework;
using SimulationFramework.Drawing;
using System.Runtime.CompilerServices;

//args = [
//"C:\\Users\\Ryan\\source\\repos\\SpaceGameAgain\\SpaceGameAgain\\Assets\\Sprites\\default\\tmp\\test.png",
//..Enumerable.Range(0, 32).Select(i => $"C:\\Users\\Ryan\\source\\repos\\SpaceGameAgain\\SpaceGameAgain\\Assets\\Sprites\\default\\tmp\\shadow_{i}.png")
//];

new SimulationHost().Start(Simulation.Create(() => Run(args), canvas => { Application.Exit(false); }));

void Run(string[] args)
{
    string inputDirectory = args[0];
    string outputFile = args[1];
    int shadowCount = int.Parse(args[2]);


    Console.WriteLine($"packing {shadowCount} textures from {inputDirectory} into {outputFile} ");
    List<ITexture> textures = [];
    for (int i = 0; i < shadowCount; i++)
    {
        textures.Add(Graphics.LoadTexture(Path.Combine(inputDirectory, $"shadow_{i}.png")));
    }

    ITexture result = Graphics.CreateTexture(textures[0].Width, textures[1].Height);
    result.Pixels.Clear();

    for (int i = 0; i < textures.Count; i++)
    {
        int mask = 1 << i;

        for (int y = 0; y < result.Height; y++)
        {
            for (int x = 0; x < result.Width; x++)
            {
                if (textures[i][x, y].R > 127)
                {
                    ref int pixel = ref Unsafe.As<Color, int>(ref result[x, y]);
                    pixel |= mask;
                    result[x, y] = Unsafe.As<int, Color>(ref pixel);
                }
            }
        }
    }

    result.ApplyChanges();
    result.Encode(args[1]);
    Console.WriteLine("done!");
    Environment.Exit(0);
}
