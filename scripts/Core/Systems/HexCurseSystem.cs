// scripts/Core/Systems/HexCurseSystem.cs
using Godot;

namespace Dungeon2048.Core.Systems
{
    /// <summary>
    /// Manages Hex Curse mechanics from Hex Witch
    /// Hex Curse inverts healing for a duration
    /// </summary>
    public sealed class HexCurseSystem
    {
        private int _hexCurseTurnsRemaining = 0;

        public int HexCurseTurnsRemaining => _hexCurseTurnsRemaining;
        public bool IsHexCursed => _hexCurseTurnsRemaining > 0;

        /// <summary>
        /// Applies hex curse for specified duration
        /// </summary>
        public void ApplyHexCurse(int duration)
        {
            _hexCurseTurnsRemaining = duration;
            GD.Print("🔮 Die Hex-Hexe verflucht dich! Heilung ist invertiert!");
        }

        /// <summary>
        /// Decrements curse duration each turn
        /// </summary>
        public void ProcessCurse()
        {
            if (_hexCurseTurnsRemaining > 0)
            {
                _hexCurseTurnsRemaining--;
                if (_hexCurseTurnsRemaining == 0)
                {
                    GD.Print("✨ Der Hex-Fluch wurde gebrochen!");
                }
            }
        }

        /// <summary>
        /// Clears the curse (used when advancing to next level)
        /// </summary>
        public void ClearCurse()
        {
            _hexCurseTurnsRemaining = 0;
        }
    }
}
