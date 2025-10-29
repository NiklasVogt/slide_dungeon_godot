// scripts/Core/Components/IEnemyComponent.cs
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Managers;

namespace Dungeon2048.Core.Components
{
    /// <summary>
    /// Base interface for enemy components
    /// Components add specific behaviors to enemies without cluttering the Enemy class
    /// Follows Component Pattern for flexible enemy design
    /// </summary>
    public interface IEnemyComponent
    {
        /// <summary>
        /// Called each turn to update component state
        /// </summary>
        void Update(Enemy enemy, EntityManager entityManager, TileManager tileManager);

        /// <summary>
        /// Called when the enemy takes damage
        /// </summary>
        void OnTakeDamage(Enemy enemy, int damage);

        /// <summary>
        /// Called when the enemy deals damage
        /// </summary>
        void OnDealDamage(Enemy enemy, EntityBase target, int damage);

        /// <summary>
        /// Called when the enemy moves
        /// </summary>
        void OnMove(Enemy enemy, int oldX, int oldY, int newX, int newY);

        /// <summary>
        /// Called when the enemy dies
        /// </summary>
        void OnDeath(Enemy enemy, EntityManager entityManager, TileManager tileManager);
    }
}
