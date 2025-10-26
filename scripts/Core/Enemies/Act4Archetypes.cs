// scripts/Core/Enemies/Act4Archetypes.cs
using System;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Services;

namespace Dungeon2048.Core.Enemies
{
    // Frost-Goblin - Standard Gegner, 5 Kälte-Resistenz, +1 Stack bei Angriff
    public sealed class FrostGoblinArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.FrostGoblin;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 35; // Häufigster Standard-Gegner in Akt 4
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var goblin = new Enemy(x, y, EnemyType.FrostGoblin, level, boss);
            goblin.FrostbiteResistance = 5; // Stirbt bei 5 Frostbite Stacks
            return goblin;
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Schneewanderer (Yeti) - 7 Resistenz, entzieht Lagerfeuer-Ladung
    public sealed class YetiArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Yeti;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 30; // Häufig, mittlere Stärke
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var yeti = new Enemy(x, y, EnemyType.Yeti, level, boss);
            yeti.FrostbiteResistance = 7; // Stirbt bei 7 Frostbite Stacks
            return yeti;
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Eiswolf (IceShard) - Immun gegen Kälte, jagt immer
    public sealed class IceShardArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.IceShard;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 25; // Mittlere Häufigkeit, gefährlich
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var wolf = new Enemy(x, y, EnemyType.IceShard, level, boss);
            wolf.FrostbiteResistance = 9999; // Praktisch immun gegen Kälte
            return wolf;
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Frostbiss-Geist (Frostbite Wraith) - Kein HP-Schaden, +4 Kälte-Stacks, schwebt
    public sealed class FrostbiteArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Frostbite;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 20; // Mittlere Häufigkeit, taktische Bedrohung
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var wraith = new Enemy(x, y, EnemyType.Frostbite, level, boss);
            wraith.FrostbiteResistance = 9999; // Immun gegen Kälte
            return wraith;
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Blizzard-Schamane (Snowblind) - 8 Resistenz, drain Lagerfeuer alle 3 Züge
    public sealed class SnowblindArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Snowblind;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 10; // Rare, hohe Priorität
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var shaman = new Enemy(x, y, EnemyType.Snowblind, level, boss);
            shaman.FrostbiteResistance = 8; // Stirbt bei 8 Frostbite Stacks
            shaman.CampfireDrainCounter = 0; // Startet bei 0, drain bei 3
            return shaman;
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Permafrost-Golem (GlacialSentinel) - Immun, immobile, Kälte-Aura
    public sealed class GlacialSentinelArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.GlacialSentinel;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 8; // Rare, sehr gefährlich
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var golem = new Enemy(x, y, EnemyType.GlacialSentinel, level, boss);
            golem.FrostbiteResistance = 9999; // Immun gegen Kälte
            golem.GolemAuraMoveCounter = 0; // Bewegt sich jeden 3. Zug
            return golem;
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Kälteschatten (Chill Touch Stalker / PermafrostLich) - Bewegt sich nicht, Achsen-Projektion
    public sealed class PermafrostLichArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.PermafrostLich;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 7; // Rare, taktische Herausforderung
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var stalker = new Enemy(x, y, EnemyType.PermafrostLich, level, boss);
            stalker.FrostbiteResistance = 9999; // Immun gegen Kälte
            return stalker;
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Frostbiss-Mimic - Getarnt als Lagerfeuer, +5 Stacks beim Entdecken
    public sealed class FrostbiteMimicArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.FrostbiteMimic;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 5; // Sehr rare, gefährliche Überraschung
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var mimic = new Enemy(x, y, EnemyType.FrostbiteMimic, level, boss);
            mimic.FrostbiteResistance = 5; // Schwach aber überraschend
            mimic.IsMimicRevealed = false; // Startet getarnt
            return mimic;
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Das Gefrorene Herz (Ice Dragon) - Boss von Akt 4, Phase 1 & 2
    public sealed class IceDragonArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.IceDragon;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 0; // Nur durch Boss-Spawn
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel() + 5; // +5 Level Boost

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var dragon = new Enemy(x, y, EnemyType.IceDragon, level, true);
            dragon.FrostbiteResistance = 9999; // Boss ist immun gegen Kälte
            dragon.IsPhase2 = false; // Phase 2 wird bei 50% HP aktiviert
            return dragon;
        }

        public bool IsBossEligible(GameContext ctx) => true;
    }
}
