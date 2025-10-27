// scripts/Core/Tiles/TorchTile.cs
using System.Linq;

namespace Dungeon2048.Core.Tiles
{
    public sealed class TorchTile : ITileBehavior
    {
        public string Id => "tile.torch";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            bool hasTorch = ctx.Torches.Any(t => t.X == nx && t.Y == ny);
            if (!hasTorch) return false;
            
            // Player kann durch, Enemies werden blockiert
            return entity is Entities.Enemy;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Akt 4: Torches entfernen Cold Stacks wenn Player sie betritt
            if (entity is Entities.Player player &&
                ctx.BiomeSystem.CurrentBiome?.Type == World.BiomeType.FrostDepths)
            {
                bool hasTorch = ctx.Torches.Any(t => t.X == x && t.Y == y);
                if (hasTorch && player.ColdStacks > 0)
                {
                    player.ColdStacks--;
                    Godot.GD.Print($"🔦 Torch Wärme! Player -1 Cold Stack (Total: {player.ColdStacks})");
                }
            }
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Torches können nicht zerstört werden
        }
    }
}