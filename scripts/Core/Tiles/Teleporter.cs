namespace Dungeon2048.Core.Tiles
{
    public sealed class Teleporter
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public string LinkedTeleporterId = null; // ID des Partner-Teleporters
        public bool IsActive = true;
        
        public Teleporter(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}