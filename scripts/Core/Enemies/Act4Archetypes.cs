// scripts/Core/Enemies/Act4Archetypes.cs
using System;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Services;
using Dungeon2048.Core.Tiles;

namespace Dungeon2048.Core.Enemies
{
    // 1. Frost-Goblin - Standard-Gegner mit +1 Cold Attack
    public sealed class FrostGoblinArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.FrostGoblin;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 40; // Häufigster Standard-Gegner
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.FrostGoblin, level, boss);
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // 2. Yeti (Schneewanderer) - +2 Cold Attack, Campfire Drain
    public sealed class YetiArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Yeti;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 25; // Mittlere Häufigkeit, hohe HP
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.Yeti, level, boss);
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // 3. Ice Shard (Eiswolf) - Cold-Immun, kein Cold Attack
    public sealed class IceShardArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.IceShard;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 30; // Häufig, schnell
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.IceShard, level, boss);
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // 4. Frostbite (Geist der Erfrierung) - Phasing, +4 Cold, kein HP-Schaden
    public sealed class FrostbiteArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Frostbite;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 8; // Rare, sehr gefährlich
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.Frostbite, level, boss);
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // 5. Snowblind (Blizzard Shaman) - Campfire Drain alle 3 Züge
    public sealed class SnowblindArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Snowblind;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 6; // Rare, taktisch gefährlich
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var snowblind = new Enemy(x, y, EnemyType.Snowblind, level, boss);
            // Counter für Ability wird in Enemy-Klasse benötigt
            return snowblind;
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // 6. Glacial Sentinel (Permafrost Golem) - Aura +1 Cold/Zug
    public sealed class GlacialSentinelArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.GlacialSentinel;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 5; // Rare, sehr mächtig
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.GlacialSentinel, level, boss);
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // 7. Permafrost Lich (Chill Touch Stalker) - Statisch, Sightline +2 Cold/Zug
    public sealed class PermafrostLichArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.PermafrostLich;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 4; // Very Rare, extrem gefährlich
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.PermafrostLich, level, boss);
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // 8. Ice Dragon (Das Gefrorene Herz) - Boss mit 2 Phasen
    public sealed class IceDragonArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.IceDragon;

        public int CalcSpawnWeight(GameContext ctx) => 0; // Nur als Boss

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel() + 2;

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var dragon = new Enemy(x, y, EnemyType.IceDragon, level, boss);
            // Phase 2 wird bei 50% HP aktiviert
            dragon.IsPhase2 = false;
            return dragon;
        }

        public bool IsBossEligible(GameContext ctx) => ctx.CurrentLevel == 40;
    }
}
