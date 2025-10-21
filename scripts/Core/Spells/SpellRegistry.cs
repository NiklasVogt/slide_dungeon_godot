using System;
using System.Collections.Generic;
using Dungeon2048.Core.Services;

namespace Dungeon2048.Core.Spells
{
    public static class SpellRegistry
    {
        private static readonly Dictionary<SpellType, ISpellBehavior> _behaviors = new();

        public static void Register(SpellType type, ISpellBehavior behavior) => _behaviors[type] = behavior;

        public static ISpellBehavior Get(SpellType type)
        {
            if (_behaviors.TryGetValue(type, out var b)) return b;
            throw new InvalidOperationException($"No behavior for spell {type}");
        }

        public static void UseSpellSlot(Entities.Player player, int index, GameContext ctx)
        {
            if (index < 0 || index >= player.Spells.Count) return;
            var s = player.Spells[index];
            var behavior = Get(s.Type);
            behavior.Cast(player, ctx);
            if (!behavior.IsPermanent) player.Spells.RemoveAt(index);
        }
    }

    // Default behaviors wired like current logic
    public sealed class FireballBehavior : ISpellBehavior
    {
        public string Id => "spell.fireball";
        public string Name => "Feuerball";
        public string Description => "Trifft Kreuz-Pattern";
        public bool IsPermanent => false;
        public void Cast(Entities.Player player, GameContext ctx)
        {
            int dmg = 8 + (int)(player.Level / 2.0);
            int px = player.X; int py = player.Y;
            var toKill = new List<Entities.Enemy>();
            foreach (var e in ctx.Enemies)
            {
                if (e.X == px || e.Y == py)
                {
                    e.Hp -= dmg;
                    if (e.Hp <= 0) toKill.Add(e);
                }
            }
            foreach (var ek in toKill)
            {
                ctx.RegisterPlayerKill(ek);
                ctx.Enemies.Remove(ek);
                player.GainExperience(ek.XpReward);
            }
        }
    }

    public sealed class HealBehavior : ISpellBehavior
    {
        public string Id => "spell.heal";
        public string Name => "Heilung";
        public string Description => "Heilt 50% HP";
        public bool IsPermanent => false;
        
        public void Cast(Entities.Player player, GameContext ctx)
        {
            int heal = (int)Math.Round(player.MaxHp * 0.5);
            
            // NEU: Hex Curse Mechanik
            if (ctx.IsHexCursed)
            {
                // Invertiert: Heilung wird zu Schaden
                player.Hp -= heal;
                Godot.GD.Print($"🔮 HEX-FLUCH! Heilung wird zu {heal} Schaden!");
            }
            else
            {
                // Normal: Heilen
                player.Hp = Math.Min(player.MaxHp, player.Hp + heal);
                Godot.GD.Print($"✨ Geheilt: +{heal} HP");
            }
        }
    }

    public sealed class FreezeBehavior : ISpellBehavior
    {
        public string Id => "spell.freeze";
        public string Name => "Frost";
        public string Description => "Friert Gegner für 1 Zug ein";
        public bool IsPermanent => false;
        public void Cast(Entities.Player player, GameContext ctx)
        {
            ctx.EnemiesFrozen = true;
        }
    }

    public sealed class LightningBehavior : ISpellBehavior
    {
        public string Id => "spell.lightning";
        public string Name => "Blitzschlag";
        public string Description => "Tötet alle Goblins";
        public bool IsPermanent => false;
        public void Cast(Entities.Player player, GameContext ctx)
        {
            var killed = new List<Entities.Enemy>();
            foreach (var e in ctx.Enemies.ToArray())
                if (e.Type == Entities.EnemyType.Goblin) killed.Add(e);

            foreach (var e in killed)
            {
                ctx.RegisterPlayerKill(e);
                ctx.Enemies.Remove(e);
                player.GainExperience(e.XpReward);
            }
        }
    }

    public sealed class TeleportBehavior : ISpellBehavior
    {
        public string Id => "spell.teleport";
        public string Name => "Teleport";
        public string Description => "Zufällige Position";
        public bool IsPermanent => false;
        public void Cast(Entities.Player player, GameContext ctx)
        {
            var p = ctx.RandomFreeCell(ignorePlayer: true);
            player.X = p.X; player.Y = p.Y;
        }
    }
}
