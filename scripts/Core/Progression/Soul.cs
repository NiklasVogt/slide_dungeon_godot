// scripts/Core/Progression/Soul.cs
namespace Dungeon2048.Core.Progression
{
    public static class SoulCurrency
    {
        public const string SOUL_SAVE_KEY = "dungeon2048_souls";
        
        // Seelen-Belohnungen pro Enemy-Typ
        public static int GetSoulReward(Entities.EnemyType type, int level, bool isBoss)
        {
            int baseSouls = type switch
            {
                // Akt 1
                Entities.EnemyType.Goblin       => 1,
                Entities.EnemyType.Skeleton     => 1,
                Entities.EnemyType.Rat          => 1,
                Entities.EnemyType.Necrophage   => 3,
                Entities.EnemyType.Mimic        => 5,
                Entities.EnemyType.GoblinKing   => 15,
                
                // Akt 2
                Entities.EnemyType.Orc          => 2,
                Entities.EnemyType.Kultist      => 2,
                Entities.EnemyType.Gargoyle     => 3,
                Entities.EnemyType.SoulLeech    => 4,
                Entities.EnemyType.MirrorKnight => 5,
                Entities.EnemyType.HexWitch     => 4,
                Entities.EnemyType.LichMage     => 20,
                
                // Akt 3
                Entities.EnemyType.FireElemental     => 2,
                Entities.EnemyType.Moloch            => 3,
                Entities.EnemyType.SchmiedGolem      => 4,
                Entities.EnemyType.Pyromaniac        => 3,
                Entities.EnemyType.ObsidianWarrior   => 4,
                Entities.EnemyType.ForgeMaster       => 5,
                Entities.EnemyType.FireGiant         => 25,
                
                // Akt 4
                Entities.EnemyType.FrostGoblin       => 2,
                Entities.EnemyType.Yeti              => 4,
                Entities.EnemyType.IceShard          => 1,
                Entities.EnemyType.Frostbite         => 3,
                Entities.EnemyType.Snowblind         => 4,
                Entities.EnemyType.GlacialSentinel   => 5,
                Entities.EnemyType.PermafrostLich    => 6,
                Entities.EnemyType.IceDragon         => 30,
                
                // Akt 5
                Entities.EnemyType.Dragon            => 5,
                Entities.EnemyType.VoidSpawn         => 3,
                Entities.EnemyType.ChaosKnight       => 4,
                Entities.EnemyType.Doppelganger      => 5,
                Entities.EnemyType.Parasite          => 2,
                Entities.EnemyType.RealityBender     => 5,
                Entities.EnemyType.SoulEater         => 6,
                Entities.EnemyType.Paradox           => 7,
                Entities.EnemyType.DungeonLord       => 50,
                
                // Legacy
                Entities.EnemyType.Boss              => 10,
                Entities.EnemyType.Masochist         => 3,
                Entities.EnemyType.Thorns            => 3,
                _ => 1
            };
            
            // Boss Multiplier
            if (isBoss) baseSouls = (int)(baseSouls * 2.0);
            
            // Level Bonus (weniger als XP)
            int levelBonus = level / 2;
            
            return baseSouls + levelBonus;
        }
        
        // Bonus Seelen für Objectives
        public static int GetObjectiveCompletionBonus(Objectives.LevelType type)
        {
            return type switch
            {
                Objectives.LevelType.Survival => 2,
                Objectives.LevelType.Elimination => 3,
                Objectives.LevelType.Boss => 10,
                _ => 1
            };
        }
        
        // Bonus für Level-Completion
        public static int GetLevelCompletionBonus(int level)
        {
            return level; // 1 Seele pro Level
        }
    }
}