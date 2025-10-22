// scripts/Nodes/GameBoardInput.cs
using Godot;
using Dungeon2048.Core.Services;

namespace Dungeon2048.Nodes
{
    /// <summary>
    /// Verwaltet Input-Handling und Debug-Commands
    /// </summary>
    public sealed class GameBoardInput
    {
        private readonly Node2D _gameBoard;
        private readonly GameContext _ctx;

        public GameBoardInput(Node2D gameBoard, GameContext ctx)
        {
            _gameBoard = gameBoard;
            _ctx = ctx;
        }

        /// <summary>
        /// Gibt Bewegungsrichtung basierend auf Keycode zurück
        /// </summary>
        public Vector2I GetDirectionFromKey(Key keycode)
        {
            return keycode switch
            {
                Key.Up => new Vector2I(0, -1),
                Key.Down => new Vector2I(0, 1),
                Key.Left => new Vector2I(-1, 0),
                Key.Right => new Vector2I(1, 0),
                _ => Vector2I.Zero
            };
        }

        #if DEBUG
        /// <summary>
        /// Verarbeitet Debug-Commands (nur in Debug-Build)
        /// </summary>
        public bool HandleDebugCommand(Key keycode)
        {
            switch (keycode)
            {
                case Key.F1:
                    Core.Debug.DebugCommands.SpawnTestEnemies(_ctx);
                    return true;
                    
                case Key.F2:
                    Core.Debug.DebugCommands.PrintBiomeInfo(_ctx);
                    return false; // Kein Redraw nötig
                    
                case Key.F3:
                    Core.Debug.DebugCommands.SpawnBoss(_ctx);
                    return true;
                    
                case Key.F4:
                    Core.Debug.DebugCommands.JumpToLevel(_ctx, 11);
                    return true;
                    
                case Key.F5:
                    Core.Debug.DebugCommands.JumpToLevel(_ctx, 20);
                    return true;
                    
                case Key.F6:
                    Core.Debug.DebugCommands.HealPlayer(_ctx);
                    return true;
                    
                case Key.F7:
                    Core.Debug.DebugCommands.SkipLevel(_ctx);
                    return true;
                    
                case Key.F8:
                    Core.Debug.DebugCommands.ToggleHexCurse(_ctx);
                    return true;
                    
                case Key.F9:
                    Core.Debug.DebugCommands.SpawnTeleporters(_ctx);
                    return true;
                    
                default:
                    return false;
            }
        }
        #endif
    }
}