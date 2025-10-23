// scripts/Core/Enemies/LegacyArchetypes.cs
using System;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Services;

namespace Dungeon2048.Core.Enemies
{
    // Thorns - Reflektiert Schaden zurück
    public sealed class ThornsArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Thorns;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 10; // Rare, gefährlich
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.Thorns, level, boss);
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }
}
