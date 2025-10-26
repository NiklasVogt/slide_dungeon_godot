// scripts/Core/Tiles/FrostTorch.cs
namespace Dungeon2048.Core.Tiles
{
    public sealed class FrostTorch
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public bool IsExtinguished = false; // Kann von Gegnern gelöscht werden

        public FrostTorch(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
