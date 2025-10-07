using System.Threading.Tasks;
using Godot;
using Dungeon2048.Core;

namespace Dungeon2048.Nodes
{
    public partial class GameBoard : Node2D
    {
        [Export] public TileMap Grid;
        [Export] public Node2D EntitiesNode;
        [Export] public Control UI;

        private GameState _gs;
        private bool _animating = false;

        // Dynamische Kachelgröße (aus Viewport berechnet)
        private float _tileSize = 96f;

        // Außenabstand
        private const float Padding = 12f;
        // Reservierte Breite für das UI (wird dynamisch gerechnet)
        private float _reservedUiWidth = 360f;

        public override void _Ready()
        {
            _gs = new GameState();
            AddChild(_gs);

            Grid ??= GetNode<TileMap>("Grid");
            EntitiesNode ??= GetNode<Node2D>("Entities");
            UI ??= GetNode<Control>("CanvasLayer/UI");

            // Viewport-Resize beobachten
            GetViewport().SizeChanged += OnViewportSizeChanged;

            RecomputeTileSize();
            QueueRedraw();
            SyncScene();
        }

        public override void _ExitTree()
        {
            if (GetViewport() != null)
                GetViewport().SizeChanged -= OnViewportSizeChanged;
        }

        private void OnViewportSizeChanged()
        {
            RecomputeTileSize();
            SyncScene();
        }

        private void RecomputeTileSize()
        {
            var vp = GetViewportRect().Size;
            float uiWidth = ComputeReservedUiWidth(vp.X);
            _reservedUiWidth = uiWidth;

            float usableW = Mathf.Max(0, vp.X - uiWidth - Padding * 2f);
            float usableH = Mathf.Max(0, vp.Y - Padding * 2f);

            float cellW = usableW / GameState.GridSize;
            float cellH = usableH / GameState.GridSize;

            // Dreiwerte-Min korrekt verschachtelt
            _tileSize = Mathf.Floor(Mathf.Min(Mathf.Min(cellW, cellH), 200f));
        }

        private float ComputeReservedUiWidth(float viewportWidth)
        {
            // 28% der Breite, min 320, max 520
            return Mathf.Clamp(viewportWidth * 0.28f, 320f, 520f);
        }

        public override void _Draw()
        {
            var size = GameState.GridSize;
            var ts = _tileSize;

            var origin = new Vector2(Padding, Padding);
            var boardSize = new Vector2(size * ts, size * ts);

            var bg = new Color(0.12f, 0.12f, 0.12f, 1f);
            DrawRect(new Rect2(origin, boardSize), bg);

            var col = new Color(0.25f, 0.25f, 0.25f, 1f);
            for (int i = 0; i <= size; i++)
            {
                DrawLine(new Vector2(origin.X, origin.Y + i * ts), new Vector2(origin.X + size * ts, origin.Y + i * ts), col, 2);
                DrawLine(new Vector2(origin.X + i * ts, origin.Y), new Vector2(origin.X + i * ts, origin.Y + size * ts), col, 2);
            }
        }

        public override async void _UnhandledInput(InputEvent @event)
        {
            if (_animating) return;

            if (@event is InputEventKey key && key.Pressed && !key.Echo)
            {
                var dir = Vector2I.Zero;
                if (key.Keycode == Key.Up) dir = new Vector2I(0, -1);
                if (key.Keycode == Key.Down) dir = new Vector2I(0, 1);
                if (key.Keycode == Key.Left) dir = new Vector2I(-1, 0);
                if (key.Keycode == Key.Right) dir = new Vector2I(1, 0);
                if (dir != Vector2I.Zero)
                    await OnArrowMove(dir);
            }
        }

        private async Task OnArrowMove(Vector2I dir)
        {
            if (_animating) return;
            _animating = true;

            _gs.RegisterSwipe();
            if (UI is UI uiA) uiA.UpdateFromState(_gs);

            await Movement.MoveEntitiesWithImmediateCollision(_gs, dir.X, dir.Y, ApplyState);
            await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);
            _gs.SpawnEnemies();

            SyncScene();
            _animating = false;
        }

        private void ApplyState(System.Action reducer)
        {
            reducer?.Invoke();
            if (UI is UI ui) ui.UpdateFromState(_gs);
        }

        private void SyncScene()
        {
            EnsureNodes();
            if (UI is UI ui) ui.UpdateFromState(_gs);

            // UI rechts andocken und Breite setzen
            float uiWidth = _reservedUiWidth;
            if (UI != null)
            {
                UI.AnchorLeft = 1f;
                UI.AnchorRight = 1f;
                UI.AnchorTop = 0f;
                UI.AnchorBottom = 0f;

                UI.OffsetRight = -Padding;
                UI.OffsetTop = Padding;
                UI.OffsetBottom = 0f;
                UI.OffsetLeft = -uiWidth - Padding;

                if (UI is UI hud) hud.SetDesiredWidth(uiWidth);
            }

            QueueRedraw();
        }

        private void EnsureNodes()
        {
            // Z-Order: erst Spells/Steine/Tür/Gegner, Spieler zuletzt oder mit höherem ZIndex
            // Vorher vorhandene dynamische Kinder entfernen
            for (int i = EntitiesNode.GetChildCount() - 1; i >= 0; i--)
            {
                var n = EntitiesNode.GetChild(i);
                var nm = n.Name.ToString();
                if (nm.StartsWith("Enemy_") || nm.StartsWith("Stone_") || nm.StartsWith("Spell_") || nm == "Door" || nm == "Player")
                {
                    EntitiesNode.RemoveChild(n);
                    n.QueueFree();
                }
            }

            foreach (var s in _gs.Stones)
                EnsureEntityNode($"Stone_{s.Id}", new ProxyEntity(s.X, s.Y, s.HitCount + 1, 0), Colors.Gray, z: 3);

            foreach (var d in _gs.SpellDrops)
                EnsureEntityNode($"Spell_{d.Id}", new ProxyEntity(d.X, d.Y, 1, 0), Colors.Gold, z: 4);

            if (_gs.Door != null && _gs.Door.IsActive)
                EnsureEntityNode("Door", new ProxyEntity(_gs.Door.X, _gs.Door.Y, 1, 0), Colors.LightGreen, z: 2);

            foreach (var e in _gs.Enemies)
                EnsureEntityNode($"Enemy_{e.Id}", e, e.Type switch
                {
                    EnemyType.Goblin => new Color("3cb44b"),
                    EnemyType.Orc => new Color("f58231"),
                    EnemyType.Dragon => new Color("911eb4"),
                    EnemyType.Boss => new Color("111111"),
                    _ => new Color("ff5f5f")
                }, showBadge: true, badgeText: e.IsBoss ? "👑" : e.EnemyLevel.ToString(), z: 6);

            // Spieler zuletzt hinzufügen, damit er über Spells gezeichnet wird
            EnsureEntityNode("Player", _gs.Player, Colors.SkyBlue, showBadge: false, z: 8);
        }

        private void EnsureEntityNode(string name, EntityBase ent, Color color, bool showBadge = false, string badgeText = "", int z = 0)
        {
            var node = new Node2D { Name = name, Position = MapToLocal(new Vector2I(ent.X, ent.Y)), ZIndex = z };

            var wrap = new Control { Name = "Wrap", CustomMinimumSize = new Vector2(_tileSize, _tileSize) };

            var box = new ColorRect { Name = "Box", Color = color, Size = new Vector2(_tileSize, _tileSize) };
            wrap.AddChild(box);

            var hp = new Label { Name = "HP", Text = ent.Hp.ToString() };
            hp.HorizontalAlignment = HorizontalAlignment.Center;
            hp.VerticalAlignment = VerticalAlignment.Center;
            hp.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hp.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            wrap.AddChild(hp);

            if (showBadge)
            {
                var badge = new Label { Name = "Badge", Text = badgeText };
                badge.Position = new Vector2(_tileSize - 22, 2);
                wrap.AddChild(badge);
            }

            node.AddChild(wrap);
            EntitiesNode.AddChild(node);
        }

        private Vector2 MapToLocal(Vector2I gridPos)
        {
            return new Vector2(Padding + gridPos.X * _tileSize, Padding + gridPos.Y * _tileSize);
        }

        public void CastSpellFromUI(int index)
        {
            if (_animating) return;
            if (index < 0 || index >= _gs.Player.Spells.Count) return;

            var s = _gs.Player.Spells[index];

            switch (s)
            {
                case FreezeSpell:
                    _gs.EnemiesFrozen = true;
                    _gs.Player.UseSpell(index);
                    break;
                case TeleportSpell:
                    var p = _gs.RandomFreeCell(ignorePlayer: true);
                    _gs.Player.X = p.X; _gs.Player.Y = p.Y;
                    _gs.Player.UseSpell(index);
                    break;
                case LightningSpell:
                    int removed = _gs.Enemies.RemoveAll(e => e.Type == EnemyType.Goblin);
                    if (removed > 0) GD.Print($"⚡ {removed} Goblins vernichtet");
                    _gs.Player.UseSpell(index);
                    break;
                case FireballSpell:
                {
                    int dmg = 8 + (int)(_gs.Player.Level / 2.0);
                    int px = _gs.Player.X; int py = _gs.Player.Y;
                    var toKill = new System.Collections.Generic.List<Enemy>();
                    foreach (var e in _gs.Enemies)
                    {
                        if (e.X == px || e.Y == py)
                        {
                            e.Hp -= dmg;
                            if (e.Hp <= 0) toKill.Add(e);
                        }
                    }
                    foreach (var ek in toKill)
                    {
                        _gs.RegisterPlayerKill(ek);  // Fortschritt + Kills hochzählen
                        _gs.Enemies.Remove(ek);
                        _gs.Player.GainExperience(ek.XpReward);
                        if (ek.IsBoss) GD.Print("Boss getötet!");
                    }
                    _gs.Player.UseSpell(index);
                    break;
                }
                case HealSpell:
                    _gs.Player.UseSpell(index);
                    break;
                default:
                    _gs.Player.UseSpell(index);
                    break;
            }

            _gs.SpawnEnemies();
            SyncScene();
        }

        private sealed class ProxyEntity : EntityBase
        {
            public ProxyEntity(int x, int y, int hp, int atk) : base(x, y, hp, atk) { }
        }
    }
}
