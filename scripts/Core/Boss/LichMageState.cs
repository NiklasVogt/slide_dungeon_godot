// scripts/Core/Boss/LichMageState.cs
using System;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Enemies;
using Dungeon2048.Core.Managers;
using Godot;

namespace Dungeon2048.Core.Boss
{
    /// <summary>
    /// Lich Mage boss state - Phase 1
    /// Teleports every few turns
    /// Transitions to Phase 2 at 50% HP
    /// </summary>
    public sealed class LichMagePhase1State : IBossState
    {
        private readonly Random _rng;

        public LichMagePhase1State(Random rng)
        {
            _rng = rng;
        }

        public void Update(EntityManager entityManager, TileManager tileManager)
        {
            var lich = entityManager.Enemies.FirstOrDefault(e => e.Type == EnemyType.LichMage && e.IsBoss);
            if (lich == null) return;

            lich.LichTeleportCounter++;

            if (lich.ShouldLichTeleport())
            {
                var pos = FindFreePosition(entityManager, true);
                lich.X = pos.X;
                lich.Y = pos.Y;
                lich.LichTeleportCounter = 0;
                GD.Print("🌀 Der Lich-Magier teleportiert sich!");
            }
        }

        public IBossState? CheckTransition(Enemy boss)
        {
            // Transition to Phase 2 at 50% HP
            if (!boss.IsPhase2 && boss.Hp <= boss.MaxHp / 2)
            {
                return new LichMagePhase2State(_rng);
            }
            return null;
        }

        private (int X, int Y) FindFreePosition(EntityManager entityManager, bool ignorePlayer)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int x = _rng.Next(0, 6);
                int y = _rng.Next(0, 6);

                if (!entityManager.IsOccupiedByEntity(x, y, ignorePlayer))
                {
                    return (x, y);
                }
            }
            return (0, 0);
        }
    }

    /// <summary>
    /// Lich Mage boss state - Phase 2
    /// Spawns kultist adds at phase start
    /// Continues teleporting
    /// </summary>
    public sealed class LichMagePhase2State : IBossState
    {
        private readonly Random _rng;
        private bool _kulitstsSpawned = false;

        public LichMagePhase2State(Random rng)
        {
            _rng = rng;
        }

        public void Update(EntityManager entityManager, TileManager tileManager)
        {
            var lich = entityManager.Enemies.FirstOrDefault(e => e.Type == EnemyType.LichMage && e.IsBoss);
            if (lich == null) return;

            // Spawn kultists once
            if (!_kulitstsSpawned)
            {
                lich.IsPhase2 = true;
                SpawnLichKultists(entityManager);
                _kulitstsSpawned = true;
            }

            // Continue teleporting
            lich.LichTeleportCounter++;

            if (lich.ShouldLichTeleport())
            {
                var pos = FindFreePosition(entityManager, true);
                lich.X = pos.X;
                lich.Y = pos.Y;
                lich.LichTeleportCounter = 0;
                GD.Print("🌀 Der Lich-Magier teleportiert sich!");
            }
        }

        public IBossState? CheckTransition(Enemy boss)
        {
            // No further transitions
            return null;
        }

        private void SpawnLichKultists(EntityManager entityManager)
        {
            GD.Print("⚡ Der Lich-Magier beschwört Kultisten!");

            for (int i = 0; i < 4; i++)
            {
                var pos = FindFreePosition(entityManager, false);
                var kultist = EnemyRegistry.Get(EnemyType.Kultist).Create(pos.X, pos.Y, CalculateEnemyLevel(entityManager));
                entityManager.AddEnemy(kultist);
            }
        }

        private (int X, int Y) FindFreePosition(EntityManager entityManager, bool ignorePlayer)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int x = _rng.Next(0, 6);
                int y = _rng.Next(0, 6);

                if (!entityManager.IsOccupiedByEntity(x, y, ignorePlayer))
                {
                    return (x, y);
                }
            }
            return (0, 0);
        }

        private int CalculateEnemyLevel(EntityManager entityManager)
        {
            return System.Math.Max(1, entityManager.Player.Level / 3);
        }
    }
}
