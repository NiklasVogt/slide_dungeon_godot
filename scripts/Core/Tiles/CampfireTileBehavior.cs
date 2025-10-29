// scripts/Core/Tiles/CampfireTileBehavior.cs
using System.Linq;
using Godot;

namespace Dungeon2048.Core.Tiles
{
    public sealed class CampfireTileBehavior : ITileBehavior
    {
        public string Id => "tile.campfire";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            // Campfire blockiert Bewegung wie Stones (für alle Entities)
            var campfire = ctx.Campfires.FirstOrDefault(c => c.X == nx && c.Y == ny);
            return campfire != null && !campfire.IsExtinguished;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Campfire wird nicht betreten, nur angrenzend genutzt
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Campfire kann nicht zerstört werden durch Hits
        }
    }
}
