// scripts/Core/Tiles/FireTile.cs
namespace Dungeon2048.Core.Tiles
{
    public sealed class FireTile
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public int EntitiesPassedThrough = 0;
        public const int MaxPasses = 2; // Löscht sich nach 2 Durchgängen
        public const int BurningDamage = 3; // Schaden beim Durchgehen

        public bool IsExtinguished => EntitiesPassedThrough >= MaxPasses;

        public FireTile(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Wird aufgerufen wenn eine Entität durch das Feuer geht
        /// Gibt zurück ob die Entität Schaden nehmen sollte
        /// </summary>
        public bool OnEntityPass()
        {
            if (IsExtinguished) return false;

            EntitiesPassedThrough++;
            return true; // Entität nimmt Schaden + Burning
        }
    }
}
