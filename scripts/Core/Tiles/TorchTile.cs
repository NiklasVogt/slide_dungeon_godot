// scripts/Core/Tiles/TorchTile.cs
using System.Linq;

namespace Dungeon2048.Core.Tiles
{
    public sealed class TorchTile : ITileBehavior
    {
        public string Id => "tile.torch";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            bool hasTorch = ctx.Torches.Any(t => t.X == nx && t.Y == ny);
            if (!hasTorch) return false;
            
            // Player kann durch, Enemies werden blockiert
            return entity is Entities.Enemy;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Aktion
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Torches können nicht zerstört werden
        }
    }
}