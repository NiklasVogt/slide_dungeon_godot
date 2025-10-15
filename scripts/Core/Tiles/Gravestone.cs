// scripts/Core/Tiles/Gravestone.cs
namespace Dungeon2048.Core.Tiles
{
    public sealed class Gravestone
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public int HitCount = 0;
        public const int MaxHits = 2; // Weniger als Stone
        public bool IsDestroyed => HitCount >= MaxHits;
        
        public Gravestone(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}