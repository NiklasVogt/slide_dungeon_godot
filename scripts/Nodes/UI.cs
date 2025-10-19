// scripts/Nodes/UI.cs
using Godot;
using Dungeon2048.Core.Services;
using Dungeon2048.Core.Spells;
using Dungeon2048.Core.Objectives;
using System.Linq;

namespace Dungeon2048.Nodes
{
    public partial class UI : Control
    {
        private Label _title;
        private Label _biomeLabel;
        private Label _soulLabel;
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
            AnchorLeft = 1f;
            AnchorRight = 1f;
            AnchorTop = 0f;
            AnchorBottom = 0f;
            OffsetRight = -12f;
            OffsetTop = 12f;
            OffsetBottom = 0f;
            OffsetLeft = -_desiredWidth - 12f;
            GetViewport().SizeChanged += OnViewportSizeChanged;

            var panel = new Panel
            {
                Name = "Panel",
                CustomMinimumSize = new Vector2(_desiredWidth, 300)
            };
            AddChild(panel);

            var vb = new VBoxContainer
            {
                Name = "VBox",
                CustomMinimumSize = new Vector2(_desiredWidth - 20f, 260),
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            vb.Position = new Vector2(10, 10);
            panel.AddChild(vb);

            _title = new Label { Text = "Dungeon 2048" };
            _title.AddThemeFontSizeOverride("font_size", 18);
            
            _biomeLabel = new Label { Text = "Biome: ..." };
            _biomeLabel.AddThemeFontSizeOverride("font_size", 12);

            _soulLabel = new Label { Text = "💎 Seelen: 0 (Run: +0)" };
            _soulLabel.AddThemeFontSizeOverride("font_size", 14);
            _soulLabel.Modulate = new Color("9c27b0");
            
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
            vb.AddChild(_biomeLabel);
            vb.AddChild(_soulLabel);
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
                if (_board == null)
                    _board = GetTree().Root.GetNodeOrNull<GameBoard>("Main/GameBoard");
                _board?.CastSpellFromUI(index);
            };
            return b;
        }

        public void UpdateFromState(GameContext gs)
        {
            if (gs == null) return;

            // Boss-Level spezielle Anzeige
            string levelIndicator = "";
            if (ObjectiveService.IsBossLevel(gs.CurrentLevel))
            {
                levelIndicator = " 👑 BOSS LEVEL 👑";
            }
            
            _title.Text = $"Level {gs.CurrentLevel}{levelIndicator} {gs.Objective.Icon} {gs.Objective.Description}";
            
            var biome = gs.BiomeSystem?.CurrentBiome;
            if (biome != null)
            {
                string biomeText = $"🏛️ {biome.Name} (Level {biome.StartLevel}-{biome.EndLevel})";
                
                // Boss-Status anzeigen
                if (gs.Enemies.Any(e => e.IsBoss))
                {
                    biomeText += " | ⚔️ BOSS AKTIV!";
                }
                
                _biomeLabel.Text = biomeText;
                _biomeLabel.Modulate = biome.AmbientColor;
            }
            _soulLabel.Text = $"💎 Seelen: {gs.SoulManager.CurrentSouls} (Run: +{gs.SoulManager.SoulsThisRun})";
            _objBar.Value = gs.Objective.Progress;
            _objText.Text = gs.Objective.ProgressText;
            _stats.Text = $"HP {gs.Player.Hp}/{gs.Player.MaxHp} | ATK {gs.Player.Atk} | LVL {gs.Player.Level}\nSwipes {gs.TotalSwipes} | Kills {gs.TotalEnemiesKilled}";

            SetSpellSlot(_spell0, gs, 0);
            SetSpellSlot(_spell1, gs, 1);
            SetSpellSlot(_spell2, gs, 2);
        }

        private void SetSpellSlot(Button btn, GameContext gs, int idx)
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

        public void UpdateFromStateOldShim(GameContext gs) => UpdateFromState(gs);
    }
}