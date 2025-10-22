// scripts/Nodes/GameBoardLayout.cs
using Godot;
using Dungeon2048.Core.Services;

namespace Dungeon2048.Nodes
{
    /// <summary>
    /// Verwaltet Layout, Sizing und Positioning des GameBoards
    /// </summary>
    public sealed class GameBoardLayout
    {
        private readonly Node2D _gameBoard;
        private float _tileSize = 96f;
        private const float Padding = 12f;
        private float _reservedUiWidth = 360f;

        public float TileSize => _tileSize;
        public float UiWidth => _reservedUiWidth;

        public GameBoardLayout(Node2D gameBoard)
        {
            _gameBoard = gameBoard;
        }

        public void RecomputeTileSize()
        {
            var vp = _gameBoard.GetViewportRect().Size;
            float uiWidth = ComputeReservedUiWidth(vp.X);
            _reservedUiWidth = uiWidth;
            
            float usableW = Mathf.Max(0, vp.X - uiWidth - Padding * 2f);
            float usableH = Mathf.Max(0, vp.Y - Padding * 2f);
            
            float cellW = usableW / GameContext.GridSize;
            float cellH = usableH / GameContext.GridSize;
            
            _tileSize = Mathf.Floor(Mathf.Min(Mathf.Min(cellW, cellH), 200f));
        }

        private float ComputeReservedUiWidth(float viewportWidth)
            => Mathf.Clamp(viewportWidth * 0.28f, 320f, 520f);

        /// <summary>
        /// Konvertiert Grid-Position zu lokaler Screen-Position
        /// </summary>
        public Vector2 MapToLocal(Vector2I gridPos)
            => new Vector2(Padding + gridPos.X * _tileSize, Padding + gridPos.Y * _tileSize);

        /// <summary>
        /// Positioniert die UI am rechten Rand
        /// </summary>
        public void PositionUI(Control ui)
        {
            if (ui == null) return;

            ui.AnchorLeft = 1f;
            ui.AnchorRight = 1f;
            ui.AnchorTop = 0f;
            ui.AnchorBottom = 0f;
            ui.OffsetRight = -Padding;
            ui.OffsetTop = Padding;
            ui.OffsetBottom = 0f;
            ui.OffsetLeft = -_reservedUiWidth - Padding;
            
            if (ui is UI hud) 
                hud.SetDesiredWidth(_reservedUiWidth);
        }

        /// <summary>
        /// Gibt Origin-Position für Grid-Drawing zurück
        /// </summary>
        public Vector2 GetGridOrigin() => new Vector2(Padding, Padding);

        /// <summary>
        /// Gibt Board-Größe zurück
        /// </summary>
        public Vector2 GetBoardSize() 
            => new Vector2(GameContext.GridSize * _tileSize, GameContext.GridSize * _tileSize);
    }
}