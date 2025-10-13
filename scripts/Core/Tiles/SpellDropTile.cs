using System.Linq;

namespace Dungeon2048.Core.Tiles
{
    public sealed class SpellDropTile : ITileBehavior
    {
        public string Id => "tile.spelldrop";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            bool hasDrop = ctx.SpellDrops.Any(d => d.X == nx && d.Y == ny);
            if (!hasDrop) return false;
            // Spieler darf durchsliden (Pickup via Enter/Sweep), Gegner werden blockiert
            return entity is Entities.Enemy;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            if (entity is Entities.Player)
            {
                int idx = ctx.SpellDrops.FindIndex(d => d.X == x && d.Y == y);
                if (idx != -1)
                {
                    var drop = ctx.SpellDrops[idx];
                    ctx.RegisterSpellPickup(drop);
                }
            }
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            if (entity is Entities.Enemy)
            {
                int idx = ctx.SpellDrops.FindIndex(d => d.X == x && d.Y == y);
                if (idx != -1)
                {
                    var drop = ctx.SpellDrops[idx];
                    drop.HitCount++;
                    if (drop.IsDestroyed)
                    {
                        ctx.SpellDrops.RemoveAt(idx);
                    }
                }
            }
        }
    }
}
