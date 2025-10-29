// scripts/Core/Components/NecrophageComponent.cs
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Managers;
using Godot;

namespace Dungeon2048.Core.Components
{
    /// <summary>
    /// Necrophage Component: Heals when any enemy dies nearby
    /// </summary>
    public sealed class NecrophageComponent : IEnemyComponent
    {
        private const int HealAmount = 3;

        public void Update(Enemy enemy, EntityManager entityManager, TileManager tileManager)
        {
            // Reset heal tracking each turn
            enemy.HealedThisRound = 0;
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

        /// <summary>
        /// Called when any enemy dies to trigger healing
        /// </summary>
        public static void OnAnyEnemyDeath(EntityManager entityManager)
        {
            var necrophages = entityManager.Enemies.Where(e => e.Type == EnemyType.Necrophage).ToList();
            foreach (var necro in necrophages)
            {
                necro.Hp += HealAmount;
                necro.HealedThisRound += HealAmount;
                GD.Print($"Necrophage heilt sich um {HealAmount} HP! (jetzt {necro.Hp} HP)");
            }
        }
    }
}
