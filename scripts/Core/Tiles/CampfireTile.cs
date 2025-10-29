// scripts/Core/Tiles/CampfireTile.cs
namespace Dungeon2048.Core.Tiles
{
    public sealed class CampfireTile
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public int Charges = 3; // Kann 3x wärmen
        public const int MaxCharges = 3;
        public const int WarmthAmount = 3; // Entfernt 3 Cold Stacks

        public bool IsExtinguished => Charges <= 0;

        public CampfireTile(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Verwendet eine Charge und gibt zurück ob die Wärme gewährt werden soll
        /// </summary>
        public bool UseCharge()
        {
            if (IsExtinguished) return false;

            Charges--;
            return true;
        }
    }
}
