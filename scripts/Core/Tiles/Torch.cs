// scripts/Core/Tiles/Torch.cs
namespace Dungeon2048.Core.Tiles
{
    public sealed class Torch
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public bool IsLit = true;
        
        public Torch(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}