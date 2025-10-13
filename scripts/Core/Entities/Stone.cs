namespace Dungeon2048.Core.Entities
{
    public sealed class Stone
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X; public int Y;
        public int HitCount = 0;
        public const int MaxHits = 3;
        public bool IsDestroyed => HitCount >= MaxHits;
        public Stone(int x, int y) { X=x; Y=y; }
    }
}
