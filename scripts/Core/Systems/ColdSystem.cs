// scripts/Core/Systems/ColdSystem.cs
using System.Collections.Generic;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Managers;
using Dungeon2048.Core.World;
using Godot;

namespace Dungeon2048.Core.Systems
{
    /// <summary>
    /// Manages Cold Stack mechanics for Act 4: Frost Depths
    /// Cold stacks accumulate over time and can kill entities when reaching resistance threshold
    /// </summary>
    public sealed class ColdSystem
    {
        private int _frostwindCounter = 0;
        private int _totalMovementTiles = 0;

        public int FrostwindCounter => _frostwindCounter;
        public int TotalMovementTiles => _totalMovementTiles;

        /// <summary>
        /// Processes cold accumulation from environment
        /// Called during RegisterSwipe()
        /// </summary>
        public void ProcessColdAccumulation(EntityManager entityManager, int totalSwipes, BiomeType? currentBiome)
        {
            // Only active in Frost Depths biome
            if (currentBiome != BiomeType.FrostDepths)
                return;

            bool isNightPhase = totalSwipes >= 30;

            // 1. Passive environmental cold: Every 3 swipes +1 stack (all entities)
            if (totalSwipes % 3 == 0)
            {
                int baseStacks = 1;
                int nightModifier = isNightPhase ? 2 : 1; // Night phase doubles
                int stacksToAdd = baseStacks * nightModifier;

                entityManager.Player.ColdStacks += stacksToAdd;
                GD.Print($"❄️ Passive Kälte: Player +{stacksToAdd} Cold Stacks (Total: {entityManager.Player.ColdStacks})");

                foreach (var enemy in entityManager.Enemies)
                {
                    enemy.ColdStacks += stacksToAdd;
                }
            }

            // 2. Frostwind Event: Every 10 swipes +2 stacks (all entities)
            _frostwindCounter++;
            if (_frostwindCounter >= 10)
            {
                GD.Print("🌬️ FROSTWIND EVENT! Alle Entities erhalten +2 Cold Stacks!");
                entityManager.Player.ColdStacks += 2;

                foreach (var enemy in entityManager.Enemies)
                {
                    enemy.ColdStacks += 2;
                }

                _frostwindCounter = 0;
            }

            // 3. Night Phase Warning
            if (isNightPhase && totalSwipes == 30)
            {
                GD.Print("🌙 NACHT-PHASE BEGINNT! Kälte-Rate verdoppelt!");
            }
        }

        /// <summary>
        /// Processes cold damage and kills entities that exceed their cold resistance
        /// Called after movement/combat is complete
        /// </summary>
        public void ProcessColdDamage(EntityManager entityManager, BiomeType? currentBiome)
        {
            // Only active in Frost Depths biome
            if (currentBiome != BiomeType.FrostDepths)
                return;

            // Check Player death from cold
            if (entityManager.Player.ColdStacks >= entityManager.Player.ColdResistance)
            {
                entityManager.Player.Hp = 0;
                GD.Print($"❄️💀 Player ist erfroren! ({entityManager.Player.ColdStacks}/{entityManager.Player.ColdResistance} Stacks)");
            }

            // Check Enemies death from cold
            var frozenEnemies = new List<Enemy>();
            foreach (var enemy in entityManager.Enemies)
            {
                if (enemy.ColdStacks >= enemy.ColdResistance)
                {
                    frozenEnemies.Add(enemy);
                    GD.Print($"❄️💀 {enemy.DisplayName} ist erfroren! ({enemy.ColdStacks}/{enemy.ColdResistance} Stacks)");
                }
            }

            // Remove frozen enemies (caller must handle objective tracking)
            foreach (var frozen in frozenEnemies)
            {
                entityManager.RemoveEnemy(frozen);
            }
        }

        /// <summary>
        /// Processes Glacial Sentinel auras (+1 cold stack to entities in 1-tile radius)
        /// </summary>
        public void ProcessGlacialSentinelAuras(EntityManager entityManager, BiomeType? currentBiome)
        {
            // Only active in Frost Depths biome
            if (currentBiome != BiomeType.FrostDepths)
                return;

            var sentinels = entityManager.GetEnemiesByType(EnemyType.GlacialSentinel).ToList();
            if (!sentinels.Any()) return;

            foreach (var sentinel in sentinels)
            {
                // Check all entities in 1-tile radius (8 tiles around sentinel, including diagonal)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue; // Sentinel itself

                        int checkX = sentinel.X + dx;
                        int checkY = sentinel.Y + dy;

                        // Player check
                        if (entityManager.Player.X == checkX && entityManager.Player.Y == checkY)
                        {
                            entityManager.Player.ColdStacks++;
                            GD.Print($"🧊 Glacial Sentinel Aura: Player +1 Cold Stack");
                        }

                        // Enemy check
                        foreach (var enemy in entityManager.Enemies)
                        {
                            if (enemy.X == checkX && enemy.Y == checkY && enemy.Id != sentinel.Id)
                            {
                                enemy.ColdStacks++;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes Permafrost Lich chill (+2 cold stacks to player with line of sight)
        /// </summary>
        public void ProcessPermafrostLichChill(EntityManager entityManager, BiomeType? currentBiome)
        {
            // Only active in Frost Depths biome
            if (currentBiome != BiomeType.FrostDepths)
                return;

            var liches = entityManager.GetEnemiesByType(EnemyType.PermafrostLich).ToList();
            if (!liches.Any()) return;

            foreach (var lich in liches)
            {
                // Check if player is on same X OR Y axis and direct line of sight
                bool sameX = lich.X == entityManager.Player.X;
                bool sameY = lich.Y == entityManager.Player.Y;

                if (!sameX && !sameY) continue;

                // Sightline Check: No enemies between Lich and Player
                bool hasSightline = true;

                if (sameX)
                {
                    // Same X axis: Check Y values between
                    int minY = System.Math.Min(lich.Y, entityManager.Player.Y);
                    int maxY = System.Math.Max(lich.Y, entityManager.Player.Y);

                    foreach (var enemy in entityManager.Enemies)
                    {
                        if (enemy.Id != lich.Id && enemy.X == lich.X && enemy.Y > minY && enemy.Y < maxY)
                        {
                            hasSightline = false;
                            break;
                        }
                    }
                }
                else if (sameY)
                {
                    // Same Y axis: Check X values between
                    int minX = System.Math.Min(lich.X, entityManager.Player.X);
                    int maxX = System.Math.Max(lich.X, entityManager.Player.X);

                    foreach (var enemy in entityManager.Enemies)
                    {
                        if (enemy.Id != lich.Id && enemy.Y == lich.Y && enemy.X > minX && enemy.X < maxX)
                        {
                            hasSightline = false;
                            break;
                        }
                    }
                }

                if (hasSightline)
                {
                    entityManager.Player.ColdStacks += 2;
                    GD.Print($"🔷 Permafrost Lich Chill: Player +2 Cold Stacks (Sightline!)");
                }
            }
        }

        /// <summary>
        /// Processes combat warmth (-1 cold stack per kill)
        /// </summary>
        public void ProcessCombatWarmth(Player player, BiomeType? currentBiome)
        {
            // Only active in Frost Depths biome
            if (currentBiome != BiomeType.FrostDepths)
                return;

            if (player.ColdStacks > 0)
            {
                player.ColdStacks--;
                GD.Print($"⚔️ Kampf-Wärme! Player -1 Cold Stack (Total: {player.ColdStacks})");
            }
        }

        /// <summary>
        /// Resets counters when advancing to next level
        /// </summary>
        public void ResetCounters()
        {
            _frostwindCounter = 0;
            _totalMovementTiles = 0;
        }
    }
}
