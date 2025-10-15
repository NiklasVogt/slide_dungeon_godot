// scripts/Core/Tiles/BonePileTile.cs
using System.Linq;

namespace Dungeon2048.Core.Tiles
{
    public sealed class BonePileTile : ITileBehavior
    {
        public string Id => "tile.bonepile";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
            => ctx.BonePiles.Any(b => b.X == nx && b.Y == ny);

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Aktion
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            if (entity is not Entities.Player) return;

            var bonePile = ctx.BonePiles.FirstOrDefault(b => b.X == x && b.Y == y);
            if (bonePile == null) return;

            bonePile.HitCount++;
            if (bonePile.IsDestroyed)
            {
                ctx.BonePiles.RemoveAll(b => b.IsDestroyed);
            }
        }
    }
}