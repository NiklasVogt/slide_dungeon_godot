using System.Linq;

namespace Dungeon2048.Core.Tiles
{
    public sealed class MagicBarrierTile : ITileBehavior
    {
        public string Id => "tile.magicbarrier";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            var barrier = ctx.MagicBarriers.FirstOrDefault(b => b.X == nx && b.Y == ny);
            return barrier != null && !barrier.IsDestroyed;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Aktion
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            if (entity is not Entities.Player) return;

            var barrier = ctx.MagicBarriers.FirstOrDefault(b => b.X == x && b.Y == y);
            if (barrier == null || barrier.IsDestroyed) return;

            barrier.HitCount++;
            
            if (barrier.IsDestroyed)
            {
                barrier.SwipesSinceBroken = 0;
            }
        }
    }
}