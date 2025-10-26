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
            // Lagerfeuer blockiert Bewegung - kann nicht betreten werden
            var campfire = ctx.CampfireTiles.FirstOrDefault(c => c.X == nx && c.Y == ny);
            if (campfire != null && !campfire.IsExtinguished)
            {
                return true;
            }
            return false;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Aktion beim Enter (sollte nicht passieren, da blockiert)
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Aktion beim Hit
        }

        /// <summary>
        /// Wird am Ende der Bewegungsphase aufgerufen
        /// Wärmt alle angrenzenden Entities
        /// </summary>
        public static void WarmAdjacentEntities(Services.GameContext ctx)
        {
            foreach (var campfire in ctx.CampfireTiles.ToList())
            {
                if (campfire.IsExtinguished) continue;

                bool warmedAny = false;

                // Prüfe Spieler
                if (campfire.IsAdjacent(ctx.Player.X, ctx.Player.Y))
                {
                    if (ctx.Player.FrostbiteStacks > 0)
                    {
                        int removed = System.Math.Min(ctx.Player.FrostbiteStacks, CampfireTile.WarmthAmount);
                        ctx.Player.FrostbiteStacks -= removed;
                        GD.Print($"🔥 Lagerfeuer wärmt Spieler: -{removed} Kälte-Stacks (jetzt {ctx.Player.FrostbiteStacks})");
                        warmedAny = true;
                    }
                }

                // Prüfe alle Gegner
                foreach (var enemy in ctx.Enemies.ToList())
                {
                    if (campfire.IsAdjacent(enemy.X, enemy.Y))
                    {
                        if (enemy.FrostbiteStacks > 0)
                        {
                            int removed = System.Math.Min(enemy.FrostbiteStacks, CampfireTile.WarmthAmount);
                            enemy.FrostbiteStacks -= removed;
                            GD.Print($"🔥 Lagerfeuer wärmt {enemy.DisplayName}: -{removed} Kälte-Stacks (jetzt {enemy.FrostbiteStacks})");
                            warmedAny = true;
                        }
                    }
                }

                // Wenn jemand gewärmt wurde, verbrauche eine Ladung
                if (warmedAny)
                {
                    campfire.ConsumeCharge();
                    GD.Print($"🔥 Lagerfeuer bei ({campfire.X}, {campfire.Y}) verbraucht Ladung: {campfire.Charges}/{CampfireTile.MaxCharges} verbleibend");

                    if (campfire.IsExtinguished)
                    {
                        GD.Print($"🔥 Lagerfeuer bei ({campfire.X}, {campfire.Y}) ist erloschen!");
                        // Markiere für Respawn im nächsten Zug
                        ctx.ExtinguishedCampfires.Add(campfire);
                    }
                }
            }
        }
    }
}
