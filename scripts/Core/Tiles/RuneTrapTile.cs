// ===== SAVE AS: scripts/Core/Tiles/RuneTrapTile.cs =====
using System.Linq;
using Godot;

namespace Dungeon2048.Core.Tiles
{
    public sealed class RuneTrapTile : ITileBehavior
    {
        public string Id => "tile.runetrap";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            // Fallen blockieren nicht
            return false;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            var trap = ctx.RuneTraps.FirstOrDefault(t => t.X == x && t.Y == y && !t.IsTriggered);
            if (trap == null) return;
            
            trap.IsRevealed = true;
            trap.IsTriggered = true;
            
            // Schaden nur am Spieler
            if (entity is Entities.Player player)
            {
                player.Hp -= RuneTrap.Damage;
                GD.Print($"⚡ RUNEN-FALLE! -{RuneTrap.Damage} HP!");
            }
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Hit-Mechanik
        }
    }
}