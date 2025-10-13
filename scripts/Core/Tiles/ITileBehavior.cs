using Dungeon2048.Core.Services;

namespace Dungeon2048.Core.Tiles
{
    public interface ITileBehavior
    {
        string Id { get; }
        bool BlocksMovement(Entities.EntityBase entity, GameContext ctx, int nx, int ny);
        void OnEnter(Entities.EntityBase entity, GameContext ctx, int x, int y); // wenn in die Zelle gezogen wird
        void OnHit(Entities.EntityBase entity, GameContext ctx, int x, int y);   // wenn „davor“ interagiert (Schub/Kampf)
    }
}
