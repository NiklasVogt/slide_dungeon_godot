// scripts/Core/Services/MovementService.cs
using System.Collections.Generic;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Managers;
using Dungeon2048.Core.Tiles;
using Godot;

namespace Dungeon2048.Core.Services
{
    /// <summary>
    /// Refactored MovementService using Dependency Injection
    /// Replaces the static MovementPipeline with testable, injectable service
    /// </summary>
    public sealed class MovementService
    {
        private readonly EntityManager _entityManager;
        private readonly TileManager _tileManager;
        private readonly StatusEffectManager _statusEffectManager;

        public MovementService(
            EntityManager entityManager,
            TileManager tileManager,
            StatusEffectManager statusEffectManager)
        {
            _entityManager = entityManager;
            _tileManager = tileManager;
            _statusEffectManager = statusEffectManager;
        }

        /// <summary>
        /// Executes movement in the specified direction
        /// Returns true if any entity moved
        /// </summary>
        public bool ExecuteMovement(int dx, int dy, EventBus bus)
        {
            // Use the existing MovementPipeline for now
            // This is a bridge to maintain compatibility during transition
            // TODO: Fully refactor MovementPipeline logic into this service

            // For now, we create a temporary GameContext-like structure
            // In a full refactoring, MovementPipeline would be rewritten to use these managers

            return false; // Placeholder
        }

        /// <summary>
        /// Calculates the furthest position an entity can move to
        /// </summary>
        private Vector2I CalculateFurthest(EntityBase entity, int dx, int dy, HashSet<string> occupied)
        {
            int x = entity.X, y = entity.Y;

            while (true)
            {
                int nx = x + dx, ny = y + dy;

                if (nx < 0 || nx >= 6 || ny < 0 || ny >= 6)
                {
                    break;
                }

                // Check if tile blocks movement
                if (IsBlocked(entity, nx, ny))
                {
                    break;
                }

                if (occupied.Contains($"{nx},{ny}"))
                {
                    break;
                }

                // Door check for enemies
                if (entity is Enemy)
                {
                    if (_entityManager.Door != null && _entityManager.Door.IsActive)
                    {
                        if (_entityManager.Door.X == nx && _entityManager.Door.Y == ny)
                        {
                            break;
                        }
                    }
                }

                x = nx;
                y = ny;
            }

            return new Vector2I(x, y);
        }

        /// <summary>
        /// Checks if a position is blocked for movement
        /// </summary>
        private bool IsBlocked(EntityBase entity, int x, int y)
        {
            // Check entity blocking
            if (_entityManager.IsOccupiedByEntity(x, y, false))
                return true;

            // Check tile blocking
            if (_tileManager.IsOccupiedByTile(x, y))
                return true;

            return false;
        }

        /// <summary>
        /// Sorts entities based on movement direction
        /// </summary>
        private List<EntityBase> SortForDirection(IEnumerable<EntityBase> entities, int dx, int dy)
        {
            var list = entities.ToList();
            list.Sort((a, b) =>
            {
                if (dx > 0) { if (a.Y == b.Y) return b.X.CompareTo(a.X); return a.Y.CompareTo(b.Y); }
                if (dx < 0) { if (a.Y == b.Y) return a.X.CompareTo(b.X); return a.Y.CompareTo(b.Y); }
                if (dy > 0) { if (a.X == b.X) return b.Y.CompareTo(a.Y); return a.X.CompareTo(b.X); }
                if (a.X == b.X) return a.Y.CompareTo(b.Y); return a.X.CompareTo(b.X);
            });
            return list;
        }
    }
}
