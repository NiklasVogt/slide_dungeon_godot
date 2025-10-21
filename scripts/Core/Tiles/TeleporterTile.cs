// ===== SAVE AS: scripts/Core/Tiles/TeleporterTile.cs =====
using System.Linq;
using Godot;

namespace Dungeon2048.Core.Tiles
{
    public sealed class TeleporterTile : ITileBehavior
    {
        public string Id => "tile.teleporter";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            // Teleporter blockieren nicht
            return false;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            var teleporter = ctx.Teleporters.FirstOrDefault(t => t.X == x && t.Y == y && t.IsActive);
            if (teleporter == null || teleporter.LinkedTeleporterId == null) return;
            
            var target = ctx.Teleporters.FirstOrDefault(t => t.Id == teleporter.LinkedTeleporterId);
            if (target == null) return;
            
            // Teleportiere Entity
            entity.X = target.X;
            entity.Y = target.Y;
            
            GD.Print($"🌀 {(entity is Entities.Player ? "Spieler" : "Gegner")} teleportiert zu ({target.X}, {target.Y})!");
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Hit-Mechanik
        }
    }
}