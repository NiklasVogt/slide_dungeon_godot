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
        /// Wärmt den Spieler wenn er angrenzend ist und verbraucht immer eine Ladung
        /// </summary>
        public static void WarmAdjacentEntities(Services.GameContext ctx)
        {
            foreach (var campfire in ctx.CampfireTiles.ToList())
            {
                if (campfire.IsExtinguished) continue;

                // Prüfe ob Spieler angrenzend ist
                if (campfire.IsAdjacent(ctx.Player.X, ctx.Player.Y))
                {
                    // Reduziere Frostbite Stacks (kann auch 0 sein)
                    int removed = System.Math.Min(ctx.Player.FrostbiteStacks, CampfireTile.WarmthAmount);
                    if (removed > 0)
                    {
                        ctx.Player.FrostbiteStacks -= removed;
                        GD.Print($"🔥 Lagerfeuer wärmt Spieler: -{removed} Kälte-Stacks (jetzt {ctx.Player.FrostbiteStacks})");
                    }
                    else
                    {
                        GD.Print($"🔥 Spieler steht am Lagerfeuer (keine Kälte-Stacks)");
                    }

                    // IMMER eine Ladung verbrauchen wenn Spieler angrenzend ist
                    campfire.ConsumeCharge();
                    GD.Print($"🔥 Lagerfeuer bei ({campfire.X}, {campfire.Y}) verbraucht Ladung: {campfire.Charges}/{CampfireTile.MaxCharges} verbleibend");

                    if (campfire.IsExtinguished)
                    {
                        GD.Print($"🔥 Lagerfeuer bei ({campfire.X}, {campfire.Y}) ist erloschen!");
                        // Markiere für Respawn im nächsten Zug
                        ctx.ExtinguishedCampfires.Add(campfire);
                    }
                }

                // Gegner werden NICHT vom Lagerfeuer gewärmt (nur Spieler)
            }
        }
    }
}
