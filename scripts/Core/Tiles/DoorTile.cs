namespace Dungeon2048.Core.Tiles
{
    public sealed class DoorTile : ITileBehavior
    {
        public string Id => "tile.door";

        public bool BlocksMovement(Entities.EntityBase entity, Services.GameContext ctx, int nx, int ny)
        {
            var d = ctx.Door;
            if (d == null || !d.IsActive) return false;

            bool isDoor = d.X == nx && d.Y == ny;
            if (!isDoor) return false;

            // Tür blockiert grundsätzlich die Bewegung, der Spieler darf in ResolveAfterMove hineinrücken (Door-Enter),
            // Enemies bleiben blockiert.
            return true;
        }

        public void OnEnter(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            var d = ctx.Door;
            if (d == null || !d.IsActive) return;
            if (d.X != x || d.Y != y) return;

            // Nur Spieler aktiviert die Tür / Levelwechsel
            if (entity is Entities.Player)
            {
                ctx.InteractWithDoor();
            }
        }

        public void OnHit(Entities.EntityBase entity, Services.GameContext ctx, int x, int y)
        {
            // Türen werden nicht „abgebaut“, keine Wirkung für Player oder Enemy
        }
    }
}
