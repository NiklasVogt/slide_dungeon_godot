// scripts/Core/Components/MimicComponent.cs
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Managers;
using Godot;

namespace Dungeon2048.Core.Components
{
    /// <summary>
    /// Mimic Component: Enemy disguises as spell drop, reveals after 3 hits
    /// </summary>
    public sealed class MimicComponent : IEnemyComponent
    {
        private int _hitCount = 0;
        private const int HitsToReveal = 3;

        public bool IsDisguised { get; private set; } = true;

        public void Update(Enemy enemy, EntityManager entityManager, TileManager tileManager)
        {
            // No per-turn update needed
        }

        public void OnTakeDamage(Enemy enemy, int damage)
        {
            if (IsDisguised)
            {
                _hitCount++;
                if (_hitCount >= HitsToReveal)
                {
                    IsDisguised = false;
                    enemy.IsDisguised = false; // Update enemy property
                    GD.Print($"🎭 Mimic entlarvt! Es war ein Feind!");
                }
            }
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
