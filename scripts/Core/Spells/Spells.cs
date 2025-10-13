using System;

namespace Dungeon2048.Core.Spells
{
    public enum SpellType { Fireball, Heal, Freeze, Lightning, Teleport }

    // Backwards-compatible Wrapper für vorhandene Slots
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

        // Für alte Aufrufer; echte Wirkung über ISpellBehavior in Registry
        public virtual void Cast(Entities.Player player) { }
    }

    public sealed class FireballSpell : Spell
    {
        public FireballSpell() : base(SpellType.Fireball, "Feuerball", "Trifft Kreuz-Pattern") { }
    }
    public sealed class HealSpell : Spell
    {
        public HealSpell() : base(SpellType.Heal, "Heilung", "Heilt 50% HP") { }
        public override void Cast(Entities.Player player)
        {
            int heal = (int)Math.Round(player.MaxHp * 0.5);
            player.Hp = Math.Min(player.MaxHp, player.Hp + heal);
        }
    }
    public sealed class FreezeSpell : Spell
    {
        public FreezeSpell() : base(SpellType.Freeze, "Frost", "Friert Gegner ein") { }
    }
    public sealed class LightningSpell : Spell
    {
        public LightningSpell() : base(SpellType.Lightning, "Blitzschlag", "Tötet alle Goblins") { }
    }
    public sealed class TeleportSpell : Spell
    {
        public TeleportSpell() : base(SpellType.Teleport, "Teleport", "Zufällige Position") { }
    }

    public static class SpellFactory
    {
        public static Spell CreateRandom(int playerLevel, Random rng)
        {
            var available = new System.Collections.Generic.List<SpellType> { SpellType.Fireball, SpellType.Heal };
            if (playerLevel >= 3) available.Add(SpellType.Freeze);
            if (playerLevel >= 5) available.Add(SpellType.Lightning);
            if (playerLevel >= 7) available.Add(SpellType.Teleport);
            var sel = available[rng.Next(available.Count)];
            return sel switch
            {
                SpellType.Fireball => new FireballSpell(),
                SpellType.Heal => new HealSpell(),
                SpellType.Freeze => new FreezeSpell(),
                SpellType.Lightning => new LightningSpell(),
                SpellType.Teleport => new TeleportSpell(),
                _ => new HealSpell()
            };
        }
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
}
