// scripts/Nodes/GameBoardRenderer_Helpers.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Dungeon2048.Core.Services;
using Dungeon2048.Core.Entities;

namespace Dungeon2048.Nodes
{
    /// <summary>
    /// Helper-Methoden für GameBoardRenderer (partial class)
    /// </summary>
    public sealed partial class GameBoardRenderer
    {
        private Node2D GetOrCreateEntityNode(string name, int z, Color color, int hp, 
            bool showBadge = false, string badgeText = "", string displayName = "")
        {
            if (_entityNodes.TryGetValue(name, out var node))
                return node;

            node = new Node2D { Name = name, ZIndex = z };
            var wrap = new Control 
            { 
                Name = "Wrap", 
                CustomMinimumSize = new Vector2(_layout.TileSize, _layout.TileSize) 
            };
            var box = new ColorRect 
            { 
                Name = "Box", 
                Color = color, 
                Size = new Vector2(_layout.TileSize, _layout.TileSize) 
            };
            wrap.AddChild(box);

            // HP Label (zentriert)
            var hpLbl = new Label { Name = "HP", Text = hp.ToString() };
            hpLbl.HorizontalAlignment = HorizontalAlignment.Center;
            hpLbl.VerticalAlignment = VerticalAlignment.Center;
            hpLbl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hpLbl.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            wrap.AddChild(hpLbl);

            // Badge (oben rechts)
            if (showBadge)
            {
                var badge = new Label { Name = "Badge", Text = badgeText };
                badge.Position = new Vector2(_layout.TileSize - 22, 2);
                badge.AddThemeFontSizeOverride("font_size", 14);
                wrap.AddChild(badge);
            }
            
            // Display Name (unten mitte)
            if (!string.IsNullOrEmpty(displayName))
            {
                var nameLabel = new Label { Name = "DisplayName", Text = displayName };
                nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
                nameLabel.Position = new Vector2(0, _layout.TileSize - 18);
                nameLabel.Size = new Vector2(_layout.TileSize, 18);
                nameLabel.AddThemeFontSizeOverride("font_size", 10);
                nameLabel.AddThemeColorOverride("font_color", Colors.White);
                nameLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
                nameLabel.AddThemeConstantOverride("outline_size", 2);
                wrap.AddChild(nameLabel);
            }

            node.AddChild(wrap);
            _entitiesNode.AddChild(node);
            _entityNodes[name] = node;
            return node;
        }

        private void UpdateEntityNodeVisuals(string name, int hp, string? badgeText = null, string? displayName = null)
        {
            if (!_entityNodes.TryGetValue(name, out var node)) return;
            var wrap = node.GetNodeOrNull<Control>("Wrap");
            if (wrap == null) return;

            wrap.CustomMinimumSize = new Vector2(_layout.TileSize, _layout.TileSize);
            var box = wrap.GetNodeOrNull<ColorRect>("Box");
            if (box != null) box.Size = new Vector2(_layout.TileSize, _layout.TileSize);

            var hpLbl = wrap.GetNodeOrNull<Label>("HP");
            if (hpLbl != null) hpLbl.Text = hp.ToString();

            if (badgeText != null)
            {
                var badge = wrap.GetNodeOrNull<Label>("Badge");
                if (badge != null)
                {
                    badge.Text = badgeText;
                    badge.Position = new Vector2(_layout.TileSize - 22, 2);
                }
            }
            
            if (displayName != null)
            {
                var nameLabel = wrap.GetNodeOrNull<Label>("DisplayName");
                if (nameLabel != null)
                {
                    nameLabel.Text = displayName;
                    nameLabel.Position = new Vector2(0, _layout.TileSize - 18);
                    nameLabel.Size = new Vector2(_layout.TileSize, 18);
                }
            }
        }

        private void SlideNodeTo(Node2D node, Vector2 target)
        {
            var dist = (node.Position - target).Length();
            double duration = Mathf.Clamp(dist / Mathf.Max(1f, _layout.TileSize) * 0.07, 0.08, 0.25);
            if (dist < 0.01f) return;
            
            var tw = _gameBoard.CreateTween();
            tw.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
            tw.TweenProperty(node, "position", target, duration);
        }

        private void PruneMissingNodes(GameContext ctx)
        {
            var alive = new HashSet<string> { "Player" };
            
            foreach (var e in ctx.Enemies) alive.Add($"Enemy_{e.Id}");
            foreach (var s in ctx.Stones) alive.Add($"Stone_{s.Id}");
            foreach (var g in ctx.Gravestones) alive.Add($"Gravestone_{g.Id}");
            foreach (var t in ctx.Torches) alive.Add($"Torch_{t.Id}");
            foreach (var b in ctx.BonePiles) alive.Add($"BonePile_{b.Id}");
            foreach (var d in ctx.SpellDrops) alive.Add($"Spell_{d.Id}");
            foreach (var t in ctx.Teleporters.Where(t => t.IsActive)) alive.Add($"Teleporter_{t.Id}");
            foreach (var r in ctx.RuneTraps.Where(r => !r.IsTriggered)) alive.Add($"RuneTrap_{r.Id}");
            foreach (var m in ctx.MagicBarriers) alive.Add($"MagicBarrier_{m.Id}");
            
            if (ctx.Door != null && ctx.Door.IsActive) alive.Add("Door");

            foreach (var kv in _entityNodes.ToArray())
            {
                if (!alive.Contains(kv.Key))
                {
                    kv.Value.QueueFree();
                    _entityNodes.Remove(kv.Key);
                }
            }
        }

        private Color GetEnemyColor(EnemyType type)
        {
            return type switch
            {
                // Akt 1
                EnemyType.Goblin       => new Color("3cb44b"),
                EnemyType.Skeleton     => new Color("e6d5b8"),
                EnemyType.Rat          => new Color("6b5b4d"),
                EnemyType.Necrophage   => new Color("663399"),
                EnemyType.Mimic        => new Color("dc143c"),
                EnemyType.GoblinKing   => new Color("0a5f0a"),
                
                // Akt 2
                EnemyType.Orc          => new Color("f58231"),
                EnemyType.Kultist      => new Color("000080"),
                EnemyType.Gargoyle     => new Color("808080"),
                EnemyType.SoulLeech    => new Color("add8e6"),
                EnemyType.MirrorKnight => new Color("c0c0c0"),
                EnemyType.HexWitch     => new Color("9370db"),
                EnemyType.LichMage     => new Color("4b0082"),
                
                // Sonstige
                EnemyType.Dragon       => new Color("911eb4"),
                EnemyType.Boss         => new Color("111111"),
                EnemyType.Masochist    => new Color("4699e1"),
                EnemyType.Thorns       => new Color("22aa22"),
                _                      => new Color("ff5f5f")
            };
        }

        private string GetEnemyBadge(Enemy e)
        {
            if (e.IsBoss)
                return "👑";
            
            if (e.Type == EnemyType.Necrophage && e.HealedThisRound > 0)
                return $"+{e.HealedThisRound}";
            
            if (e.Type == EnemyType.Gargoyle)
                return e.HasMoved ? "⚡" : "🗿";
            
            return e.EnemyLevel.ToString();
        }

        public void AttackFx(string attackerName, string targetName, Vector2I dir)
        {
            if (!_entityNodes.TryGetValue(attackerName, out var attackerNode))
                return;

            var origin = attackerNode.Position;
            var nudge = new Vector2(dir.X, dir.Y) * (_layout.TileSize * 0.25f);
            var tw = _gameBoard.CreateTween();
            tw.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            tw.TweenProperty(attackerNode, "position", origin + nudge, 0.06);
            tw.TweenProperty(attackerNode, "position", origin, 0.08)
                .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);

            if (_entityNodes.TryGetValue(targetName, out var targetNode))
            {
                var hit = _gameBoard.CreateTween();
                hit.TweenProperty(targetNode, "scale", new Vector2(1.06f, 1.06f), 0.05);
                hit.TweenProperty(targetNode, "scale", new Vector2(1f, 1f), 0.08);
                
                var wrap = targetNode.GetNodeOrNull<Control>("Wrap");
                if (wrap != null)
                {
                    var shake = _gameBoard.CreateTween();
                    shake.TweenProperty(wrap, "position", wrap.Position + new Vector2(3, 0), 0.03);
                    shake.TweenProperty(wrap, "position", wrap.Position + new Vector2(-3, 0), 0.03);
                    shake.TweenProperty(wrap, "position", wrap.Position, 0.03);
                }
            }
        }

        // Kultist Attack Animation in GameBoardRenderer_Animation.cs ...
    }
}