namespace Dungeon2048.Core.Tiles
{
    public sealed class BonePile
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public int HitCount = 0;
        public int SwipesAlive = 0; // NEU: Zählt wie lange der Haufen existiert
        public const int MaxHits = 2;
        public const int MaxSwipesAlive = 3; // NEU: Nach 3 Swipes wird wieder Skelett
        public bool IsDestroyed => HitCount >= MaxHits;
        public bool ShouldRevive => SwipesAlive >= MaxSwipesAlive; // NEU
        
        public BonePile(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}