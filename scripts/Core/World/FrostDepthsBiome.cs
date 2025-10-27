// scripts/Core/World/FrostDepthsBiome.cs
using System.Collections.Generic;
using Godot;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Services;
using Dungeon2048.Core.Objectives;
using Dungeon2048.Core.Tiles;

namespace Dungeon2048.Core.World
{
    public sealed class FrostDepthsBiome : IBiome
    {
        public BiomeType Type => BiomeType.FrostDepths;
        public string Name => "Die Frostigen Tiefen";
        public string Description => "Eisige Kälte und gefrorene Abgründe";
        public int StartLevel => 31;
        public int EndLevel => 40;

        // Frost-Farbschema: Eisblau, Weiß-Blau
        public Color BackgroundColor => new Color(0.05f, 0.1f, 0.2f); // Dunkel Eisblau
        public Color GridColor => new Color(0.6f, 0.7f, 0.9f); // Hell Blau-Grau
        public Color AmbientColor => new Color(0.7f, 0.8f, 1.0f); // Eisiges Weiß-Blau

        public float EnemyHealthMultiplier => 1.5f; // Noch stärker als Akt 3
        public float EnemyDamageMultiplier => 1.6f;
        public float SpawnRateMultiplier => 0.8f; // Weniger aber mächtiger

        public List<EnemyType> StandardEnemies => new()
        {
            EnemyType.FrostGoblin,
            EnemyType.Yeti,
            EnemyType.IceShard
        };

        public List<EnemyType> RareEnemies => new()
        {
            EnemyType.Frostbite,
            EnemyType.Snowblind,
            EnemyType.GlacialSentinel,
            EnemyType.PermafrostLich
        };

        public string[] SpecialTileTypes => new[] { "Campfire", "Torch" };

        public void OnEnter(GameContext ctx)
        {
            GD.Print($"❄️ Betrete {Name} ❄️");
            GD.Print("Die Kälte durchdringt deine Knochen! Halte dich warm, oder erfriere!");
        }

        public void OnLevelStart(GameContext ctx)
        {
            // Boss-Level: Besondere Setup
            if (ObjectiveService.IsBossLevel(ctx.CurrentLevel))
            {
                GD.Print("❄️ Das Gefrorene Herz des Eisdrachen schlägt... ❄️");
                // Boss Arena: 3 Campfires, 2 Torches
                SpawnCampfires(ctx, 3);
                SpawnTorches(ctx, 2);
                return;
            }

            // Normale Level: 2-3 Campfires spawnen
            int campfireCount = ctx.Rng.Next(2, 4);
            SpawnCampfires(ctx, campfireCount);

            // 2 Torches spawnen
            SpawnTorches(ctx, 2);

            GD.Print($"❄️ Level-Start: {campfireCount} Campfires, 2 Torches");
        }

        public void OnLevelComplete(GameContext ctx)
        {
            GD.Print("Level geschafft! Die Kälte weicht für einen Moment...");
        }

        public void OnExit(GameContext ctx)
        {
            GD.Print($"Verlasse {Name}");
            GD.Print("Die Wärme kehrt zurück, aber die Kälte bleibt in deinen Knochen...");
        }

        public bool HasBoss(int level) => level == 40;

        public EnemyType GetBossType() => EnemyType.IceDragon;

        private void SpawnCampfires(GameContext ctx, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var pos = ctx.RandomFreeCell();

                // 10% Chance für Frostbite Mimic statt normales Campfire
                // TODO: Implementieren wenn Frostbite Mimic fertig ist
                // Für jetzt: Nur normale Campfires
                ctx.Campfires.Add(new CampfireTile(pos.X, pos.Y));
                GD.Print($"🔥 Campfire gespawned bei ({pos.X},{pos.Y})");
            }
        }

        private void SpawnTorches(GameContext ctx, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var pos = ctx.RandomFreeCell();
                ctx.Torches.Add(new Torch(pos.X, pos.Y));
                GD.Print($"🔦 Torch gespawned bei ({pos.X},{pos.Y})");
            }
        }
    }
}
