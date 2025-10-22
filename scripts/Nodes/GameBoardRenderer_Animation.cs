// scripts/Nodes/GameBoardRenderer_Animation.cs
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
    /// Spezielle Animationen für GameBoardRenderer (partial class)
    /// </summary>
    public sealed partial class GameBoardRenderer
    {
        /// <summary>
        /// Animiert Kultist-Angriffe mit Projektilen
        /// </summary>
        public async Task AnimateKultistAttacks(GameContext ctx)
        {
            var kultists = ctx.Enemies.Where(e => e.Type == EnemyType.Kultist).ToList();
            if (kultists.Count == 0) return;
            
            var animationTasks = new List<Task>();
            
            foreach (var kultist in kultists)
            {
                var kultistPos = _layout.MapToLocal(new Vector2I(kultist.X, kultist.Y));
                kultistPos += new Vector2(_layout.TileSize / 2, _layout.TileSize / 2); // Center
                
                // 4 Richtungen
                var directions = new[]
                {
                    new Vector2I(1, 0),   // Rechts
                    new Vector2I(-1, 0),  // Links
                    new Vector2I(0, 1),   // Unten
                    new Vector2I(0, -1)   // Oben
                };
                
                foreach (var dir in directions)
                {
                    int range = 3;
                    
                    for (int step = 1; step <= range; step++)
                    {
                        int targetX = kultist.X + (dir.X * step);
                        int targetY = kultist.Y + (dir.Y * step);
                        
                        // Out of bounds
                        if (targetX < 0 || targetX >= GameContext.GridSize || 
                            targetY < 0 || targetY >= GameContext.GridSize)
                            break;
                        
                        // Blocked by tiles
                        if (TileRegistry.AnyBlocks(kultist, ctx, targetX, targetY))
                            break;
                        
                        var targetPos = _layout.MapToLocal(new Vector2I(targetX, targetY));
                        targetPos += new Vector2(_layout.TileSize / 2, _layout.TileSize / 2); // Center
                        
                        // Animiere Projektil
                        var task = AnimateKultistProjectile(kultistPos, targetPos, new Color("8a2be2"));
                        animationTasks.Add(task);
                        
                        // Hit Player oder Enemy? Dann stoppen
                        bool hitSomething = false;
                        
                        if (ctx.Player.X == targetX && ctx.Player.Y == targetY)
                        {
                            hitSomething = true;
                            FlashTarget("Player");
                        }
                        
                        var targetEnemy = ctx.Enemies.FirstOrDefault(e => 
                            e.X == targetX && e.Y == targetY && e.Id != kultist.Id
                        );
                        
                        if (targetEnemy != null)
                        {
                            hitSomething = true;
                        }
                        
                        if (hitSomething) break;
                    }
                }
            }
            
            // Warte auf alle Projektile gleichzeitig
            await Task.WhenAll(animationTasks);
            await Task.Delay(100); // Kurze Pause nach allen Projektilen
        }

        private async Task AnimateKultistProjectile(Vector2 startPos, Vector2 endPos, Color color)
        {
            var projectile = new ColorRect
            {
                Size = new Vector2(8, 8),
                Color = color,
                Position = startPos,
                ZIndex = 10
            };
            
            _gameBoard.AddChild(projectile);
            
            var tween = _gameBoard.CreateTween();
            tween.SetTrans(Tween.TransitionType.Linear);
            tween.TweenProperty(projectile, "position", endPos, 0.2);
            
            await _gameBoard.ToSignal(tween, Tween.SignalName.Finished);
            projectile.QueueFree();
        }

        private void FlashTarget(string targetName)
        {
            if (!_entityNodes.TryGetValue(targetName, out var node)) return;
            
            var wrap = node.GetNodeOrNull<Control>("Wrap");
            if (wrap == null) return;
            
            var box = wrap.GetNodeOrNull<ColorRect>("Box");
            if (box == null) return;
            
            var originalColor = box.Color;
            
            var tween = _gameBoard.CreateTween();
            tween.TweenProperty(box, "color", Colors.White, 0.05);
            tween.TweenProperty(box, "color", originalColor, 0.1);
        }
    }
}