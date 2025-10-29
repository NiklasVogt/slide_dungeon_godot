// scripts/Core/Components/PyromaniacComponent.cs
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Managers;
using Godot;

namespace Dungeon2048.Core.Components
{
    /// <summary>
    /// Pyromaniac Component: Explodes on death, dealing area damage and creating fire
    /// </summary>
    public sealed class PyromaniacComponent : IEnemyComponent
    {
        private const int ExplosionDamage = 10;

        public void Update(Enemy enemy, EntityManager entityManager, TileManager tileManager)
        {
            // No per-turn update
        }

        public void OnTakeDamage(Enemy enemy, int damage)
        {
            // No special behavior
        }

        public void OnDealDamage(Enemy enemy, EntityBase target, int damage)
        {
            // No special behavior
        }

        public void OnMove(Enemy enemy, int oldX, int oldY, int newX, int newY)
        {
            // No special behavior
        }

        public void OnDeath(Enemy enemy, EntityManager entityManager, TileManager tileManager)
        {
            Explode(enemy.X, enemy.Y, entityManager, tileManager);
        }

        private void Explode(int x, int y, EntityManager entityManager, TileManager tileManager)
        {
            GD.Print($"💥 PYROMANIAC EXPLODIERT! ({x},{y})");

            // Damage all entities in 1-tile radius (including diagonal)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // Not the explosion position itself

                    int targetX = x + dx;
                    int targetY = y + dy;

                    // Bounds check
                    if (targetX < 0 || targetX >= 6 || targetY < 0 || targetY >= 6)
                        continue;

                    // Damage Player
                    if (entityManager.Player.X == targetX && entityManager.Player.Y == targetY)
                    {
                        entityManager.Player.Hp -= ExplosionDamage;
                        GD.Print($"💥 Explosion trifft Spieler! {ExplosionDamage} Schaden!");
                    }

                    // Damage Enemies
                    var enemiesHit = entityManager.Enemies.Where(e => e.X == targetX && e.Y == targetY).ToList();
                    foreach (var enemy in enemiesHit)
                    {
                        enemy.Hp -= ExplosionDamage;
                        GD.Print($"💥 Explosion trifft {enemy.DisplayName}! {ExplosionDamage} Schaden!");

                        // Remove dead enemies (caller must handle kill tracking)
                        if (enemy.Hp <= 0)
                        {
                            entityManager.RemoveEnemy(enemy);
                        }
                    }
                }
            }

            // Create fire tile at explosion position
            tileManager.AddFireTile(x, y);
            GD.Print($"🔥 Explosion hinterlässt Feuer bei ({x},{y})");
        }
    }
}
