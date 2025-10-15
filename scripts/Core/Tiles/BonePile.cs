// scripts/Core/Tiles/BonePile.cs
namespace Dungeon2048.Core.Tiles
{
    public sealed class BonePile
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public int HitCount = 0;
        public const int MaxHits = 2;
        public bool IsDestroyed => HitCount >= MaxHits;
        
        public BonePile(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}