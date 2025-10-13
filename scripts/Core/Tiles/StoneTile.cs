using System.Linq;

namespace Dungeon2048.Core.Tiles
{
    public sealed class StoneTile : ITileBehavior
    {
        public string Id => "tile.stone";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
            => ctx.Stones.Any(s => s.X == nx && s.Y == ny);

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Aktion beim Betreten, Stones werden nur per „Hit“ behandelt.
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Nur der Spieler darf Steine beschädigen.
            if (entity is not Entities.Player) return;

            var stone = ctx.Stones.FirstOrDefault(s => s.X == x && s.Y == y);
            if (stone == null) return;

            stone.HitCount++;
            if (stone.IsDestroyed)
            {
                ctx.Stones.RemoveAll(s => s.IsDestroyed);
                // MovementPipeline rückt den Spieler in derselben Tick-Resolution nach.
            }
        }
    }
}
