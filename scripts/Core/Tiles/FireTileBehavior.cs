// scripts/Core/Tiles/FireTileBehavior.cs
using System.Linq;
using Godot;

namespace Dungeon2048.Core.Tiles
{
    public sealed class FireTileBehavior : ITileBehavior
    {
        public string Id => "tile.fire";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            // Feuer blockiert nicht die Bewegung
            return false;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            var fireTile = ctx.FireTiles.FirstOrDefault(f => f.X == x && f.Y == y);
            if (fireTile == null || fireTile.IsExtinguished) return;

            // Wenn Moloch, keine Schaden aber zähle Pass
            if (entity is Entities.Enemy enemy && enemy.Type == Entities.EnemyType.Moloch)
            {
                fireTile.OnEntityPass();
                return;
            }

            // Wenn Obsidian Warrior, absorbiere den Schaden
            if (entity is Entities.Enemy obsidian && obsidian.Type == Entities.EnemyType.ObsidianWarrior)
            {
                obsidian.AbsorbFireDamage(FireTile.BurningDamage);
                fireTile.OnEntityPass();
                return;
            }

            // Normale Entities nehmen Schaden und bekommen Burning
            if (fireTile.OnEntityPass())
            {
                entity.Hp -= FireTile.BurningDamage;
                GD.Print($"{GetEntityName(entity)} betritt Feuer! {FireTile.BurningDamage} Schaden!");

                // Burning Status hinzufügen
                if (entity is Entities.Enemy e)
                {
                    e.BurningStacks++;
                    GD.Print($"{e.DisplayName} brennt jetzt! ({e.BurningStacks} Stacks)");
                }
                else if (entity is Entities.Player player)
                {
                    player.BurningStacks++;
                    GD.Print($"Spieler brennt jetzt! ({player.BurningStacks} Stacks)");
                }
            }
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Aktion beim Hit
        }

        private string GetEntityName(Entities.EntityBase entity)
        {
            if (entity is Entities.Player) return "Spieler";
            if (entity is Entities.Enemy e) return e.DisplayName;
            return "Entity";
        }
    }
}
