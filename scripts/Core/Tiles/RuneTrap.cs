namespace Dungeon2048.Core.Tiles
{
    public sealed class RuneTrap
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public bool IsRevealed = false;
        public bool IsTriggered = false;
        public const int Damage = 5;
        
        public RuneTrap(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}