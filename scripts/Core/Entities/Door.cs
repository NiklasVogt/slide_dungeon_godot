namespace Dungeon2048.Core.Entities
{
    public sealed class Door
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X; public int Y;
        public bool IsActive = false;
        public Door(int x, int y) { X=x; Y=y; }
    }
}
