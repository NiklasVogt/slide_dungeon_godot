// scripts/Core/Boss/IBossState.cs
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Managers;

namespace Dungeon2048.Core.Boss
{
    /// <summary>
    /// Interface for boss state pattern
    /// Each boss phase implements this interface
    /// </summary>
    public interface IBossState
    {
        /// <summary>
        /// Updates the boss state each turn
        /// </summary>
        void Update(EntityManager entityManager, TileManager tileManager);

        /// <summary>
        /// Checks if the boss should transition to another phase
        /// Returns the new state if transition should occur, null otherwise
        /// </summary>
        IBossState? CheckTransition(Enemy boss);
    }
}
