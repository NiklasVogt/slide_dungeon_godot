// scripts/Core/Boss/FireGiantState.cs
using System;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Enemies;
using Dungeon2048.Core.Managers;
using Godot;

namespace Dungeon2048.Core.Boss
{
    /// <summary>
    /// Fire Giant boss state - Phase 1
    /// Hammer slam attack every 4 swipes (creates fire tiles in diagonal cross pattern)
    /// Transitions to Phase 2 at 50% HP
    /// </summary>
    public sealed class FireGiantPhase1State : IBossState
    {
        private readonly Random _rng;
        private int _turnCounter = 0;

        public FireGiantPhase1State(Random rng)
        {
            _rng = rng;
        }

        public void Update(EntityManager entityManager, TileManager tileManager)
        {
            var fireGiant = entityManager.Enemies.FirstOrDefault(e => e.Type == EnemyType.FireGiant && e.IsBoss);
            if (fireGiant == null) return;

            _turnCounter++;

            // Every 4 turns: Hammer Slam (diagonal cross pattern becomes fire)
            if (_turnCounter % 4 == 0)
            {
                HammerSlam(fireGiant, tileManager);
            }
        }

        public IBossState? CheckTransition(Enemy boss)
        {
            // Transition to Phase 2 at 50% HP
            if (!boss.IsPhase2 && boss.Hp <= boss.MaxHp / 2)
            {
                return new FireGiantPhase2State(_rng);
            }
            return null;
        }

        private void HammerSlam(Enemy fireGiant, TileManager tileManager)
        {
            // Diagonal cross pattern: 4 diagonal directions
            var diagonals = new[] {
                (1, 1),   // Down-Right
                (1, -1),  // Up-Right
                (-1, 1),  // Down-Left
                (-1, -1)  // Up-Left
            };

            GD.Print("🔥🔨 FEUERGIGANT schwingt seinen Hammer! 🔨🔥");

            foreach (var (dx, dy) in diagonals)
            {
                for (int dist = 1; dist <= 2; dist++) // 2 tiles far
                {
                    int targetX = fireGiant.X + (dx * dist);
                    int targetY = fireGiant.Y + (dy * dist);

                    // Bounds check
                    if (targetX < 0 || targetX >= 6 || targetY < 0 || targetY >= 6)
                        continue;

                    // Spawn Fire Tile
                    tileManager.AddFireTile(targetX, targetY);
                    GD.Print($"🔥 Hammer-Schlag erzeugt Lava bei ({targetX},{targetY})");
                }
            }
        }
    }

    /// <summary>
    /// Fire Giant boss state - Phase 2
    /// Spawns Fire Elementals at phase start
    /// Continues hammer slam attacks
    /// </summary>
    public sealed class FireGiantPhase2State : IBossState
    {
        private readonly Random _rng;
        private bool _elementalsSpawned = false;
        private int _turnCounter = 0;

        public FireGiantPhase2State(Random rng)
        {
            _rng = rng;
        }

        public void Update(EntityManager entityManager, TileManager tileManager)
        {
            var fireGiant = entityManager.Enemies.FirstOrDefault(e => e.Type == EnemyType.FireGiant && e.IsBoss);
            if (fireGiant == null) return;

            // Spawn elementals once
            if (!_elementalsSpawned)
            {
                fireGiant.IsPhase2 = true;
                SpawnFireElementals(entityManager);
                _elementalsSpawned = true;
            }

            _turnCounter++;

            // Continue hammer slam
            if (_turnCounter % 4 == 0)
            {
                HammerSlam(fireGiant, tileManager);
            }
        }

        public IBossState? CheckTransition(Enemy boss)
        {
            // No further transitions
            return null;
        }

        private void SpawnFireElementals(EntityManager entityManager)
        {
            GD.Print("🔥🔥 FEUERGIGANT PHASE 2! Er beschwört Feuer-Elementare! 🔥🔥");

            // Spawn 3 Fire Elementals
            for (int i = 0; i < 3; i++)
            {
                var pos = FindFreePosition(entityManager);
                var elemental = EnemyRegistry.Get(EnemyType.FireElemental).Create(pos.X, pos.Y, CalculateEnemyLevel(entityManager) + 2);
                entityManager.AddEnemy(elemental);
            }
        }

        private void HammerSlam(Enemy fireGiant, TileManager tileManager)
        {
            var diagonals = new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) };

            GD.Print("🔥🔨 FEUERGIGANT schwingt seinen Hammer! 🔨🔥");

            foreach (var (dx, dy) in diagonals)
            {
                for (int dist = 1; dist <= 2; dist++)
                {
                    int targetX = fireGiant.X + (dx * dist);
                    int targetY = fireGiant.Y + (dy * dist);

                    if (targetX < 0 || targetX >= 6 || targetY < 0 || targetY >= 6)
                        continue;

                    tileManager.AddFireTile(targetX, targetY);
                    GD.Print($"🔥 Hammer-Schlag erzeugt Lava bei ({targetX},{targetY})");
                }
            }
        }

        private (int X, int Y) FindFreePosition(EntityManager entityManager)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int x = _rng.Next(0, 6);
                int y = _rng.Next(0, 6);

                if (!entityManager.IsOccupiedByEntity(x, y, false))
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
