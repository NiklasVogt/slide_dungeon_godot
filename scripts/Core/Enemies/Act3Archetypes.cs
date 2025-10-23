// scripts/Core/Enemies/Act3Archetypes.cs
using System;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Services;

namespace Dungeon2048.Core.Enemies
{
    // Feuer-Elementar - Hinterlässt Feuer auf vorheriger Position
    public sealed class FireElementalArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.FireElemental;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 35; // Häufigster Standard-Gegner in Akt 3
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.FireElemental, level, boss);
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Moloch (Lava-Dämon) - Kann durch Feuer gehen, heilt auf Feuer
    public sealed class MolochArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Moloch;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 30; // Häufig, hohe HP
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var moloch = new Enemy(x, y, EnemyType.Moloch, level, boss);
            moloch.StandingOnFire = false;
            return moloch;
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Schmied-Golem - Sehr langsam (jeden 3. Zug), sehr hoher Schaden
    public sealed class SchmiedGolemArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.SchmiedGolem;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 25; // Mittlere Häufigkeit, sehr gefährlich
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var golem = new Enemy(x, y, EnemyType.SchmiedGolem, level, boss);
            golem.GolemMoveCounter = 0; // Startet bei 0, bewegt sich bei 3
            return golem;
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Pyromaniac - Explodiert beim Tod (Schaden an alles in 1-Tile-Radius)
    public sealed class PyromaniacArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Pyromaniac;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 10; // Rare, taktisch nutzbar
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.Pyromaniac, level, boss);
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Obsidian Warrior - Immun gegen Feuer, wird stärker durch Feuer-Schaden
    public sealed class ObsidianWarriorArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.ObsidianWarrior;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 8; // Rare, sehr gefährlich
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.ObsidianWarrior, level, boss);
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Forge Master - Buffet benachbarte Gegner anstatt zu schaden
    public sealed class ForgeMasterArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.ForgeMaster;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 7; // Rare, hohe Priorität zu töten
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.ForgeMaster, level, boss);
        }

        public bool IsBossEligible(GameContext ctx) => false;
    }

    // Fire Giant - Boss von Akt 3
    public sealed class FireGiantArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.FireGiant;

        public int CalcSpawnWeight(GameContext ctx)
        {
            return 0; // Nur durch Boss-Spawn
        }

        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel() + 4; // +4 Level Boost

        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var giant = new Enemy(x, y, EnemyType.FireGiant, level, true);
            giant.IsPhase2 = false; // Phase 2 wird bei 50% HP aktiviert
            return giant;
        }

        public bool IsBossEligible(GameContext ctx) => true;
    }
}
