// scripts/Core/Managers/StatusEffectManager.cs
using Dungeon2048.Core.Systems;
using Dungeon2048.Core.World;

namespace Dungeon2048.Core.Managers
{
    /// <summary>
    /// Coordinates all status effect systems
    /// Follows Facade Pattern to provide unified interface to status effects
    /// </summary>
    public sealed class StatusEffectManager
    {
        public BurningSystem BurningSystem { get; }
        public ColdSystem ColdSystem { get; }
        public HexCurseSystem HexCurseSystem { get; }
        public FreezeSystem FreezeSystem { get; }

        public StatusEffectManager()
        {
            BurningSystem = new BurningSystem();
            ColdSystem = new ColdSystem();
            HexCurseSystem = new HexCurseSystem();
            FreezeSystem = new FreezeSystem();
        }

        /// <summary>
        /// Processes all status effects at the end of a swipe
        /// </summary>
        public void ProcessEndOfTurnEffects(EntityManager entityManager, TileManager tileManager, int totalSwipes, BiomeType? currentBiome)
        {
            // Burning effects
            BurningSystem.ApplyBurningFromFireTiles(entityManager, tileManager);
            BurningSystem.ProcessMolochHealing(entityManager, tileManager);

            // Cold effects (only in Frost Depths)
            ColdSystem.ProcessColdAccumulation(entityManager, totalSwipes, currentBiome);
            ColdSystem.ProcessGlacialSentinelAuras(entityManager, currentBiome);
            ColdSystem.ProcessPermafrostLichChill(entityManager, currentBiome);

            // Hex curse
            HexCurseSystem.ProcessCurse();

            // Freeze
            FreezeSystem.ProcessFreezeStatus(entityManager);
        }

        /// <summary>
        /// Processes cold damage (should be called after combat)
        /// </summary>
        public void ProcessColdDamage(EntityManager entityManager, BiomeType? currentBiome)
        {
            ColdSystem.ProcessColdDamage(entityManager, currentBiome);
        }

        /// <summary>
        /// Clears all status effects (used when advancing to next level)
        /// </summary>
        public void ClearAllEffects()
        {
            HexCurseSystem.ClearCurse();
            ColdSystem.ResetCounters();
        }
    }
}
