// scripts/Core/Tiles/GravestoneTile.cs
using System.Linq;

namespace Dungeon2048.Core.Tiles
{
    public sealed class GravestoneTile : ITileBehavior
    {
        public string Id => "tile.gravestone";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
            => ctx.Gravestones.Any(g => g.X == nx && g.Y == ny);

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Aktion beim Betreten
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            if (entity is not Entities.Player) return;

            var gravestone = ctx.Gravestones.FirstOrDefault(g => g.X == x && g.Y == y);
            if (gravestone == null) return;

            gravestone.HitCount++;
            if (gravestone.IsDestroyed)
            {
                ctx.Gravestones.RemoveAll(g => g.IsDestroyed);
            }
        }
    }
}