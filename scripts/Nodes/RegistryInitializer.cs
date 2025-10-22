// scripts/Nodes/RegistryInitializer.cs
using Dungeon2048.Core.Tiles;
using Dungeon2048.Core.Enemies;
using Dungeon2048.Core.Spells;

namespace Dungeon2048.Nodes
{
    public static class RegistryInitializer
    {
        private static bool _initialized = false;

        public static void InitializeAllRegistries()
        {
            if (_initialized) return;

            RegisterTiles();
            RegisterEnemies();
            RegisterSpells();

            _initialized = true;
        }

        private static void RegisterTiles()
        {
            // Core Tiles
            TileRegistry.Register(new StoneTile());
            TileRegistry.Register(new DoorTile());
            TileRegistry.Register(new SpellDropTile());
            
            // Akt 1 Tiles
            TileRegistry.Register(new GravestoneTile());
            TileRegistry.Register(new TorchTile());
            TileRegistry.Register(new BonePileTile());
            
            // Akt 2 Tiles
            TileRegistry.Register(new TeleporterTile());
            // ENTFERNT: TileRegistry.Register(new RuneTrapTile());
            TileRegistry.Register(new MagicBarrierTile());
        }

        private static void RegisterEnemies()
        {
            // Akt 1
            EnemyRegistry.Register(new GoblinArch());
            EnemyRegistry.Register(new SkeletonArch());
            EnemyRegistry.Register(new RatArch());
            EnemyRegistry.Register(new NecrophageArch());
            EnemyRegistry.Register(new MimicArch());
            EnemyRegistry.Register(new GoblinKingArch());
            
            // Akt 2
            EnemyRegistry.Register(new OrcArch());
            EnemyRegistry.Register(new KultistArch());
            EnemyRegistry.Register(new GargoyleArch());
            EnemyRegistry.Register(new SoulLeechArch());
            EnemyRegistry.Register(new MirrorKnightArch());
            EnemyRegistry.Register(new HexWitchArch());
            EnemyRegistry.Register(new LichMageArch());
        }

        private static void RegisterSpells()
        {
            SpellRegistry.Register(SpellType.Fireball, new FireballBehavior());
            SpellRegistry.Register(SpellType.Heal, new HealBehavior());
            SpellRegistry.Register(SpellType.Freeze, new FreezeBehavior());
            SpellRegistry.Register(SpellType.Lightning, new LightningBehavior());
            SpellRegistry.Register(SpellType.Teleport, new TeleportBehavior());
        }
    }
}