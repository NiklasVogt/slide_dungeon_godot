using System;
using System.Collections.Generic;
using Godot;

namespace Dungeon2048.Core
{
    public abstract class EntityBase
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public int Hp;
        public int Atk;

        protected EntityBase(int x, int y, int hp, int atk)
        {
            X = x; Y = y; Hp = hp; Atk = atk;
        }
    }

    public enum EnemyType { Goblin, Orc, Dragon, Boss }
    public enum LevelType { Survival, Elimination, Boss }
    public enum SpellType { Fireball, Heal, Freeze, Lightning, Teleport }

    public sealed class Player : EntityBase
    {
        public int Level = 1;
        public int Experience = 0;
        public int MaxHp = 20;
        public const int BaseHp = 20;
        public const int BaseAtk = 5;
        public const int MaxSpells = 3;
        public readonly List<Spell> Spells = new();

        public Player(int x, int y) : base(x, y, BaseHp, BaseAtk) {}

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

        public bool AddSpell(Spell spell)
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

        public static int CalculateXpReward(EnemyType type, int enemyLevel, bool isBoss)
        {
            int baseXp = type switch {
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

    public abstract class Spell
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public SpellType Type { get; }
        public string Name { get; }
        public string Description { get; }
        public bool IsPermanent { get; }

        protected Spell(SpellType type, string name, string description, bool permanent=false)
        {
            Type = type; Name = name; Description = description; IsPermanent = permanent;
        }

        public abstract void Cast(Player player);

        public static Spell CreateRandom(int playerLevel, Random rng)
        {
            var available = new List<SpellType> { SpellType.Fireball, SpellType.Heal };
            if (playerLevel >= 3) available.Add(SpellType.Freeze);
            if (playerLevel >= 5) available.Add(SpellType.Lightning);
            if (playerLevel >= 7) available.Add(SpellType.Teleport);
            var sel = available[rng.Next(available.Count)];
            return sel switch {
                SpellType.Fireball => new FireballSpell(),
                SpellType.Heal => new HealSpell(),
                SpellType.Freeze => new FreezeSpell(),
                SpellType.Lightning => new LightningSpell(),
                SpellType.Teleport => new TeleportSpell(),
                _ => new HealSpell()
            };
        }
    }

    public sealed class FireballSpell : Spell
    {
        public FireballSpell() : base(SpellType.Fireball, "Feuerball", "Trifft Kreuz-Pattern") {}
        public override void Cast(Player player) { /* Effekt in GameBoard angewandt, falls genutzt */ }
    }
    public sealed class HealSpell : Spell
    {
        public HealSpell() : base(SpellType.Heal, "Heilung", "Heilt 50% HP") {}
        public override void Cast(Player player)
        {
            int heal = (int)Math.Round(player.MaxHp * 0.5);
            player.Hp = Math.Min(player.MaxHp, player.Hp + heal);
        }
    }
    public sealed class FreezeSpell : Spell
    {
        public FreezeSpell() : base(SpellType.Freeze, "Frost", "Friert Gegner ein") {}
        public override void Cast(Player player) {}
    }
    public sealed class LightningSpell : Spell
    {
        public LightningSpell() : base(SpellType.Lightning, "Blitzschlag", "Tötet alle Goblins") {}
        public override void Cast(Player player) {}
    }
    public sealed class TeleportSpell : Spell
    {
        public TeleportSpell() : base(SpellType.Teleport, "Teleport", "Zufällige Position") {}
        public override void Cast(Player player) {}
    }

    public sealed class SpellDrop
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public Spell Spell;
        public int HitCount = 0;
        public const int MaxHits = 3;
        public bool IsDestroyed => HitCount >= MaxHits;
        public SpellDrop(int x, int y, Spell spell) { X=x; Y=y; Spell=spell; }
    }

    public sealed class Enemy : EntityBase
    {
        public EnemyType Type;
        public int EnemyLevel;
        public bool IsBoss;

        public Enemy(int x, int y, EnemyType type, int enemyLevel, bool isBoss=false)
            : base(x, y, CalcHp(type, enemyLevel, isBoss), CalcAtk(type, enemyLevel, isBoss))
        { Type=type; EnemyLevel=enemyLevel; IsBoss=isBoss; }

        static int CalcHp(EnemyType type, int level, bool isBoss)
        {
            int baseHp = type switch {
                EnemyType.Goblin => 8,
                EnemyType.Orc => 15,
                EnemyType.Dragon => 25,
                EnemyType.Boss => 50,
                _ => 10
            };
            double levelMul = Math.Pow(1 + level * 0.4, 1.2);
            double bossMul = isBoss ? 2.5 : 1.0;
            return (int)Math.Round(baseHp * levelMul * bossMul);
        }

        static int CalcAtk(EnemyType type, int level, bool isBoss)
        {
            int baseAtk = type switch {
                EnemyType.Goblin => 2,
                EnemyType.Orc => 4,
                EnemyType.Dragon => 7,
                EnemyType.Boss => 12,
                _ => 1
            };
            double mult = type switch {
                EnemyType.Goblin => 1.2,
                EnemyType.Orc => 1.8,
                EnemyType.Dragon => 2.5,
                EnemyType.Boss => 3.0,
                _ => 1.0
            };
            double levelBonus = level * mult;
            double bossMul = isBoss ? 1.8 : 1.0;
            return (int)Math.Round((baseAtk + levelBonus) * bossMul);
        }

        public int XpReward => Player.CalculateXpReward(Type, EnemyLevel, IsBoss);
        public bool IsElite => IsBoss || EnemyLevel >= 5;
        public string DisplayName => IsBoss ? "Boss"
            : Type switch { EnemyType.Goblin => "Goblin", EnemyType.Orc => "Orc", EnemyType.Dragon => "Dragon", _=>"Enemy" };
    }

    public sealed class Stone
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public int X; public int Y;
        public int HitCount = 0;
        public const int MaxHits = 3;
        public bool IsDestroyed => HitCount >= MaxHits;
        public Stone(int x, int y) { X=x; Y=y; }
    }

    public sealed class Door
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public int X; public int Y;
        public bool IsActive = false;
        public Door(int x, int y) { X=x; Y=y; }
    }
}
