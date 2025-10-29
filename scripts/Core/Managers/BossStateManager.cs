// scripts/Core/Managers/BossStateManager.cs
using System;
using System.Collections.Generic;
using Dungeon2048.Core.Boss;
using Dungeon2048.Core.Entities;

namespace Dungeon2048.Core.Managers
{
    /// <summary>
    /// Manages boss state machines
    /// Coordinates boss behavior and phase transitions
    /// </summary>
    public sealed class BossStateManager
    {
        private readonly Dictionary<int, IBossState> _bossStates = new();
        private readonly Random _rng;

        public BossStateManager(Random rng)
        {
            _rng = rng;
        }

        /// <summary>
        /// Registers a boss and initializes its state
        /// </summary>
        public void RegisterBoss(Enemy boss)
        {
            if (_bossStates.ContainsKey(boss.Id))
                return;

            IBossState? state = boss.Type switch
            {
                EnemyType.GoblinKing => new GoblinKingState(_rng),
                EnemyType.LichMage => new LichMagePhase1State(_rng),
                EnemyType.FireGiant => new FireGiantPhase1State(_rng),
                EnemyType.IceDragon => new IceDragonPhase1State(_rng),
                _ => null
            };

            if (state != null)
            {
                _bossStates[boss.Id] = state;
            }
        }

        /// <summary>
        /// Updates all active boss states
        /// </summary>
        public void UpdateBosses(EntityManager entityManager, TileManager tileManager)
        {
            var bossesToUpdate = new List<(Enemy boss, IBossState state)>();

            // Collect bosses
            foreach (var enemy in entityManager.Enemies)
            {
                if (enemy.IsBoss && _bossStates.ContainsKey(enemy.Id))
                {
                    bossesToUpdate.Add((enemy, _bossStates[enemy.Id]));
                }
            }

            // Update and check transitions
            foreach (var (boss, state) in bossesToUpdate)
            {
                state.Update(entityManager, tileManager);

                // Check for phase transition
                var newState = state.CheckTransition(boss);
                if (newState != null)
                {
                    _bossStates[boss.Id] = newState;
                }
            }
        }

        /// <summary>
        /// Removes a boss state when boss dies
        /// </summary>
        public void UnregisterBoss(int bossId)
        {
            _bossStates.Remove(bossId);
        }

        /// <summary>
        /// Clears all boss states (used when advancing to next level)
        /// </summary>
        public void ClearAllBossStates()
        {
            _bossStates.Clear();
        }
    }
}
