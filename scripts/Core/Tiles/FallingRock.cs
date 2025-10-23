// scripts/Core/Tiles/FallingRock.cs
namespace Dungeon2048.Core.Tiles
{
    public sealed class FallingRock
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public int WarningTurnsRemaining;
        public bool HasFallen = false;
        public const int FallDamage = 8; // Hoher Schaden

        public bool IsWarning => WarningTurnsRemaining > 0 && !HasFallen;
        public bool ShouldFall => WarningTurnsRemaining <= 0 && !HasFallen;

        public FallingRock(int x, int y, int warningTurnsRemaining = 1)
        {
            X = x;
            Y = y;
            WarningTurnsRemaining = warningTurnsRemaining;
        }

        /// <summary>
        /// Wird jeden Zug aufgerufen, zählt Warnung runter
        /// </summary>
        public void AdvanceTurn()
        {
            if (IsWarning)
            {
                WarningTurnsRemaining--;
            }
        }

        /// <summary>
        /// Fels fällt und verursacht Schaden
        /// </summary>
        public void Fall()
        {
            HasFallen = true;
        }
    }
}
