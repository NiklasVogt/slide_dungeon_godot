namespace Dungeon2048.Core.Tiles
{
    public sealed class MagicBarrier
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public int HitCount = 0;
        public int SwipesSinceBroken = 0;
        public const int MaxHits = 3;
        public const int RegenerationTime = 5;
        
        public bool IsDestroyed => HitCount >= MaxHits;
        public bool ShouldRegenerate => IsDestroyed && SwipesSinceBroken >= RegenerationTime;
        
        public MagicBarrier(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public void Reset()
        {
            HitCount = 0;
            SwipesSinceBroken = 0;
        }
    }
}