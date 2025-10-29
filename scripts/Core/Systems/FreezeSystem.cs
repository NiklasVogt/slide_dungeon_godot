// scripts/Core/Systems/FreezeSystem.cs
using Dungeon2048.Core.Managers;

namespace Dungeon2048.Core.Systems
{
    /// <summary>
    /// Manages Freeze status effect mechanics
    /// Frozen enemies cannot move for a duration
    /// </summary>
    public sealed class FreezeSystem
    {
        private bool _enemiesFrozen = false;

        public bool EnemiesFrozen
        {
            get => _enemiesFrozen;
            set => _enemiesFrozen = value;
        }

        /// <summary>
        /// Processes freeze status on all enemies
        /// Decrements frozen turn counters
        /// </summary>
        public void ProcessFreezeStatus(EntityManager entityManager)
        {
            foreach (var enemy in entityManager.Enemies)
            {
                if (enemy.FrozenTurnsRemaining > 0)
                {
                    enemy.FrozenTurnsRemaining--;
                }
            }
        }
    }
}
