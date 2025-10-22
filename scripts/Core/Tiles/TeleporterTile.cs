// scripts/Core/Tiles/TeleporterTile.cs
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

        // ÄNDERUNG: OnEnter macht NICHTS mehr - Teleportation passiert später
        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine sofortige Teleportation mehr
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Hit-Mechanik
        }
    }
}