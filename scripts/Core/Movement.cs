using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Dungeon2048.Core
{
    public static class Movement
    {
        public struct AttackEvent
        {
            public string Attacker;
            public string Target;
            public Vector2I Dir;
            public AttackEvent(string a, string t, Vector2I d){ Attacker=a; Target=t; Dir=d; }
        }

        private static int SpellIndexAt(List<SpellDrop> drops, int x, int y)
            => drops.FindIndex(d => d.X == x && d.Y == y);

        private static bool HasSpellAt(List<SpellDrop> drops, int x, int y)
            => SpellIndexAt(drops, x, y) != -1;

        private static Vector2I CalculateFurthestPosition(
            EntityBase entity, int dx, int dy,
            HashSet<string> occupied, List<Stone> stones, Door? door, List<SpellDrop> spellDrops)
        {
            int x = entity.X, y = entity.Y;
            while (true)
            {
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || nx >= GameState.GridSize || ny < 0 || ny >= GameState.GridSize) break;
                if (door != null && door.IsActive && door.X == nx && door.Y == ny) break;
                if (stones.Any(s => s.X == nx && s.Y == ny)) break;

                if (HasSpellAt(spellDrops, nx, ny))
                {
                    if (entity is Player) { x = nx; y = ny; }
                    break;
                }

                if (occupied.Contains($"{nx},{ny}")) break;
                x = nx; y = ny;
            }
            return new Vector2I(x, y);
        }

        private static List<EntityBase> SortEntitiesForDirection(IEnumerable<EntityBase> entities, int dx, int dy)
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

        private static void ResolveImmediateCollisionAfterMove(
            GameState gs, EntityBase moved, int dx, int dy, Action<Action> setState, HashSet<string> occupied,
            List<AttackEvent> events)
        {
            if (moved is Player)
            {
                int hereSpellIdx = SpellIndexAt(gs.SpellDrops, moved.X, moved.Y);
                if (hereSpellIdx != -1)
                {
                    var drop = gs.SpellDrops[hereSpellIdx];
                    setState(() =>
                    {
                        gs.RegisterSpellPickup(drop);
                    });
                    return;
                }

                int tx = moved.X + dx, ty = moved.Y + dy;
                if (tx < 0 || tx >= GameState.GridSize || ty < 0 || ty >= GameState.GridSize) return;

                var door = gs.Door;
                if (door != null && door.IsActive && door.X == tx && door.Y == ty)
                {
                    int oldPx = moved.X, oldPy = moved.Y;
                    setState(() =>
                    {
                        occupied.Remove($"{oldPx},{oldPy}");
                        moved.X = tx; moved.Y = ty;
                        occupied.Add($"{moved.X},{moved.Y}");
                        gs.InteractWithDoor();
                    });
                    return;
                }

                int stoneIdx = gs.Stones.FindIndex(s => s.X == tx && s.Y == ty);
                if (stoneIdx != -1)
                {
                    var stone = gs.Stones[stoneIdx];
                    stone.HitCount++;
                    if (stone.IsDestroyed)
                    {
                        int oldPx = moved.X, oldPy = moved.Y;
                        setState(() =>
                        {
                            gs.Stones.RemoveAll(s => s.IsDestroyed);
                            occupied.Remove($"{oldPx},{oldPy}");
                            moved.X = tx; moved.Y = ty;
                            occupied.Add($"{moved.X},{moved.Y}");
                        });
                    }
                    else setState(() => { });
                    return;
                }

                int eidx = gs.Enemies.FindIndex(e => e.X == tx && e.Y == ty);
                if (eidx != -1)
                {
                    var target = gs.Enemies[eidx];
                    events.Add(new AttackEvent("Player", $"Enemy_{target.Id}", new Vector2I(dx, dy)));

                    target.Hp -= moved.Atk;
                    if (target.Hp <= 0)
                    {
                        int oldPx = moved.X, oldPy = moved.Y;
                        int ex = target.X, ey = target.Y;
                        int xp = target.XpReward;
                        setState(() =>
                        {
                            gs.RegisterPlayerKill(target);
                            gs.Enemies.RemoveAt(eidx);
                            occupied.Remove($"{oldPx},{oldPy}");
                            occupied.Remove($"{ex},{ey}");
                            moved.X = ex; moved.Y = ey;
                            occupied.Add($"{moved.X},{moved.Y}");
                            gs.Player.GainExperience(xp);
                        });
                    }
                    else setState(() => { });
                    return;
                }
            }

            if (moved is Enemy enemy && !gs.EnemiesFrozen)
            {
                int tx = moved.X + dx, ty = moved.Y + dy;
                if (tx < 0 || tx >= GameState.GridSize || ty < 0 || ty >= GameState.GridSize) return;

                if (gs.Player.X == tx && gs.Player.Y == ty)
                {
                    events.Add(new AttackEvent($"Enemy_{enemy.Id}", "Player", new Vector2I(dx, dy)));

                    gs.Player.Hp -= enemy.Atk;
                    setState(() => { });
                    return;
                }

                int spellIdx = SpellIndexAt(gs.SpellDrops, tx, ty);
                if (spellIdx != -1)
                {
                    var drop = gs.SpellDrops[spellIdx];
                    setState(() =>
                    {
                        drop.HitCount++;
                        if (drop.IsDestroyed) gs.SpellDrops.RemoveAt(spellIdx);
                    });
                    return;
                }

                int eidx = gs.Enemies.FindIndex(e => e.X == tx && e.Y == ty && !ReferenceEquals(e, enemy));
                if (eidx != -1)
                {
                    var target = gs.Enemies[eidx];
                    target.Hp -= enemy.Atk;
                    if (target.Hp <= 0)
                    {
                        int oldEx = enemy.X, oldEy = enemy.Y;
                        int targetX = target.X, targetY = target.Y;
                        setState(() =>
                        {
                            gs.RegisterEnemyKill(target);
                            gs.Enemies.RemoveAt(eidx);
                            occupied.Remove($"{oldEx},{oldEy}");
                            occupied.Remove($"{targetX},{targetY}");
                            enemy.X = targetX; enemy.Y = targetY;
                            occupied.Add($"{enemy.X},{enemy.Y}");
                        });
                    }
                    else setState(() => { });
                    return;
                }
            }
        }

        public static async Task<List<AttackEvent>> MoveEntitiesWithImmediateCollision(
            GameState gs, int dx, int dy, Action<Action> setState)
        {
            var events = new List<AttackEvent>();

            var entitiesToMove = gs.EnemiesFrozen
                ? new List<EntityBase> { gs.Player }
                : new List<EntityBase> { gs.Player }.Concat(gs.Enemies.Cast<EntityBase>()).ToList();

            if (gs.EnemiesFrozen) GD.Print("Freeze aktiv: nur Spieler bewegt sich.");

            var ordered = SortEntitiesForDirection(entitiesToMove, dx, dy);
            var occupied = new HashSet<string>(
                new[] { $"{gs.Player.X},{gs.Player.Y}" }.Concat(gs.Enemies.Select(e => $"{e.X},{e.Y}"))
            );

            foreach (var entity in ordered)
            {
                if (entity is Enemy en && !gs.Enemies.Contains(en)) continue;
                occupied.Remove($"{entity.X},{entity.Y}");
                var pos = CalculateFurthestPosition(entity, dx, dy, occupied, gs.Stones, gs.Door, gs.SpellDrops);
                entity.X = pos.X; entity.Y = pos.Y;
                occupied.Add($"{entity.X},{entity.Y}");
                setState(() => { });

                ResolveImmediateCollisionAfterMove(gs, entity, dx, dy, setState, occupied, events);
                await Task.Delay(150);
            }

            if (gs.EnemiesFrozen)
            {
                gs.EnemiesFrozen = false;
                GD.Print("Freeze Ende: Gegner bewegen sich wieder.");
            }

            return events;
        }
    }
}
