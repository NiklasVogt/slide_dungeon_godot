using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Tiles;
using Dungeon2048.Core.Enemies;
using Godot;

namespace Dungeon2048.Core.Services
{
    public static class MovementPipeline
    {
        private const int MasochistMoveDamagePerCell = 1;

        private static int ThornsRetaliationForPlayer(Player p, Enemy thorns)
        {
            return 3 + p.Level / 2;
        }

        private static int ThornsRetaliationForEnemy(Enemy attacker, Enemy thorns)
        {
            return 2 + thorns.EnemyLevel / 2;
        }

        private static Vector2I CalculateFurthest(GameContext ctx, EntityBase entity, int dx, int dy, HashSet<string> occupied)
        {
            int x = entity.X, y = entity.Y;
            while (true)
            {
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || nx >= GameContext.GridSize || ny < 0 || ny >= GameContext.GridSize) break;
                if (TileRegistry.AnyBlocks(entity, ctx, nx, ny)) break;
                if (occupied.Contains($"{nx},{ny}")) break;
                x = nx; y = ny;
            }
            return new Vector2I(x, y);
        }

        private static List<EntityBase> SortForDirection(IEnumerable<EntityBase> entities, int dx, int dy)
        {
            var list = entities.ToList();
            list.Sort((a, b) =>
            {
                if (dx > 0) { if (a.Y == b.Y) return b.X.CompareTo(a.X); return a.Y.CompareTo(b.Y); }
                if (dx < 0) { if (a.Y == b.Y) return a.X.CompareTo(b.X); return a.Y.CompareTo(b.Y); }
                if (dy > 0) { if (a.X == b.X) return b.Y.CompareTo(a.Y); return a.X.CompareTo(b.X); }
                if (a.X == b.X) return a.Y.CompareTo(b.Y); return a.X.CompareTo(b.X);
            });
            return list;
        }

        private static void SweepEnterForPlayer(GameContext ctx, Player player, int startX, int startY, int endX, int endY, int dx, int dy)
        {
            int x = startX, y = startY;
            while (x != endX || y != endY)
            {
                x += dx; y += dy;
                TileRegistry.Enter(player, ctx, x, y);
            }
        }

        private static void ResolveAfterMove(GameContext ctx, EventBus bus, EntityBase moved, int dx, int dy, HashSet<string> occupied, int startX, int startY)
        {
            TileRegistry.Enter(moved, ctx, moved.X, moved.Y);

            if (moved is Player pl && (moved.X != startX || moved.Y != startY) && (dx != 0 || dy != 0))
            {
                SweepEnterForPlayer(ctx, pl, startX, startY, moved.X, moved.Y, dx, dy);
            }

            int tx = moved.X + dx, ty = moved.Y + dy;
            if (tx < 0 || tx >= GameContext.GridSize || ty < 0 || ty >= GameContext.GridSize) return;

            var d = ctx.Door;
            if (d != null && d.IsActive && d.X == tx && d.Y == ty)
            {
                var oldKey = $"{moved.X},{moved.Y}";
                occupied.Remove(oldKey);
                moved.X = tx; moved.Y = ty;
                occupied.Add($"{moved.X},{moved.Y}");
                TileRegistry.Enter(moved, ctx, moved.X, moved.Y);
                return;
            }

            TileRegistry.Hit(moved, ctx, tx, ty);

            if (!TileRegistry.AnyBlocks(moved, ctx, tx, ty) && !occupied.Contains($"{tx},{ty}"))
            {
                var oldKey = $"{moved.X},{moved.Y}";
                occupied.Remove(oldKey);
                moved.X = tx; moved.Y = ty;
                occupied.Add($"{moved.X},{moved.Y}");
                TileRegistry.Enter(moved, ctx, moved.X, moved.Y);

                if (moved is Player pl2)
                {
                    SweepEnterForPlayer(ctx, pl2, moved.X - dx, moved.Y - dy, moved.X, moved.Y, dx, dy);
                }
                return;
            }

            // Player -> Enemy (mit Mimic-Reveal)
            if (moved is Player player)
            {
                int eidx = ctx.Enemies.FindIndex(e => e.X == tx && e.Y == ty);
                if (eidx != -1)
                {
                    var target = ctx.Enemies[eidx];
                    
                    // Mimic reveal beim ersten Kontakt
                    if (target.Type == EnemyType.Mimic && target.IsDisguised)
                    {
                        target.IsDisguised = false;
                        GD.Print("Das war ein MIMIC!");
                    }
                    
                    bus.AddAttackEvent(new AttackEvent("Player", $"Enemy_{target.Id}", new Vector2I(dx, dy)));

                    if (target.Type == EnemyType.Masochist)
                    {
                        // Kein Kollisionsschaden
                    }
                    else
                    {
                        target.Hp -= moved.Atk;
                    }

                    if (target.Type == EnemyType.Thorns)
                    {
                        int refl = ThornsRetaliationForPlayer(player, target);
                        ctx.Player.Hp -= refl;
                    }

                    if (target.Hp <= 0)
                    {
                        var oldPx = (moved.X, moved.Y);
                        var ex = target.X; var ey = target.Y;
                        var xp = target.XpReward;

                        ctx.RegisterPlayerKill(target);
                        ctx.Enemies.RemoveAt(eidx);

                        occupied.Remove($"{oldPx.Item1},{oldPx.Item2}");
                        occupied.Remove($"{ex},{ey}");

                        moved.X = ex; moved.Y = ey;
                        occupied.Add($"{moved.X},{moved.Y}");

                        ctx.Player.GainExperience(xp);
                        TileRegistry.Enter(moved, ctx, moved.X, moved.Y);
                    }
                    return;
                }
            }

            // Enemy -> Player / Enemy
            if (moved is Enemy enemy && !ctx.EnemiesFrozen)
            {
                if (ctx.Player.X == tx && ctx.Player.Y == ty)
                {
                    if (enemy.Type != EnemyType.Thorns)
                    {
                        bus.AddAttackEvent(new AttackEvent($"Enemy_{enemy.Id}", "Player", new Vector2I(dx, dy)));
                        ctx.Player.Hp -= enemy.Atk;
                    }
                    return;
                }

                int eidx = ctx.Enemies.FindIndex(e => e.X == tx && e.Y == ty && !ReferenceEquals(e, enemy));
                if (eidx != -1)
                {
                    var target = ctx.Enemies[eidx];

                    if (target.Type != EnemyType.Masochist)
                    {
                        target.Hp -= enemy.Atk;
                    }

                    if (target.Type == EnemyType.Thorns)
                    {
                        int refl = ThornsRetaliationForEnemy(enemy, target);
                        enemy.Hp -= refl;
                    }

                    if (target.Hp <= 0)
                    {
                        var oldEx = (enemy.X, enemy.Y);
                        var targetX = target.X; var targetY = target.Y;

                        ctx.RegisterEnemyKill(target);
                        ctx.Enemies.RemoveAt(eidx);

                        occupied.Remove($"{oldEx.Item1},{oldEx.Item2}");
                        occupied.Remove($"{targetX},{targetY}");

                        enemy.X = targetX; enemy.Y = targetY;
                        occupied.Add($"{enemy.X},{enemy.Y}");

                        TileRegistry.Enter(enemy, ctx, enemy.X, enemy.Y);
                    }

                    if (enemy.Hp <= 0)
                    {
                        var idxSelf = ctx.Enemies.FindIndex(e => ReferenceEquals(e, enemy));
                        if (idxSelf >= 0) ctx.Enemies.RemoveAt(idxSelf);
                        occupied.Remove($"{enemy.X},{enemy.Y}");
                        ctx.RegisterEnemyKill(enemy);
                    }

                    return;
                }
            }
        }

        public static async Task<List<AttackEvent>> Move(GameContext ctx, int dx, int dy)
        {
            var bus = new EventBus();

            var entitiesToMove = ctx.EnemiesFrozen
                ? new List<EntityBase> { ctx.Player }
                : new List<EntityBase> { ctx.Player }.Concat(ctx.Enemies.Cast<EntityBase>()).ToList();

            if (ctx.EnemiesFrozen) GD.Print("Freeze aktiv: nur Spieler bewegt sich.");

            var ordered = SortForDirection(entitiesToMove, dx, dy);

            var occupied = new HashSet<string>(new[] { $"{ctx.Player.X},{ctx.Player.Y}" }.Concat(ctx.Enemies.Select(e => $"{e.X},{e.Y}")));

            foreach (var entity in ordered)
            {
                if (entity is Enemy en && !ctx.Enemies.Contains(en)) continue;

                int startX = entity.X, startY = entity.Y;

                occupied.Remove($"{entity.X},{entity.Y}");
                var pos = CalculateFurthest(ctx, entity, dx, dy, occupied);
                entity.X = pos.X; entity.Y = pos.Y;
                occupied.Add($"{entity.X},{entity.Y}");

                ResolveAfterMove(ctx, bus, entity, dx, dy, occupied, startX, startY);

                // Masochist Schaden
                if (entity is Enemy en2 && en2.Type == EnemyType.Masochist)
                {
                    int dist = System.Math.Abs(en2.X - startX) + System.Math.Abs(en2.Y - startY);
                    if (dist > 0)
                    {
                        en2.Hp -= System.Math.Max(1, MasochistMoveDamagePerCell * dist);
                        if (en2.Hp <= 0)
                        {
                            var idx = ctx.Enemies.FindIndex(e => ReferenceEquals(e, en2));
                            if (idx >= 0) ctx.Enemies.RemoveAt(idx);
                            occupied.Remove($"{en2.X},{en2.Y}");
                            ctx.RegisterEnemyKill(en2);
                        }
                    }
                }

                await Task.Delay(150);
            }

            if (ctx.EnemiesFrozen)
            {
                ctx.EnemiesFrozen = false;
                GD.Print("Freeze Ende: Gegner bewegen sich wieder.");
            }

            return bus.Attacks;
        }
    }
}