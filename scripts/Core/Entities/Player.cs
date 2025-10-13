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

        public const int BaseHp = 20;
        public const int BaseAtk = 5;
        public const int MaxSpells = 3;

        public readonly List<Spells.Spell> Spells = new();

        public Player(int x, int y) : base(x, y, BaseHp, BaseAtk) { }

        public int ExperienceToNextLevel => (int)Math.Round(50 * Math.Pow(Level, 1.5));
        public int CalculatedMaxHp => BaseHp + (Level * 8);
        public int CalculatedAtk => BaseAtk + ((Level - 1) * 2);
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

        // Fix: Verweis auf EnemyType aus Dungeon2048.Core.Entities statt Dungeon2048.Core.Enemies
        public static int CalculateXpReward(EnemyType type, int enemyLevel, bool isBoss)
        {
            int baseXp = type switch
            {
                EnemyType.Goblin => 15,
                EnemyType.Orc => 35,
                EnemyType.Dragon => 75,
                EnemyType.Boss => 150,
                _ => 10
            };
            double levelMul = 1 + enemyLevel * 0.3;
            double bossMul = isBoss ? 3.0 : 1.0;
            return (int)Math.Round(baseXp * levelMul * bossMul);
        }
    }
}
