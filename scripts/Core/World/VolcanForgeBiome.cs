// scripts/Core/World/VolcanForgeBiome.cs
using System.Collections.Generic;
using Godot;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Services;
using Dungeon2048.Core.Objectives;
using Dungeon2048.Core.Tiles;

namespace Dungeon2048.Core.World
{
    public sealed class VolcanForgeBiome : IBiome
    {
        public BiomeType Type => BiomeType.VolcanForge;
        public string Name => "Die Vulkanschmiede";
        public string Description => "Glühende Lava und industrielle Schmieden";
        public int StartLevel => 21;
        public int EndLevel => 30;

        // Vulkan-Farbschema: Orange, Rot, Glühend
        public Color BackgroundColor => new Color(0.15f, 0.05f, 0.05f); // Dunkelrot
        public Color GridColor => new Color(0.4f, 0.15f, 0.1f); // Orange-Rot
        public Color AmbientColor => new Color(1.0f, 0.4f, 0.0f); // Orange Glühen

        public float EnemyHealthMultiplier => 1.4f; // Deutlich stärker als Akt 2
        public float EnemyDamageMultiplier => 1.5f;
        public float SpawnRateMultiplier => 0.85f; // Weniger Gegner, höhere Qualität

        public List<EnemyType> StandardEnemies => new()
        {
            EnemyType.FireElemental,
            EnemyType.Moloch,
            EnemyType.SchmiedGolem
        };

        public List<EnemyType> RareEnemies => new()
        {
            EnemyType.Thorns,           // Bereits vorhanden - perfekt für Vulkan
            EnemyType.Pyromaniac,
            EnemyType.ObsidianWarrior,
            EnemyType.ForgeMaster
        };

        public string[] SpecialTileTypes => new[] { "FireTile", "FallingRocks" };

        public void OnEnter(GameContext ctx)
        {
            GD.Print($"🔥 Betrete {Name} 🔥");
            GD.Print("Die Hitze ist unerträglich! Vorsicht vor Lava und explodierenden Feinden!");
        }

        public void OnLevelStart(GameContext ctx)
        {
            // Boss-Level: Keine speziellen Tiles, aber bereits Lava vorhanden
            if (ObjectiveService.IsBossLevel(ctx.CurrentLevel))
            {
                GD.Print("🔥 Der Feuergigant wartet in seiner Schmiede... 🔥");
                // Boss Arena hat bereits 3-5 Feuer-Tiles
                SpawnFireTiles(ctx, ctx.Rng.Next(3, 6));
                return;
            }

            // Normale Level: 2-4 Feuer-Tiles
            int fireCount = ctx.Rng.Next(2, 5);
            SpawnFireTiles(ctx, fireCount);

            // GEÄNDERT: Keine initialen Falling Rocks mehr
            // Stattdessen 30% Chance pro Swipe in GameContext.RegisterSwipe()
        }

        public void OnLevelComplete(GameContext ctx)
        {
            // Bonus wenn der Spieler nie Burning hatte
            // TODO: Implementieren wenn Burning-Tracking hinzugefügt wird
            GD.Print("Level geschafft! Die Glut erlischt...");
        }

        public void OnExit(GameContext ctx)
        {
            GD.Print($"Verlasse {Name}");
            GD.Print("Die Hitze lässt nach, aber die Narben bleiben...");
        }

        public bool HasBoss(int level) => level == 30;

        public EnemyType GetBossType() => EnemyType.FireGiant;

        private void SpawnFireTiles(GameContext ctx, int count)
        {
            GD.Print($"🔥 Spawne {count} Feuer-Tiles");

            for (int i = 0; i < count; i++)
            {
                var pos = ctx.RandomFreeCell();
                ctx.FireTiles.Add(new FireTile(pos.X, pos.Y));
            }
        }

        public void SpawnFallingRock(GameContext ctx)
        {
            // Öffentliche Methode für dynamisches Spawnen während des Spiels
            var pos = ctx.RandomFreeCell();
            ctx.FallingRocks.Add(new FallingRock(pos.X, pos.Y, warningTurnsRemaining: 1));
            GD.Print($"🪨 Fallender Fels erscheint bei ({pos.X},{pos.Y})!");
        }
    }
}
