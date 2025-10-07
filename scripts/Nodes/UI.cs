using Godot;
using Dungeon2048.Core;

namespace Dungeon2048.Nodes
{
    // Klassenname = Dateiname (UI.cs) für Godot-Bindung
    public partial class UI : Control
    {
        private Label _title;
        private ProgressBar _objBar;
        private Label _objText;
        private Label _stats;

        private Button _spell0;
        private Button _spell1;
        private Button _spell2;

        private GameBoard _board;
        private float _desiredWidth = 360f;

        public override void _Ready()
        {
            _board = GetParent()?.GetParent() as GameBoard;

            // Rechts andocken
            AnchorLeft = 1f;
            AnchorRight = 1f;
            AnchorTop = 0f;
            AnchorBottom = 0f;

            OffsetRight = -12f;
            OffsetTop = 12f;
            OffsetBottom = 0f;
            OffsetLeft = -_desiredWidth - 12f;

            // Viewport-Resize
            GetViewport().SizeChanged += OnViewportSizeChanged;

            var panel = new Panel
            {
                Name = "Panel",
                CustomMinimumSize = new Vector2(_desiredWidth, 260)
            };
            AddChild(panel);

            var vb = new VBoxContainer
            {
                Name = "VBox",
                CustomMinimumSize = new Vector2(_desiredWidth - 20f, 240),
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            vb.Position = new Vector2(10, 10);
            panel.AddChild(vb);

            _title = new Label { Text = "Dungeon 2048" };
            _title.AddThemeFontSizeOverride("font_size", 18);

            _objBar = new ProgressBar { MinValue = 0, MaxValue = 1, Value = 0, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            _objText = new Label { Text = "…" };

            _stats = new Label { Text = "HP/ATK/Swipes/Kills" };

            var spellsLabel = new Label { Text = "Zauber" };
            var spells = new HBoxContainer
            {
                Name = "Spells",
                CustomMinimumSize = new Vector2(_desiredWidth - 20f, 40),
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };

            _spell0 = MakeSpellButton(0);
            _spell1 = MakeSpellButton(1);
            _spell2 = MakeSpellButton(2);

            spells.AddChild(_spell0);
            spells.AddChild(_spell1);
            spells.AddChild(_spell2);

            vb.AddChild(_title);
            vb.AddChild(_objBar);
            vb.AddChild(_objText);
            vb.AddChild(_stats);
            vb.AddChild(spellsLabel);
            vb.AddChild(spells);
        }

        public override void _ExitTree()
        {
            if (GetViewport() != null)
                GetViewport().SizeChanged -= OnViewportSizeChanged;
        }

        private void OnViewportSizeChanged()
        {
            var vpw = GetViewportRect().Size.X;
            float w = Mathf.Clamp(vpw * 0.28f, 320f, 520f);
            SetDesiredWidth(w);
        }

        public void SetDesiredWidth(float width)
        {
            _desiredWidth = width;

            OffsetLeft = -_desiredWidth - 12f;

            var panel = GetNodeOrNull<Panel>("Panel");
            if (panel != null)
                panel.CustomMinimumSize = new Vector2(_desiredWidth, panel.CustomMinimumSize.Y);

            var vb = GetNodeOrNull<VBoxContainer>("Panel/VBox");
            if (vb != null)
                vb.CustomMinimumSize = new Vector2(_desiredWidth - 20f, vb.CustomMinimumSize.Y);

            var spells = GetNodeOrNull<HBoxContainer>("Panel/VBox/Spells");
            if (spells != null)
                spells.CustomMinimumSize = new Vector2(_desiredWidth - 20f, spells.CustomMinimumSize.Y);
        }

        private Button MakeSpellButton(int index)
        {
            var b = new Button
            {
                Text = "Leer",
                CustomMinimumSize = new Vector2(96, 36),
                Disabled = true
            };
            b.Pressed += () =>
            {
                if (_board != null)
                    _board.CastSpellFromUI(index);
            };
            return b;
        }

        public void UpdateFromState(GameState gs)
        {
            if (gs == null) return;

            _title.Text = $"Level {gs.CurrentLevel}  {gs.CurrentObjective.Icon}  {gs.CurrentObjective.Description}";
            _objBar.Value = gs.ObjectiveProgress();
            _objText.Text = gs.CurrentObjective.ProgressText;
            _stats.Text = $"HP {gs.Player.Hp}/{gs.Player.MaxHp}  ATK {gs.Player.Atk}  Swipes {gs.TotalSwipes}  Kills {gs.TotalEnemiesKilled}";

            SetSpellSlot(_spell0, gs, 0);
            SetSpellSlot(_spell1, gs, 1);
            SetSpellSlot(_spell2, gs, 2);
        }

        private void SetSpellSlot(Button btn, GameState gs, int idx)
        {
            if (gs.Player.Spells.Count > idx)
            {
                var sp = gs.Player.Spells[idx];
                btn.Text = sp.Name;
                btn.Disabled = false;
                switch (sp.Type)
                {
                    case SpellType.Fireball: btn.Modulate = new Color("ff5722"); break;
                    case SpellType.Heal: btn.Modulate = new Color("4caf50"); break;
                    case SpellType.Freeze: btn.Modulate = new Color("03a9f4"); break;
                    case SpellType.Lightning: btn.Modulate = new Color("fbc02d"); break;
                    case SpellType.Teleport: btn.Modulate = new Color("9c27b0"); break;
                    default: btn.Modulate = Colors.White; break;
                }
            }
            else
            {
                btn.Text = "Leer";
                btn.Disabled = true;
                btn.Modulate = Colors.White;
            }
        }
    }
}
