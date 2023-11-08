using System;

namespace Planetoid3D
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new PlanetoidGame())
                game.Run();
        }
    }
}
