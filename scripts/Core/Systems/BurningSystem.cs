// scripts/Core/Systems/BurningSystem.cs
using System.Collections.Generic;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Managers;
using Godot;

namespace Dungeon2048.Core.Systems
{
    /// <summary>
    /// Manages Burning status effect mechanics
    /// Burning stacks cause damage over time and are acquired from Fire Tiles
    /// </summary>
    public sealed class BurningSystem
    {
        /// <summary>
        /// Applies burning status to entities standing on fire tiles
        /// </summary>
        public void ApplyBurningFromFireTiles(EntityManager entityManager, TileManager tileManager)
        {
            // Check all enemies
            foreach (var enemy in entityManager.Enemies)
            {
                var fireTile = tileManager.GetFireTileAt(enemy.X, enemy.Y);
                if (fireTile != null)
                {
                    // Special handling for Moloch (immune to fire, but can heal)
                    if (enemy.Type == EnemyType.Moloch)
                    {
                        enemy.StandingOnFire = true;
                        continue;
                    }

                    // Special handling for Obsidian Warrior (absorbs fire)
                    if (enemy.Type == EnemyType.ObsidianWarrior)
                    {
                        continue;
                    }

                    // Normal enemies get burning
                    enemy.BurningStacks++;
                    enemy.BurningTurnsRemaining = 2;
                    GD.Print($"🔥 {enemy.DisplayName} beendet Zug auf Lava! Burning: {enemy.BurningStacks} Stacks");
                }
                else
                {
                    enemy.StandingOnFire = false;
                }
            }

            // Check player
            var playerFireTile = tileManager.GetFireTileAt(entityManager.Player.X, entityManager.Player.Y);
            if (playerFireTile != null)
            {
                entityManager.Player.BurningStacks++;
                entityManager.Player.BurningTurnsRemaining = 2;
                GD.Print($"🔥 Spieler beendet Zug auf Lava! Burning: {entityManager.Player.BurningStacks} Stacks");
            }
        }

        /// <summary>
        /// Processes Moloch healing on fire tiles
        /// </summary>
        public void ProcessMolochHealing(EntityManager entityManager, TileManager tileManager)
        {
            foreach (var enemy in entityManager.Enemies.Where(e => e.Type == EnemyType.Moloch))
            {
                var onFire = tileManager.GetFireTileAt(enemy.X, enemy.Y) != null;
                enemy.StandingOnFire = onFire;

                if (onFire)
                {
                    enemy.HealOnFire(5); // Heals 5 HP per turn on fire
                }
            }
        }
    }
}
