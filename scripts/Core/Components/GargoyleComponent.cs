// scripts/Core/Components/GargoyleComponent.cs
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Managers;

namespace Dungeon2048.Core.Components
{
    /// <summary>
    /// Gargoyle Component: Statue mechanic - only moves every other turn
    /// </summary>
    public sealed class GargoyleComponent : IEnemyComponent
    {
        public void Update(Enemy enemy, EntityManager entityManager, TileManager tileManager)
        {
            // Toggle movement state each turn
            enemy.HasMoved = !enemy.HasMoved;
        }

        public void OnTakeDamage(Enemy enemy, int damage)
        {
            // No special behavior
        }

        public void OnDealDamage(Enemy enemy, EntityBase target, int damage)
        {
            // No special behavior
        }

        public void OnMove(Enemy enemy, int oldX, int oldY, int newX, int newY)
        {
            // No special behavior
        }

        public void OnDeath(Enemy enemy, EntityManager entityManager, TileManager tileManager)
        {
            // No special behavior
        }
    }
}
