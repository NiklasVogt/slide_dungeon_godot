using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Dungeon2048.Core.Services;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Spells;
using Dungeon2048.Core.Tiles;
using Dungeon2048.Core.Enemies;
using Dungeon2048.Core.Objectives;

namespace Dungeon2048.Nodes
{
    public partial class GameBoard : Node2D
    {
        [Export] public TileMap Grid;
        [Export] public Node2D EntitiesNode;
        [Export] public Control UI;

        private GameContext _ctx;
        private bool _animating = false;
        private float _tileSize = 96f;
        private const float Padding = 12f;
        private float _reservedUiWidth = 360f;
        private readonly Dictionary<string, Node2D> _entityNodes = new();

        public override void _Ready()
        {
            // Registries bootstrap
            TileRegistry.Register(new StoneTile());
            TileRegistry.Register(new DoorTile());
            TileRegistry.Register(new SpellDropTile());

            // WICHTIG: Enemies-Namespace ist per using eingebunden
            EnemyRegistry.Register(new GoblinArch());
            EnemyRegistry.Register(new OrcArch());
            EnemyRegistry.Register(new DragonArch());
            EnemyRegistry.Register(new MasochistArch());
            EnemyRegistry.Register(new ThornsArch());

            SpellRegistry.Register(SpellType.Fireball, new FireballBehavior());
            SpellRegistry.Register(SpellType.Heal, new HealBehavior());
            SpellRegistry.Register(SpellType.Freeze, new FreezeBehavior());
            SpellRegistry.Register(SpellType.Lightning, new LightningBehavior());
            SpellRegistry.Register(SpellType.Teleport, new TeleportBehavior());

            _ctx = new GameContext();

            Grid ??= GetNode<TileMap>("Grid");
            EntitiesNode ??= GetNode<Node2D>("Entities");
            UI ??= GetNode<Control>("CanvasLayer/UI");

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
            float cellW = usableW / GameContext.GridSize;
            float cellH = usableH / GameContext.GridSize;
            _tileSize = Mathf.Floor(Mathf.Min(Mathf.Min(cellW, cellH), 200f));
        }

        private float ComputeReservedUiWidth(float viewportWidth)
            => Mathf.Clamp(viewportWidth * 0.28f, 320f, 520f);

        public override void _Draw()
        {
            var size = GameContext.GridSize;
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
                if (dir != Vector2I.Zero) await OnArrowMove(dir);
            }
        }

        private async Task OnArrowMove(Vector2I dir)
        {
            if (_animating) return;
            _animating = true;

            _ctx.RegisterSwipe();
            if (UI is UI uiA) uiA.UpdateFromStateOldShim(_ctx);

            var attackEvents = await MovementPipeline.Move(_ctx, dir.X, dir.Y);

            AnimateEntities();
            await ToSignal(GetTree().CreateTimer(0.12f), SceneTreeTimer.SignalName.Timeout);
            foreach (var ev in attackEvents)
            {
                AttackFx(ev.Attacker, ev.Target, ev.Dir);
            }
            await ToSignal(GetTree().CreateTimer(0.12f), SceneTreeTimer.SignalName.Timeout);
            await ToSignal(GetTree().CreateTimer(0.18f), SceneTreeTimer.SignalName.Timeout);

            SpawnService.NextTickSpawns(_ctx);
            SyncScene();
            _animating = false;
        }

        private void SyncScene()
        {
            AnimateEntities();
            if (UI is UI ui) ui.UpdateFromStateOldShim(_ctx);

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

        private void SlideNodeTo(Node2D node, Vector2 target)
        {
            var dist = (node.Position - target).Length();
            double duration = Mathf.Clamp(dist / Mathf.Max(1f, _tileSize) * 0.07, 0.08, 0.25);
            if (dist < 0.01f) return;
            var tw = CreateTween();
            tw.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
            tw.TweenProperty(node, "position", target, duration);
        }

        private Node2D GetOrCreateEntityNode(string name, int z, Color color, int hp, bool showBadge = false, string badgeText = "")
        {
            if (_entityNodes.TryGetValue(name, out var node))
                return node;

            node = new Node2D { Name = name, ZIndex = z };
            var wrap = new Control { Name = "Wrap", CustomMinimumSize = new Vector2(_tileSize, _tileSize) };
            var box = new ColorRect { Name = "Box", Color = color, Size = new Vector2(_tileSize, _tileSize) };
            wrap.AddChild(box);

            var hpLbl = new Label { Name = "HP", Text = hp.ToString() };
            hpLbl.HorizontalAlignment = HorizontalAlignment.Center;
            hpLbl.VerticalAlignment = VerticalAlignment.Center;
            hpLbl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hpLbl.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            wrap.AddChild(hpLbl);

            if (showBadge)
            {
                var badge = new Label { Name = "Badge", Text = badgeText };
                badge.Position = new Vector2(_tileSize - 22, 2);
                wrap.AddChild(badge);
            }

            node.AddChild(wrap);
            EntitiesNode.AddChild(node);
            _entityNodes[name] = node;
            return node;
        }

        private void UpdateEntityNodeVisuals(string name, int hp, string? badgeText = null)
        {
            if (!_entityNodes.TryGetValue(name, out var node)) return;
            var wrap = node.GetNodeOrNull<Control>("Wrap");
            if (wrap == null) return;

            wrap.CustomMinimumSize = new Vector2(_tileSize, _tileSize);
            var box = wrap.GetNodeOrNull<ColorRect>("Box");
            if (box != null) box.Size = new Vector2(_tileSize, _tileSize);

            var hpLbl = wrap.GetNodeOrNull<Label>("HP");
            if (hpLbl != null) hpLbl.Text = hp.ToString();

            if (badgeText != null)
            {
                var badge = wrap.GetNodeOrNull<Label>("Badge");
                if (badge != null) badge.Text = badgeText;
            }
        }

        private void AnimateEntities()
        {
            // Player
            {
                var name = "Player";
                var node = GetOrCreateEntityNode(name, 8, Colors.SkyBlue, _ctx.Player.Hp);
                SlideNodeTo(node, MapToLocal(new Vector2I(_ctx.Player.X, _ctx.Player.Y)));
                UpdateEntityNodeVisuals(name, _ctx.Player.Hp);
            }

            // Enemies mit Farben je Typ (inkl. neue Rare-Typen)
            foreach (var e in _ctx.Enemies)
            {
                var name = $"Enemy_{e.Id}";
                var color = e.Type switch
                {
                    EnemyType.Goblin    => new Color("3cb44b"), // grün
                    EnemyType.Orc       => new Color("f58231"), // orange
                    EnemyType.Dragon    => new Color("911eb4"), // violett
                    EnemyType.Boss      => new Color("111111"), // fast schwarz
                    EnemyType.Masochist => new Color("4699e1"), // blau-türkis
                    EnemyType.Thorns    => new Color("22aa22"), // dunkelgrün
                    _                   => new Color("ff5f5f")
                };

                var node = GetOrCreateEntityNode(name, 6, color, e.Hp, showBadge: true, badgeText: e.IsBoss ? "👑" : e.EnemyLevel.ToString());
                SlideNodeTo(node, MapToLocal(new Vector2I(e.X, e.Y)));
                UpdateEntityNodeVisuals(name, e.Hp, e.IsBoss ? "👑" : e.EnemyLevel.ToString());
            }

            // Stones
            foreach (var s in _ctx.Stones)
            {
                var name = $"Stone_{s.Id}";
                var node = GetOrCreateEntityNode(name, 3, Colors.Gray, s.HitCount + 1);
                SlideNodeTo(node, MapToLocal(new Vector2I(s.X, s.Y)));
                UpdateEntityNodeVisuals(name, s.HitCount + 1);
            }

            // Spells
            foreach (var d in _ctx.SpellDrops)
            {
                var name = $"Spell_{d.Id}";
                var node = GetOrCreateEntityNode(name, 4, Colors.Gold, 1);
                SlideNodeTo(node, MapToLocal(new Vector2I(d.X, d.Y)));
                UpdateEntityNodeVisuals(name, 1);
            }

            // Door
            if (_ctx.Door != null && _ctx.Door.IsActive)
            {
                var name = "Door";
                var node = GetOrCreateEntityNode(name, 2, Colors.LightGreen, 1);
                SlideNodeTo(node, MapToLocal(new Vector2I(_ctx.Door.X, _ctx.Door.Y)));
                UpdateEntityNodeVisuals(name, 1);
            }

            PruneMissingNodes();
        }

        private void PruneMissingNodes()
        {
            var alive = new System.Collections.Generic.HashSet<string> { "Player" };
            foreach (var e in _ctx.Enemies) alive.Add($"Enemy_{e.Id}");
            foreach (var s in _ctx.Stones) alive.Add($"Stone_{s.Id}");
            foreach (var d in _ctx.SpellDrops) alive.Add($"Spell_{d.Id}");
            if (_ctx.Door != null && _ctx.Door.IsActive) alive.Add("Door");

            foreach (var kv in _entityNodes.ToArray())
            {
                if (!alive.Contains(kv.Key))
                {
                    kv.Value.QueueFree();
                    _entityNodes.Remove(kv.Key);
                }
            }
        }

        private Vector2 MapToLocal(Vector2I gridPos)
            => new Vector2(Padding + gridPos.X * _tileSize, Padding + gridPos.Y * _tileSize);

        private void AttackFx(string attackerName, string targetName, Vector2I dir)
        {
            if (!_entityNodes.TryGetValue(attackerName, out var attackerNode))
                return;

            var origin = attackerNode.Position;
            var nudge = new Vector2(dir.X, dir.Y) * (_tileSize * 0.25f);
            var tw = CreateTween();
            tw.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            tw.TweenProperty(attackerNode, "position", origin + nudge, 0.06);
            tw.TweenProperty(attackerNode, "position", origin, 0.08).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);

            if (_entityNodes.TryGetValue(targetName, out var targetNode))
            {
                var hit = CreateTween();
                hit.TweenProperty(targetNode, "scale", new Vector2(1.06f, 1.06f), 0.05);
                hit.TweenProperty(targetNode, "scale", new Vector2(1f, 1f), 0.08);
            }
        }

        public void CastSpellFromUI(int index)
        {
            if (_animating) return;
            SpellRegistry.UseSpellSlot(_ctx.Player, index, _ctx);
            SpawnService.NextTickSpawns(_ctx);
            SyncScene();
        }
    }
}
