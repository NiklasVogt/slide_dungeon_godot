using System.Collections.Generic;
using Godot;

namespace Dungeon2048.Core.Services
{
    public struct AttackEvent
    {
        public string Attacker;
        public string Target;
        public Vector2I Dir;

        public AttackEvent(string a, string t, Vector2I d)
        {
            Attacker = a;
            Target = t;
            Dir = d;
        }
    }

    public sealed class EventBus
    {
        public readonly List<AttackEvent> Attacks = new();

        // Öffentliche, klar benannte API (so wie in MovementPipeline genutzt)
        public void AddAttackEvent(AttackEvent e)
        {
            Attacks.Add(e);
        }

        public void Clear()
        {
            Attacks.Clear();
        }
    }
}
