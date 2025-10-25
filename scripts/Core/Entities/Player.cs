// scripts/Core/Entities/Player.cs
using System;
using System.Collections.Generic;
using Godot;

namespace Dungeon2048.Core.Entities
{
    public sealed class Player : EntityBase
    {
        public int Level = 1;
        public int Experience = 0;
        public int MaxHp = 20;

        // Akt 3: Burning Status
        public int BurningStacks = 0;

        public const int BaseHp = 20;
        public const int BaseAtk = 5;
        public const int MaxSpells = 3;

        public readonly List<Spells.Spell> Spells = new();

        public Player(int x, int y) : base(x, y, BaseHp, BaseAtk) { }

        // Langsamer Level-Up
        public int ExperienceToNextLevel => (int)Math.Round(75 * Math.Pow(Level, 1.7));
        
        // Langsamer Stats-Skalierung
        public int CalculatedMaxHp => BaseHp + (Level * 6);
        public int CalculatedAtk => BaseAtk + ((Level - 1) * 1);
        
        public bool CanLevelUp() => Experience >= ExperienceToNextLevel;

        public void LevelUp()
        {
            if (!CanLevelUp()) return;
            Experience -= ExperienceToNextLevel;
            Level++;
            MaxHp = CalculatedMaxHp;
            Hp = MaxHp;
            Atk = CalculatedAtk;
            GD.Print($"Level Up -> L{Level} HP {Hp}/{MaxHp} ATK {Atk}");
        }

        public void GainExperience(int xp)
        {
            Experience += xp;
            while (CanLevelUp()) LevelUp();
        }

        public bool AddSpell(Spells.Spell spell)
        {
            if (Spells.Count >= MaxSpells) return false;
            Spells.Add(spell);
            return true;
        }

        public bool UseSpell(int index)
        {
            if (index < 0 || index >= Spells.Count) return false;
            var s = Spells[index];
            s.Cast(this);
            if (!s.IsPermanent) Spells.RemoveAt(index);
            return true;
        }

        // Reduzierte XP Rewards
        public static int CalculateXpReward(EnemyType type, int enemyLevel, bool isBoss)
        {
            int baseXp = type switch
            {
                // Akt 1 - Reduziert
                EnemyType.Goblin       => 10,
                EnemyType.Skeleton     => 12,
                EnemyType.Rat          => 5,
                EnemyType.Necrophage   => 30,
                EnemyType.Mimic        => 40,
                EnemyType.GoblinKing   => 120,
                
                // Akt 2
                EnemyType.Orc          => 25,
                EnemyType.Kultist      => 20,
                EnemyType.Gargoyle     => 28,
                EnemyType.SoulLeech    => 35,
                EnemyType.MirrorKnight => 45,
                EnemyType.HexWitch     => 38,
                EnemyType.LichMage     => 150,
                
                // Akt 3
                EnemyType.FireElemental     => 22,
                EnemyType.Moloch            => 30,
                EnemyType.SchmiedGolem      => 35,
                EnemyType.Pyromaniac        => 25,
                EnemyType.ObsidianWarrior   => 32,
                EnemyType.ForgeMaster       => 40,
                EnemyType.FireGiant         => 180,
                
                // Akt 4
                EnemyType.FrostGoblin       => 15,
                EnemyType.Yeti              => 40,
                EnemyType.IceShard          => 12,
                EnemyType.Frostbite         => 28,
                EnemyType.Snowblind         => 35,
                EnemyType.GlacialSentinel   => 38,
                EnemyType.PermafrostLich    => 45,
                EnemyType.IceDragon         => 200,
                
                // Akt 5
                EnemyType.Dragon            => 50,
                EnemyType.VoidSpawn         => 30,
                EnemyType.ChaosKnight       => 35,
                EnemyType.Doppelganger      => 40,
                EnemyType.Parasite          => 15,
                EnemyType.RealityBender     => 38,
                EnemyType.SoulEater         => 45,
                EnemyType.Paradox           => 50,
                EnemyType.DungeonLord       => 300,
                
                // Legacy
                EnemyType.Boss              => 100,
                EnemyType.Masochist         => 35,
                EnemyType.Thorns            => 28,
                _ => 8
            };
            
            double levelMul = 1 + enemyLevel * 0.25;
            double bossMul = isBoss ? 2.5 : 1.0;
            return (int)Math.Round(baseXp * levelMul * bossMul);
        }
    }
}