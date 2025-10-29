// scripts/Core/Managers/TileManager.cs
using System.Collections.Generic;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Tiles;

namespace Dungeon2048.Core.Managers
{
    /// <summary>
    /// Manages all tile-based game objects (Gravestones, Torches, BonePiles, FireTiles, etc.)
    /// Follows Single Responsibility Principle - only responsible for tile lifecycle
    /// </summary>
    public sealed class TileManager
    {
        // Act 1: Catacombs Tiles
        public readonly List<Gravestone> Gravestones = new();
        public readonly List<Torch> Torches = new();
        public readonly List<BonePile> BonePiles = new();

        // Act 3: Volcan Forge Tiles
        public readonly List<FireTile> FireTiles = new();
        public readonly List<FallingRock> FallingRocks = new();

        // Act 4: Frost Depths Tiles
        public readonly List<CampfireTile> Campfires = new();

        /// <summary>
        /// Checks if a position is occupied by any tile
        /// </summary>
        public bool IsOccupiedByTile(int x, int y)
        {
            if (Gravestones.Any(g => g.X == x && g.Y == y)) return true;
            if (Torches.Any(t => t.X == x && t.Y == y)) return true;
            if (BonePiles.Any(b => b.X == x && b.Y == y)) return true;
            if (Campfires.Any(c => c.X == x && c.Y == y && !c.IsExtinguished)) return true;

            return false;
        }

        /// <summary>
        /// Clears all tiles (typically used when advancing to next level)
        /// </summary>
        public void ClearAllTiles()
        {
            Gravestones.Clear();
            Torches.Clear();
            BonePiles.Clear();
            FireTiles.Clear();
            FallingRocks.Clear();
            Campfires.Clear();
        }

        /// <summary>
        /// Ages bone piles and handles skeleton revival
        /// Returns list of bone piles that should revive as skeletons
        /// </summary>
        public List<BonePile> GetBonePilesToRevive()
        {
            var toRevive = new List<BonePile>();

            foreach (var pile in BonePiles)
            {
                pile.SwipesAlive++;

                if (pile.ShouldRevive)
                {
                    toRevive.Add(pile);
                }
            }

            return toRevive;
        }

        /// <summary>
        /// Removes a bone pile from the collection
        /// </summary>
        public bool RemoveBonePile(BonePile pile)
        {
            return BonePiles.Remove(pile);
        }

        /// <summary>
        /// Adds a bone pile at the specified position
        /// </summary>
        public void AddBonePile(int x, int y)
        {
            BonePiles.Add(new BonePile(x, y));
        }

        /// <summary>
        /// Processes fire tiles (removes extinguished ones)
        /// </summary>
        public void ProcessFireTiles()
        {
            FireTiles.RemoveAll(f => f.IsExtinguished);
        }

        /// <summary>
        /// Advances falling rock warning timers
        /// </summary>
        public void AdvanceFallingRocks()
        {
            foreach (var rock in FallingRocks.Where(r => r.IsWarning).ToList())
            {
                rock.AdvanceTurn();
            }
        }

        /// <summary>
        /// Gets all falling rocks that are ready to fall
        /// </summary>
        public List<FallingRock> GetFallingRocksReady()
        {
            return FallingRocks.Where(r => r.ShouldFall).ToList();
        }

        /// <summary>
        /// Removes a falling rock from the collection
        /// </summary>
        public bool RemoveFallingRock(FallingRock rock)
        {
            return FallingRocks.Remove(rock);
        }

        /// <summary>
        /// Adds a fire tile at the specified position
        /// </summary>
        public void AddFireTile(int x, int y)
        {
            if (!FireTiles.Any(f => f.X == x && f.Y == y))
            {
                FireTiles.Add(new FireTile(x, y));
            }
        }

        /// <summary>
        /// Gets fire tile at specified position, if any
        /// </summary>
        public FireTile? GetFireTileAt(int x, int y)
        {
            return FireTiles.FirstOrDefault(f => f.X == x && f.Y == y && !f.IsExtinguished);
        }

        /// <summary>
        /// Gets campfire at specified position, if any
        /// </summary>
        public CampfireTile? GetCampfireAt(int x, int y)
        {
            return Campfires.FirstOrDefault(c => c.X == x && c.Y == y && !c.IsExtinguished);
        }

        /// <summary>
        /// Ensures only one campfire exists (safety check for Act 4)
        /// </summary>
        public void EnsureSingleCampfire()
        {
            if (Campfires.Count > 1)
            {
                Godot.GD.PrintErr($"⚠️ WARNING: {Campfires.Count} campfires exist! Removing duplicates...");
                while (Campfires.Count > 1)
                {
                    Campfires.RemoveAt(Campfires.Count - 1);
                }
            }
        }

        /// <summary>
        /// Removes all campfires (used in Ice Dragon Phase 2)
        /// </summary>
        public int ClearAllCampfires()
        {
            int count = Campfires.Count;
            Campfires.Clear();
            return count;
        }

        /// <summary>
        /// Removes all torches (used in Ice Dragon Phase 2)
        /// </summary>
        public int ClearAllTorches()
        {
            int count = Torches.Count;
            Torches.Clear();
            return count;
        }
    }
}
