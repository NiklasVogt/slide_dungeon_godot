// scripts/Nodes/GameBoard.cs
using System.Threading.Tasks;
using Godot;
using Dungeon2048.Core.Services;

namespace Dungeon2048.Nodes
{
    /// <summary>
    /// Hauptklasse für das Spielbrett - koordiniert Input, Rendering und Layout
    /// </summary>
    public partial class GameBoard : Node2D
    {
        [Export] public TileMap Grid;
        [Export] public Node2D EntitiesNode;
        [Export] public Control UI;

        private GameContext _ctx;
        private bool _animating = false;
        
        // Komponenten
        private GameBoardLayout _layout;
        private GameBoardRenderer _renderer;
        private GameBoardInput _input;

        public override void _Ready()
        {
            // Registry initialisieren
            RegistryInitializer.InitializeAllRegistries();

            // Game Context erstellen
            _ctx = new GameContext();

            // Nodes finden
            Grid ??= GetNode<TileMap>("Grid");
            EntitiesNode ??= GetNode<Node2D>("Entities");
            UI ??= GetNode<Control>("CanvasLayer/UI");

            // Komponenten initialisieren
            _layout = new GameBoardLayout(this);
            _renderer = new GameBoardRenderer(this, EntitiesNode, _layout);
            _input = new GameBoardInput(this, _ctx);

            // Layout Setup
            GetViewport().SizeChanged += OnViewportSizeChanged;
            _layout.RecomputeTileSize();

            // Initial Sync
            QueueRedraw();
            _renderer.SyncScene(_ctx, UI);
            
            // Debug Info
            LogGameStart();
        }

        public override void _ExitTree()
        {
            if (GetViewport() != null)
                GetViewport().SizeChanged -= OnViewportSizeChanged;
        }

        private void OnViewportSizeChanged()
        {
            _layout.RecomputeTileSize();
            _renderer.SyncScene(_ctx, UI);
        }

        public override void _Draw()
        {
            _renderer.DrawGrid(_ctx);
        }

        public override async void _UnhandledInput(InputEvent @event)
        {
            if (_animating) return;

            if (@event is InputEventKey key && key.Pressed && !key.Echo)
            {
                // Arrow Keys
                var dir = _input.GetDirectionFromKey(key.Keycode);
                if (dir != Vector2I.Zero)
                {
                    await OnArrowMove(dir);
                    return;
                }
                
                // Debug Commands
                #if DEBUG
                if (_input.HandleDebugCommand(key.Keycode))
                {
                    _renderer.SyncScene(_ctx, UI);
                }
                #endif
            }
        }

        private async Task OnArrowMove(Vector2I dir)
        {
            if (_animating) return;
            _animating = true;

            // Register Swipe
            _ctx.RegisterSwipe();
            if (UI is UI uiA) uiA.UpdateFromStateOldShim(_ctx);

            // Movement
            var attackEvents = await MovementPipeline.Move(_ctx, dir.X, dir.Y);

            // Reset Turn Counters for all enemies (wichtig für Schmied-Golem, Burning, etc.)
            foreach (var enemy in _ctx.Enemies)
            {
                enemy.ResetTurnCounters();
            }

            // Animations
            _renderer.AnimateEntities(_ctx);
            await ToSignal(GetTree().CreateTimer(0.12f), SceneTreeTimer.SignalName.Timeout);
            
            await _renderer.AnimateKultistAttacks(_ctx);
            
            foreach (var ev in attackEvents)
            {
                _renderer.AttackFx(ev.Attacker, ev.Target, ev.Dir);
            }
            
            await ToSignal(GetTree().CreateTimer(0.12f), SceneTreeTimer.SignalName.Timeout);
            await ToSignal(GetTree().CreateTimer(0.18f), SceneTreeTimer.SignalName.Timeout);

            // Fire Giant Mechanics (after all entities reached final position)
            _ctx.HandleFireGiantMechanics();

            // Process Burning Damage (after combat)
            ProcessBurningDamage();

            // Process Falling Rock Damage (after all entities reached final position)
            _ctx.ProcessFallingRockDamage();

            // Spawns
            SpawnService.NextTickSpawns(_ctx);
            _renderer.SyncScene(_ctx, UI);
            
            // Game Over Check
            if (_ctx.Player.Hp <= 0)
            {
                LogGameOver();
            }
            
            _animating = false;
        }

        public void CastSpellFromUI(int index)
        {
            if (_animating) return;
            Core.Spells.SpellRegistry.UseSpellSlot(_ctx.Player, index, _ctx);
            SpawnService.NextTickSpawns(_ctx);
            _renderer.SyncScene(_ctx, UI);
        }

        private void ProcessBurningDamage()
        {
            // Process Burning damage on all enemies
            var deadEnemies = new System.Collections.Generic.List<Core.Entities.Enemy>();

            foreach (var enemy in _ctx.Enemies)
            {
                if (enemy.BurningStacks > 0)
                {
                    int damage = Core.Tiles.FireTile.BurningDamage * enemy.BurningStacks;
                    enemy.Hp -= damage;
                    GD.Print($"🔥 {enemy.Type} nimmt {damage} Feuerschaden! ({enemy.BurningStacks} Stacks)");

                    if (enemy.Hp <= 0)
                    {
                        GD.Print($"🔥💀 {enemy.Type} stirbt an Feuerschaden!");
                        deadEnemies.Add(enemy);
                    }
                }
            }

            // Remove dead enemies and grant XP
            foreach (var enemy in deadEnemies)
            {
                _ctx.RegisterPlayerKill(enemy);
                int xp = Core.Entities.Player.CalculateXpReward(enemy.Type, enemy.EnemyLevel, enemy.IsBoss);
                _ctx.Player.GainExperience(xp);
                GD.Print($"💎 +{xp} XP (Tod durch Feuer)");
                _ctx.Enemies.Remove(enemy);
            }

            // Process Burning damage on Player
            if (_ctx.Player.BurningStacks > 0)
            {
                int damage = Core.Tiles.FireTile.BurningDamage * _ctx.Player.BurningStacks;
                _ctx.Player.Hp -= damage;
                GD.Print($"🔥 Du nimmst {damage} Feuerschaden! ({_ctx.Player.BurningStacks} Stacks)");
            }

            // Decay Player Burning Duration
            if (_ctx.Player.BurningTurnsRemaining > 0)
            {
                _ctx.Player.BurningTurnsRemaining--;
                if (_ctx.Player.BurningTurnsRemaining == 0)
                {
                    _ctx.Player.BurningStacks = 0;
                    GD.Print("✨ Spieler: Burning-Effekt ist abgelaufen.");
                }
            }
        }

        private void LogGameStart()
        {
            GD.Print($"=== Game Started ===");
            GD.Print($"Biome: {_ctx.BiomeSystem.CurrentBiome.Name}");
            GD.Print($"Level: {_ctx.CurrentLevel}");
            GD.Print($"Objective: {_ctx.Objective.Description}");
        }

        private void LogGameOver()
        {
            GD.Print("=== GAME OVER ===");
            GD.Print($"Reached Level: {_ctx.CurrentLevel}");
            GD.Print($"Total Kills: {_ctx.TotalEnemiesKilled}");
            GD.Print($"Total Swipes: {_ctx.TotalSwipes}");
            GD.Print($"💎 Seelen gesammelt diesen Run: {_ctx.SoulManager.SoulsThisRun}");
            GD.Print($"💎 Gesamt Seelen: {_ctx.SoulManager.CurrentSouls}");
            
            _ctx.SoulManager.Save();
        }
    }
}