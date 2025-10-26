// scripts/Core/Tiles/FrostTorchTile.cs
using System.Linq;
using Godot;

namespace Dungeon2048.Core.Tiles
{
    public sealed class FrostTorchTile : ITileBehavior
    {
        public string Id => "tile.frosttorch";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            var torch = ctx.FrostTorches.FirstOrDefault(t => t.X == nx && t.Y == ny);
            if (torch == null || torch.IsExtinguished) return false;

            // Player kann durch, Enemies werden blockiert (außer Boss in Phase 2)
            if (entity is Entities.Enemy e && e.Type == Entities.EnemyType.IceDragon && e.IsPhase2)
            {
                // Boss in Phase 2 kann Fackeln löschen
                return false;
            }

            return entity is Entities.Enemy;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            var torch = ctx.FrostTorches.FirstOrDefault(t => t.X == x && t.Y == y);
            if (torch == null || torch.IsExtinguished) return;

            // Nur Spieler kann Fackeln betreten und profitieren
            if (entity is Entities.Player player)
            {
                if (player.FrostbiteStacks > 0)
                {
                    player.FrostbiteStacks--;
                    GD.Print($"🔥 Fackel wärmt dich! -1 Kälte-Stack (jetzt {player.FrostbiteStacks})");
                }
            }
            // Boss in Phase 2 löscht Fackel
            else if (entity is Entities.Enemy e && e.Type == Entities.EnemyType.IceDragon && e.IsPhase2)
            {
                torch.IsExtinguished = true;
                GD.Print($"❄️ {e.DisplayName} löscht die Fackel!");
            }
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Aktion beim Hit
        }
    }
}
