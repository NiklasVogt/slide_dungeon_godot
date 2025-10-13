using System;
using System.Collections.Generic;
using System.Numerics;

namespace Dungeon2048.Core.Services
{
    public struct AttackEvent
    {
        public string Attacker;
        public string Target;
        public Godot.Vector2I Dir;
        public AttackEvent(string a, string t, Godot.Vector2I d){ Attacker=a; Target=t; Dir=d; }
    }

    public sealed class EventBus
    {
        public readonly List<AttackEvent> Attacks = new();
        public void Add(AttackEvent e) => Attacks.Add(e);
        public void Clear() => Attacks.Clear();
    }
}
