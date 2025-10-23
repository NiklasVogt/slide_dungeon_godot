// scripts/Nodes/GameBoardRenderer.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Dungeon2048.Core.Services;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Tiles;

namespace Dungeon2048.Nodes
{
    /// <summary>
    /// Verwaltet Rendering und Animationen des GameBoards
    /// </summary>
    public sealed partial class GameBoardRenderer
    {
        private readonly Node2D _gameBoard;
        private readonly Node2D _entitiesNode;
        private readonly GameBoardLayout _layout;
        private readonly Dictionary<string, Node2D> _entityNodes = new();

        public GameBoardRenderer(Node2D gameBoard, Node2D entitiesNode, GameBoardLayout layout)
        {
            _gameBoard = gameBoard;
            _entitiesNode = entitiesNode;
            _layout = layout;
        }

        /// <summary>
        /// Zeichnet das Grid
        /// </summary>
        public void DrawGrid(GameContext ctx)
        {
            var size = GameContext.GridSize;
            var ts = _layout.TileSize;
            var origin = _layout.GetGridOrigin();
            var boardSize = _layout.GetBoardSize();

            // Biome-basierte Hintergrundfarbe
            var bg = ctx.BiomeSystem.GetBackgroundColor();
            _gameBoard.DrawRect(new Rect2(origin, boardSize), bg);

            // Spezielle Tile-Highlights BEVOR die Grid-Linien gezeichnet werden
            DrawSpecialTileHighlights(ctx, origin, ts);

            // Biome-basierte Gridfarbe
            var col = ctx.BiomeSystem.GetGridColor();

            for (int i = 0; i <= size; i++)
            {
                _gameBoard.DrawLine(
                    new Vector2(origin.X, origin.Y + i * ts),
                    new Vector2(origin.X + size * ts, origin.Y + i * ts),
                    col, 2
                );
                _gameBoard.DrawLine(
                    new Vector2(origin.X + i * ts, origin.Y),
                    new Vector2(origin.X + i * ts, origin.Y + size * ts),
                    col, 2
                );
            }
        }

        /// <summary>
        /// Zeichnet spezielle Highlights für Tiles (FallingRocks, Fire, etc.)
        /// </summary>
        private void DrawSpecialTileHighlights(GameContext ctx, Vector2 origin, float ts)
        {
            // Fire Tiles - Orange Glow
            foreach (var fire in ctx.FireTiles)
            {
                if (fire.IsExtinguished) continue;

                var tilePos = new Vector2(origin.X + fire.X * ts, origin.Y + fire.Y * ts);
                var tileRect = new Rect2(tilePos, new Vector2(ts, ts));

                // Orange-rotes Glühen mit etwas Transparenz
                var fireGlow = new Color(1.0f, 0.3f, 0.0f, 0.3f);
                _gameBoard.DrawRect(tileRect, fireGlow);
            }

            // Falling Rocks - Warnung und Gefahr
            foreach (var rock in ctx.FallingRocks)
            {
                if (rock.HasFallen) continue;

                var tilePos = new Vector2(origin.X + rock.X * ts, origin.Y + rock.Y * ts);
                var tileRect = new Rect2(tilePos, new Vector2(ts, ts));

                if (rock.IsWarning)
                {
                    // Gelbe Warnung (pulsierend darstellbar durch Transparenz)
                    var warningColor = new Color(1.0f, 0.8f, 0.0f, 0.4f);
                    _gameBoard.DrawRect(tileRect, warningColor);

                    // Zusätzlicher Rand für extra Aufmerksamkeit
                    _gameBoard.DrawRect(tileRect, new Color(1.0f, 0.6f, 0.0f, 0.8f), false, 3);
                }
                else if (rock.ShouldFall)
                {
                    // Rote Gefahr (stark leuchtend)
                    var dangerColor = new Color(1.0f, 0.0f, 0.0f, 0.6f);
                    _gameBoard.DrawRect(tileRect, dangerColor);

                    // Dicker roter Rand
                    _gameBoard.DrawRect(tileRect, new Color(1.0f, 0.0f, 0.0f, 1.0f), false, 4);
                }
            }
        }

        /// <summary>
        /// Synchronisiert Scene mit Game State
        /// </summary>
        public void SyncScene(GameContext ctx, Control ui)
        {
            AnimateEntities(ctx);
            
            if (ui is UI uiComponent) 
                uiComponent.UpdateFromStateOldShim(ctx);

            _layout.PositionUI(ui);
            _gameBoard.QueueRedraw();
        }

        /// <summary>
        /// Animiert alle Entities
        /// </summary>
        public void AnimateEntities(GameContext ctx)
        {
            AnimatePlayer(ctx);
            AnimateEnemies(ctx);
            AnimateStones(ctx);
            AnimateGravestones(ctx);
            AnimateTorches(ctx);
            AnimateBonePiles(ctx);
            AnimateSpellDrops(ctx);
            AnimateTeleporters(ctx);
            AnimateRuneTraps(ctx);
            AnimateMagicBarriers(ctx);
            AnimateFireTiles(ctx);
            AnimateFallingRocks(ctx);
            AnimateDoor(ctx);

            PruneMissingNodes(ctx);
        }

        private void AnimatePlayer(GameContext ctx)
        {
            var name = "Player";
            var node = GetOrCreateEntityNode(name, 8, Colors.SkyBlue, ctx.Player.Hp, displayName: "Spieler");
            SlideNodeTo(node, _layout.MapToLocal(new Vector2I(ctx.Player.X, ctx.Player.Y)));
            UpdateEntityNodeVisuals(name, ctx.Player.Hp, displayName: "Spieler");
        }

        private void AnimateEnemies(GameContext ctx)
        {
            foreach (var e in ctx.Enemies)
            {
                var name = $"Enemy_{e.Id}";
                
                // Getarnter Mimic wird als Spell-Drop dargestellt
                if (e.Type == EnemyType.Mimic && e.IsDisguised)
                {
                    var node = GetOrCreateEntityNode(name, 4, Colors.Gold, 1, displayName: "Zauber");
                    SlideNodeTo(node, _layout.MapToLocal(new Vector2I(e.X, e.Y)));
                    UpdateEntityNodeVisuals(name, 1, null, "Zauber");
                    continue;
                }
                
                var color = GetEnemyColor(e.Type);
                string badge = GetEnemyBadge(e);
                var displayName = e.DisplayName;
                
                var enemyNode = GetOrCreateEntityNode(name, 6, color, e.Hp, 
                    showBadge: true, badgeText: badge, displayName: displayName);
                SlideNodeTo(enemyNode, _layout.MapToLocal(new Vector2I(e.X, e.Y)));
                UpdateEntityNodeVisuals(name, e.Hp, badge, displayName);
                
                if (e.Type == EnemyType.Necrophage)
                    e.HealedThisRound = 0;
            }
        }

        private void AnimateStones(GameContext ctx)
        {
            foreach (var s in ctx.Stones)
            {
                var name = $"Stone_{s.Id}";
                var node = GetOrCreateEntityNode(name, 3, Colors.Gray, 
                    Stone.MaxHits - s.HitCount, displayName: "Stein");
                SlideNodeTo(node, _layout.MapToLocal(new Vector2I(s.X, s.Y)));
                UpdateEntityNodeVisuals(name, Stone.MaxHits - s.HitCount, displayName: "Stein");
            }
        }

        private void AnimateGravestones(GameContext ctx)
        {
            foreach (var g in ctx.Gravestones)
            {
                var name = $"Gravestone_{g.Id}";
                var node = GetOrCreateEntityNode(name, 3, new Color("4a4a4a"), 
                    Gravestone.MaxHits - g.HitCount, displayName: "Grabstein");
                SlideNodeTo(node, _layout.MapToLocal(new Vector2I(g.X, g.Y)));
                UpdateEntityNodeVisuals(name, Gravestone.MaxHits - g.HitCount, displayName: "Grabstein");
            }
        }

        private void AnimateTorches(GameContext ctx)
        {
            foreach (var t in ctx.Torches)
            {
                var name = $"Torch_{t.Id}";
                var flickerColor = t.IsLit ? new Color("ff8c00") : new Color("8b4513");
                var node = GetOrCreateEntityNode(name, 2, flickerColor, 1, displayName: "Fackel");
                SlideNodeTo(node, _layout.MapToLocal(new Vector2I(t.X, t.Y)));
                UpdateEntityNodeVisuals(name, 1, displayName: "Fackel");
            }
        }

        private void AnimateBonePiles(GameContext ctx)
        {
            foreach (var b in ctx.BonePiles)
            {
                var name = $"BonePile_{b.Id}";
                int hitsRemaining = BonePile.MaxHits - b.HitCount;
                int swipesUntilRevive = BonePile.MaxSwipesAlive - b.SwipesAlive;
                
                string reviveIndicator = swipesUntilRevive > 0 ? $"💀{swipesUntilRevive}" : "💀!";
                
                var node = GetOrCreateEntityNode(name, 3, new Color("d3d3d3"), hitsRemaining, 
                    showBadge: true, badgeText: reviveIndicator, displayName: "Knochen");
                SlideNodeTo(node, _layout.MapToLocal(new Vector2I(b.X, b.Y)));
                UpdateEntityNodeVisuals(name, hitsRemaining, reviveIndicator, "Knochen");
            }
        }

        private void AnimateSpellDrops(GameContext ctx)
        {
            foreach (var d in ctx.SpellDrops)
            {
                var name = $"Spell_{d.Id}";
                var node = GetOrCreateEntityNode(name, 4, Colors.Gold, 1, displayName: "Zauber");
                SlideNodeTo(node, _layout.MapToLocal(new Vector2I(d.X, d.Y)));
                UpdateEntityNodeVisuals(name, 1, displayName: "Zauber");
            }
        }

        private void AnimateTeleporters(GameContext ctx)
        {
            foreach (var t in ctx.Teleporters)
            {
                if (!t.IsActive) continue;
                
                var name = $"Teleporter_{t.Id}";
                var node = GetOrCreateEntityNode(name, 2, new Color("00ffff"), 1, displayName: "Portal");
                SlideNodeTo(node, _layout.MapToLocal(new Vector2I(t.X, t.Y)));
                UpdateEntityNodeVisuals(name, 1, displayName: "Portal");
            }
        }

        private void AnimateRuneTraps(GameContext ctx)
        {
            foreach (var r in ctx.RuneTraps)
            {
                if (r.IsTriggered) continue;
                
                var name = $"RuneTrap_{r.Id}";
                var color = r.IsRevealed ? new Color("ff00ff") : new Color(0.05f, 0.05f, 0.15f, 0.3f);
                var displayName = r.IsRevealed ? "Falle!" : "";
                
                var node = GetOrCreateEntityNode(name, 1, color, 1, displayName: displayName);
                SlideNodeTo(node, _layout.MapToLocal(new Vector2I(r.X, r.Y)));
                UpdateEntityNodeVisuals(name, 1, displayName: displayName);
            }
        }

        private void AnimateMagicBarriers(GameContext ctx)
        {
            foreach (var m in ctx.MagicBarriers)
            {
                var name = $"MagicBarrier_{m.Id}";
                int hitsRemaining = MagicBarrier.MaxHits - m.HitCount;
                
                if (m.IsDestroyed)
                {
                    int regenTime = MagicBarrier.RegenerationTime - m.SwipesSinceBroken;
                    var node = GetOrCreateEntityNode(name, 3, new Color("4b0082", 0.3f), regenTime,
                        showBadge: true, badgeText: $"⏱{regenTime}", displayName: "Regeneriert");
                    SlideNodeTo(node, _layout.MapToLocal(new Vector2I(m.X, m.Y)));
                    UpdateEntityNodeVisuals(name, regenTime, $"⏱{regenTime}", "Regeneriert");
                }
                else
                {
                    var node = GetOrCreateEntityNode(name, 3, new Color("9370db"), hitsRemaining, displayName: "Barriere");
                    SlideNodeTo(node, _layout.MapToLocal(new Vector2I(m.X, m.Y)));
                    UpdateEntityNodeVisuals(name, hitsRemaining, displayName: "Barriere");
                }
            }
        }

        private void AnimateDoor(GameContext ctx)
        {
            if (ctx.Door != null && ctx.Door.IsActive)
            {
                var name = "Door";
                var node = GetOrCreateEntityNode(name, 2, Colors.LightGreen, 1, displayName: "Tür");
                SlideNodeTo(node, _layout.MapToLocal(new Vector2I(ctx.Door.X, ctx.Door.Y)));
                UpdateEntityNodeVisuals(name, 1, displayName: "Tür");
            }
        }

        private void AnimateFireTiles(GameContext ctx)
        {
            foreach (var f in ctx.FireTiles)
            {
                if (f.IsExtinguished) continue;

                var name = $"FireTile_{f.Id}";
                int passesLeft = FireTile.MaxPasses - f.EntitiesPassedThrough;

                // Glühende Orange-Rot Farbe für Feuer
                var fireColor = new Color(1.0f, 0.3f, 0.0f, 0.85f); // Orange-Rot, leicht transparent

                var node = GetOrCreateEntityNode(name, 5, fireColor, passesLeft,
                    showBadge: true, badgeText: "🔥", displayName: "Lava");
                SlideNodeTo(node, _layout.MapToLocal(new Vector2I(f.X, f.Y)));
                UpdateEntityNodeVisuals(name, passesLeft, "🔥", "Lava");
            }
        }

        private void AnimateFallingRocks(GameContext ctx)
        {
            foreach (var r in ctx.FallingRocks)
            {
                if (r.HasFallen) continue;

                var name = $"FallingRock_{r.Id}";

                if (r.IsWarning)
                {
                    // Warnung: Gelb/Orange blinkend
                    var warningColor = new Color(1.0f, 0.8f, 0.0f, 0.8f); // Gelb, weniger transparent
                    var node = GetOrCreateEntityNode(name, 5, warningColor, r.WarningTurnsRemaining,
                        showBadge: true, badgeText: "⚠️", displayName: "Warnung!");
                    SlideNodeTo(node, _layout.MapToLocal(new Vector2I(r.X, r.Y)));
                    UpdateEntityNodeVisuals(name, r.WarningTurnsRemaining, "⚠️", "Warnung!");
                }
                else if (r.ShouldFall)
                {
                    // Direkt vor dem Fallen: Rot
                    var dangerColor = new Color(1.0f, 0.0f, 0.0f, 0.9f); // Rot, sehr sichtbar
                    var node = GetOrCreateEntityNode(name, 7, dangerColor, 1,
                        showBadge: true, badgeText: "💥", displayName: "GEFAHR!");
                    SlideNodeTo(node, _layout.MapToLocal(new Vector2I(r.X, r.Y)));
                    UpdateEntityNodeVisuals(name, 1, "💥", "GEFAHR!");
                }
            }
        }

        // Fortsetzung in GameBoardRenderer_Helpers.cs ...
    }
}