// scripts/Core/Boss/IceDragonState.cs
using System;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Enemies;
using Dungeon2048.Core.Managers;
using Godot;

namespace Dungeon2048.Core.Boss
{
    /// <summary>
    /// Ice Dragon boss state - Phase 1
    /// - Ewiger Winter: +3 cold stacks to all entities every 4 turns
    /// - Spawns Frostbite Wraith every 5 turns
    /// Transitions to Phase 2 at 50% HP
    /// </summary>
    public sealed class IceDragonPhase1State : IBossState
    {
        private readonly Random _rng;

        public IceDragonPhase1State(Random rng)
        {
            _rng = rng;
        }

        public void Update(EntityManager entityManager, TileManager tileManager)
        {
            var iceDragon = entityManager.Enemies.FirstOrDefault(e => e.Type == EnemyType.IceDragon && e.IsBoss);
            if (iceDragon == null) return;

            // Increment counters
            iceDragon.IceDragonWinterCounter++;
            iceDragon.IceDragonSpawnCounter++;

            // Phase 1: Ewiger Winter - Every 4 turns
            if (iceDragon.IceDragonWinterCounter >= 4)
            {
                GD.Print("❄️💀 EWIGER WINTER! Alle Entities +3 Cold Stacks! ❄️💀");

                // Player
                entityManager.Player.ColdStacks += 3;

                // All Enemies
                foreach (var enemy in entityManager.Enemies)
                {
                    enemy.ColdStacks += 3;
                }

                iceDragon.IceDragonWinterCounter = 0;
            }

            // Phase 1: Wraith Spawn - Every 5 turns
            if (iceDragon.IceDragonSpawnCounter >= 5)
            {
                SpawnFrostbiteWraith(entityManager);
                iceDragon.IceDragonSpawnCounter = 0;
            }
        }

        public IBossState? CheckTransition(Enemy boss)
        {
            // Transition to Phase 2 at 50% HP
            if (!boss.IsPhase2 && boss.Hp <= boss.MaxHp / 2)
            {
                return new IceDragonPhase2State(_rng);
            }
            return null;
        }

        private void SpawnFrostbiteWraith(EntityManager entityManager)
        {
            GD.Print("👻❄️ FROSTBITE WRAITH ERSCHEINT! 👻❄️");

            var pos = FindFreePosition(entityManager);
            var wraith = EnemyRegistry.Get(EnemyType.Frostbite).Create(pos.X, pos.Y, CalculateEnemyLevel(entityManager) + 1);
            entityManager.AddEnemy(wraith);
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

    /// <summary>
    /// Ice Dragon boss state - Phase 2: ABSOLUTER NULL
    /// - Removes all campfires and torches
    /// - Increases ATK to 25
    /// - Warmes Herz Aura: -2 cold stacks to adjacent player
    /// - Continues Ewiger Winter and Wraith spawns
    /// </summary>
    public sealed class IceDragonPhase2State : IBossState
    {
        private readonly Random _rng;
        private bool _phase2Activated = false;

        public IceDragonPhase2State(Random rng)
        {
            _rng = rng;
        }

        public void Update(EntityManager entityManager, TileManager tileManager)
        {
            var iceDragon = entityManager.Enemies.FirstOrDefault(e => e.Type == EnemyType.IceDragon && e.IsBoss);
            if (iceDragon == null) return;

            // Activate Phase 2 once
            if (!_phase2Activated)
            {
                ActivatePhase2(iceDragon, tileManager);
                _phase2Activated = true;
            }

            // Continue Phase 1 mechanics
            iceDragon.IceDragonWinterCounter++;
            iceDragon.IceDragonSpawnCounter++;

            if (iceDragon.IceDragonWinterCounter >= 4)
            {
                GD.Print("❄️💀 EWIGER WINTER! Alle Entities +3 Cold Stacks! ❄️💀");
                entityManager.Player.ColdStacks += 3;
                foreach (var enemy in entityManager.Enemies)
                {
                    enemy.ColdStacks += 3;
                }
                iceDragon.IceDragonWinterCounter = 0;
            }

            if (iceDragon.IceDragonSpawnCounter >= 5)
            {
                SpawnFrostbiteWraith(entityManager);
                iceDragon.IceDragonSpawnCounter = 0;
            }

            // Phase 2: Warmes Herz Aura
            ProcessWarmesHerzAura(iceDragon, entityManager);
        }

        public IBossState? CheckTransition(Enemy boss)
        {
            // No further transitions
            return null;
        }

        private void ActivatePhase2(Enemy iceDragon, TileManager tileManager)
        {
            iceDragon.IsPhase2 = true;
            GD.Print("❄️🐉 ICE DRAGON PHASE 2: ABSOLUTER NULL! 🐉❄️");
            GD.Print("Die Kälte wird unerträglich! Alle Wärmequellen verschwinden!");

            // Remove all Campfires
            int campfireCount = tileManager.ClearAllCampfires();
            if (campfireCount > 0)
            {
                GD.Print($"🔥💨 {campfireCount} Lagerfeuer erlöschen!");
            }

            // Remove all Torches
            int torchCount = tileManager.ClearAllTorches();
            if (torchCount > 0)
            {
                GD.Print($"🔦💨 {torchCount} Fackeln erlöschen!");
            }

            // Increase ATK to 25
            iceDragon.Atk = 25;
            GD.Print($"🐉 Der Eisdrache wird stärker! ATK: 25");
            GD.Print($"❤️‍🔥 Nur noch das warme Herz des Drachens kann dich retten...");
        }

        private void ProcessWarmesHerzAura(Enemy iceDragon, EntityManager entityManager)
        {
            // Check if player is adjacent (orthogonal only, 1-tile melee range)
            int dx = System.Math.Abs(iceDragon.X - entityManager.Player.X);
            int dy = System.Math.Abs(iceDragon.Y - entityManager.Player.Y);
            bool isAdjacent = (dx == 1 && dy == 0) || (dx == 0 && dy == 1);

            if (isAdjacent && entityManager.Player.ColdStacks > 0)
            {
                int warmth = System.Math.Min(2, entityManager.Player.ColdStacks);
                entityManager.Player.ColdStacks -= warmth;
                GD.Print($"❤️‍🔥 Warmes Herz: -{warmth} Cold Stacks (Total: {entityManager.Player.ColdStacks})");
            }
        }

        private void SpawnFrostbiteWraith(EntityManager entityManager)
        {
            GD.Print("👻❄️ FROSTBITE WRAITH ERSCHEINT! 👻❄️");

            var pos = FindFreePosition(entityManager);
            var wraith = EnemyRegistry.Get(EnemyType.Frostbite).Create(pos.X, pos.Y, CalculateEnemyLevel(entityManager) + 1);
            entityManager.AddEnemy(wraith);
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
