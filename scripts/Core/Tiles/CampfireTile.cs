// scripts/Core/Tiles/CampfireTile.cs
namespace Dungeon2048.Core.Tiles
{
    public sealed class CampfireTile
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public int Charges = 3; // Kann 3x benutzt werden
        public const int MaxCharges = 3;
        public const int WarmthAmount = 3; // -3 Frostbite Stacks pro Zug

        public bool IsExtinguished => Charges <= 0;

        public CampfireTile(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Wird aufgerufen wenn eine Entität durch das Lagerfeuer gewärmt wird
        /// Reduziert Charges um 1
        /// </summary>
        public void ConsumeCharge()
        {
            if (!IsExtinguished)
            {
                Charges--;
            }
        }

        /// <summary>
        /// Prüft ob eine Position adjazent (benachbart) zum Lagerfeuer ist
        /// </summary>
        public bool IsAdjacent(int x, int y)
        {
            int dx = System.Math.Abs(x - X);
            int dy = System.Math.Abs(y - Y);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1) || (dx == 1 && dy == 1);
        }
    }
}
