using System.Collections.Generic;
using Dungeon2048.Core.Services;

namespace Dungeon2048.Core.Tiles
{
    public static class TileRegistry
    {
        private static readonly List<ITileBehavior> _tiles = new();

        public static void Register(ITileBehavior tile) => _tiles.Add(tile);

        public static bool AnyBlocks(Entities.EntityBase entity, GameContext ctx, int nx, int ny)
        {
            foreach (var t in _tiles) if (t.BlocksMovement(entity, ctx, nx, ny)) return true;
            return false;
        }

        public static void Enter(Entities.EntityBase entity, GameContext ctx, int x, int y)
        {
            foreach (var t in _tiles) t.OnEnter(entity, ctx, x, y);
        }

        public static void Hit(Entities.EntityBase entity, GameContext ctx, int x, int y)
        {
            foreach (var t in _tiles) t.OnHit(entity, ctx, x, y);
        }
    }
}
