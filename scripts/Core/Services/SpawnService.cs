namespace Dungeon2048.Core.Services
{
    public static class SpawnService
    {
        public static void NextTickSpawns(GameContext ctx)
        {
            ctx.SpawnEnemies();
            // Weitere Regeln (Boss Adds etc.) können hier eingepluggt werden
        }
    }
}
