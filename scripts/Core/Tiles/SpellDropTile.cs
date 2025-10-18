using System.Linq;

namespace Dungeon2048.Core.Tiles
{
    public sealed class SpellDropTile : ITileBehavior
    {
        public string Id => "tile.spelldrop";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            bool hasDrop = ctx.SpellDrops.Any(d => d.X == nx && d.Y == ny);
            
            // NEU: Getarnter Mimic blockiert auch wie Spell Drop
            bool hasDisguisedMimic = ctx.Enemies.Any(e => 
                e.Type == Entities.EnemyType.Mimic && 
                e.IsDisguised && 
                e.X == nx && 
                e.Y == ny
            );
            
            if (!hasDrop && !hasDisguisedMimic) return false;
            
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
                
                // NEU: Spieler betritt Feld mit getarntem Mimic -> Reveal
                var mimic = ctx.Enemies.FirstOrDefault(e => 
                    e.Type == Entities.EnemyType.Mimic && 
                    e.IsDisguised && 
                    e.X == x && 
                    e.Y == y
                );
                
                if (mimic != null)
                {
                    mimic.IsDisguised = false;
                    mimic.MimicHitCount = 0;
                    Godot.GD.Print("💀 Das war ein MIMIC! Er greift an!");
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
                
                // NEU: Enemy trifft getarnten Mimic
                var mimic = ctx.Enemies.FirstOrDefault(e => 
                    e.Type == Entities.EnemyType.Mimic && 
                    e.IsDisguised && 
                    e.X == x && 
                    e.Y == y
                );
                
                if (mimic != null)
                {
                    mimic.MimicHitCount++;
                    Godot.GD.Print($"Mimic wurde getroffen! ({mimic.MimicHitCount}/{Entities.Enemy.MimicHitsToReveal})");
                    
                    if (mimic.MimicHitCount >= Entities.Enemy.MimicHitsToReveal)
                    {
                        mimic.IsDisguised = false;
                        mimic.MimicHitCount = 0;
                        Godot.GD.Print("💀 Der Mimic wurde enttarnt!");
                    }
                }
            }
        }
    }
}