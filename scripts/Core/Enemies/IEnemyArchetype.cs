namespace Dungeon2048.Core.Enemies
{
    public interface IEnemyArchetype
    {
        Entities.EnemyType Type { get; }
        int CalcSpawnWeight(Services.GameContext ctx);
        int CalcLevel(Services.GameContext ctx);
        Entities.Enemy Create(int x, int y, int level, bool boss=false);
        bool IsBossEligible(Services.GameContext ctx);
    }
}
