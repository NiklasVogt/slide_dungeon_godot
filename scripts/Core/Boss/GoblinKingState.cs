// scripts/Core/Boss/GoblinKingState.cs
using System;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Enemies;
using Dungeon2048.Core.Managers;
using Godot;

namespace Dungeon2048.Core.Boss
{
    /// <summary>
    /// Goblin King boss state
    /// Spawns goblin adds every 3 swipes
    /// </summary>
    public sealed class GoblinKingState : IBossState
    {
        private int _spawnCounter = 0;
        private readonly Random _rng;

        public GoblinKingState(Random rng)
        {
            _rng = rng;
        }

        public void Update(EntityManager entityManager, TileManager tileManager)
        {
            var goblinKing = entityManager.Enemies.FirstOrDefault(e => e.Type == EnemyType.GoblinKing && e.IsBoss);
            if (goblinKing == null) return;

            _spawnCounter++;

            if (_spawnCounter >= 3)
            {
                SpawnGoblinAdds(entityManager);
                _spawnCounter = 0;
            }
        }

        public IBossState? CheckTransition(Enemy boss)
        {
            // Goblin King has no phase transitions
            return null;
        }

        private void SpawnGoblinAdds(EntityManager entityManager)
        {
            // Spawn 2 goblins
            for (int i = 0; i < 2; i++)
            {
                var pos = FindFreePosition(entityManager);
                var goblin = EnemyRegistry.Get(EnemyType.Goblin).Create(pos.X, pos.Y, CalculateEnemyLevel(entityManager));
                entityManager.AddEnemy(goblin);
            }
            GD.Print("Goblin-König ruft Verstärkung!");
        }

        private (int X, int Y) FindFreePosition(EntityManager entityManager)
        {
            // Simple free position finding (could be improved)
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int x = _rng.Next(0, 6);
                int y = _rng.Next(0, 6);

                if (!entityManager.IsOccupiedByEntity(x, y, true))
                {
                    return (x, y);
                }
            }
            return (0, 0); // Fallback
        }

        private int CalculateEnemyLevel(EntityManager entityManager)
        {
            // Simplified enemy level calculation
            return System.Math.Max(1, entityManager.Player.Level / 3);
        }
    }
}
