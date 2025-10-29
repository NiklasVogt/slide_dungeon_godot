// scripts/Core/Managers/EntityManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon2048.Core.Entities;

namespace Dungeon2048.Core.Managers
{
    /// <summary>
    /// Manages all game entities (Player, Enemies, Stones, Doors, etc.)
    /// Follows Single Responsibility Principle - only responsible for entity lifecycle
    /// </summary>
    public sealed class EntityManager
    {
        // Entity Collections
        public Player Player { get; set; } = null!;
        public readonly List<Enemy> Enemies = new();
        public readonly List<Stone> Stones = new();
        public readonly List<SpellDrop> SpellDrops = new();
        public readonly List<Teleporter> Teleporters = new();
        public readonly List<RuneTrap> RuneTraps = new();
        public readonly List<MagicBarrier> MagicBarriers = new();
        public Door? Door { get; set; }

        /// <summary>
        /// Checks if a position is occupied by any entity
        /// </summary>
        public bool IsOccupiedByEntity(int x, int y, bool ignorePlayer = false)
        {
            if (!ignorePlayer && Player.X == x && Player.Y == y) return true;
            if (Enemies.Any(e => e.X == x && e.Y == y)) return true;
            if (Stones.Any(s => s.X == x && s.Y == y)) return true;
            if (SpellDrops.Any(sd => sd.X == x && sd.Y == y)) return true;
            if (Door != null && Door.X == x && Door.Y == y) return true;
            if (Teleporters.Any(t => t.X == x && t.Y == y && t.IsActive)) return false; // Teleporter don't block
            if (RuneTraps.Any(r => r.X == x && r.Y == y && !r.IsTriggered)) return false; // Traps don't block
            if (MagicBarriers.Any(m => m.X == x && m.Y == y && !m.IsDestroyed)) return true;

            return false;
        }

        /// <summary>
        /// Clears all entities (typically used when advancing to next level)
        /// </summary>
        public void ClearAllEntities()
        {
            Enemies.Clear();
            Stones.Clear();
            SpellDrops.Clear();
            Teleporters.Clear();
            RuneTraps.Clear();
            MagicBarriers.Clear();
            Door = null;
        }

        /// <summary>
        /// Removes a specific enemy from the collection
        /// </summary>
        public bool RemoveEnemy(Enemy enemy)
        {
            return Enemies.Remove(enemy);
        }

        /// <summary>
        /// Adds an enemy to the collection
        /// </summary>
        public void AddEnemy(Enemy enemy)
        {
            Enemies.Add(enemy);
        }

        /// <summary>
        /// Gets all enemies of a specific type
        /// </summary>
        public IEnumerable<Enemy> GetEnemiesByType(EnemyType type)
        {
            return Enemies.Where(e => e.Type == type);
        }

        /// <summary>
        /// Gets the first enemy of a specific type, or null if none exist
        /// </summary>
        public Enemy? GetFirstEnemyByType(EnemyType type)
        {
            return Enemies.FirstOrDefault(e => e.Type == type);
        }

        /// <summary>
        /// Removes a spell drop from the collection
        /// </summary>
        public bool RemoveSpellDrop(SpellDrop drop)
        {
            return SpellDrops.Remove(drop);
        }

        /// <summary>
        /// Regenerates destroyed magic barriers after a certain number of swipes
        /// </summary>
        public void RegenerateMagicBarriers()
        {
            foreach (var barrier in MagicBarriers)
            {
                if (barrier.IsDestroyed)
                {
                    barrier.SwipesSinceBroken++;

                    if (barrier.ShouldRegenerate)
                    {
                        barrier.Reset();
                        Godot.GD.Print($"✨ Magische Barriere regeneriert bei ({barrier.X}, {barrier.Y})!");
                    }
                }
            }
        }

        /// <summary>
        /// Processes teleporter mechanics for all entities
        /// </summary>
        public void ProcessTeleporters()
        {
            if (Teleporters.Count == 0) return;

            var teleportQueue = new List<(EntityBase entity, int targetX, int targetY)>();

            // Check Player
            var playerTeleporter = Teleporters.FirstOrDefault(t =>
                t.IsActive && t.X == Player.X && t.Y == Player.Y && t.LinkedTeleporterId != null
            );

            if (playerTeleporter != null)
            {
                var target = Teleporters.FirstOrDefault(t => t.Id == playerTeleporter.LinkedTeleporterId);
                if (target != null)
                {
                    teleportQueue.Add((Player, target.X, target.Y));
                }
            }

            // Check Enemies
            foreach (var enemy in Enemies.ToList())
            {
                var enemyTeleporter = Teleporters.FirstOrDefault(t =>
                    t.IsActive && t.X == enemy.X && t.Y == enemy.Y && t.LinkedTeleporterId != null
                );

                if (enemyTeleporter != null)
                {
                    var target = Teleporters.FirstOrDefault(t => t.Id == enemyTeleporter.LinkedTeleporterId);
                    if (target != null)
                    {
                        teleportQueue.Add((enemy, target.X, target.Y));
                    }
                }
            }

            // Execute all teleportations
            foreach (var (entity, targetX, targetY) in teleportQueue)
            {
                var oldPos = (entity.X, entity.Y);
                entity.X = targetX;
                entity.Y = targetY;

                string entityName = entity is Player ? "Spieler" :
                                   entity is Enemy e ? e.DisplayName : "Entity";

                Godot.GD.Print($"🌀 {entityName} teleportiert von ({oldPos.X},{oldPos.Y}) → ({targetX},{targetY})!");
            }
        }
    }
}
