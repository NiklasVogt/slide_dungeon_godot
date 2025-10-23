// scripts/Core/Tiles/FallingRockTile.cs
using System.Linq;

namespace Dungeon2048.Core.Tiles
{
    public sealed class FallingRockTile : ITileBehavior
    {
        public string Id => "tile.fallingrock";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            // Falling Rocks blockieren keine Bewegung
            return false;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Schaden wird in GameContext.ProcessFallingRocks() behandelt
            // Keine direkte Aktion beim Betreten
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Keine Aktion
        }
    }
}
