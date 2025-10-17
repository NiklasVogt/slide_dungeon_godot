// scripts/Core/World/CatacombsBiome.cs
using System;
using System.Collections.Generic;
using Godot;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Services;
using Dungeon2048.Core.Objectives;

namespace Dungeon2048.Core.World
{
    public sealed class CatacombsBiome : IBiome
    {
        public BiomeType Type => BiomeType.Catacombs;
        public string Name => "Die Katakomben";
        public string Description => "Verfallene Grabstätten voller untoten Kreaturen";
        public int StartLevel => 1;
        public int EndLevel => 10;
        
        public Color BackgroundColor => new Color(0.08f, 0.08f, 0.12f);
        public Color GridColor => new Color(0.25f, 0.2f, 0.15f);
        public Color AmbientColor => new Color(0.6f, 0.5f, 0.4f);
        
        public float EnemyHealthMultiplier => 1.0f;
        public float EnemyDamageMultiplier => 1.0f;
        public float SpawnRateMultiplier => 1.0f;
        
        public List<EnemyType> StandardEnemies => new()
        {
            EnemyType.Goblin,
            EnemyType.Skeleton,
            EnemyType.Rat
        };
        
        public List<EnemyType> RareEnemies => new()
        {
            EnemyType.Necrophage,
            EnemyType.Mimic
        };
        
        public string[] SpecialTileTypes => new[] { "Gravestone", "Torch" };
        
        public void OnEnter(GameContext ctx)
        {
            GD.Print($"Betrete {Name}");
            SpawnGravestones(ctx);
        }
        
        public void OnLevelStart(GameContext ctx)
        {
            // Boss-Level: Keine zusätzlichen Fackeln, mehr Spannung
            if (ObjectiveService.IsBossLevel(ctx.CurrentLevel))
            {
                GD.Print("⚔️ Bereite dich auf den Boss vor! ⚔️");
                return;
            }
            
            // Normale Level: Optional Fackeln
            if (ctx.Rng.NextDouble() < 0.3)
            {
                SpawnTorch(ctx);
            }
        }
        
        public void OnLevelComplete(GameContext ctx)
        {
            // Bonus nur für sehr schnelles Clearing
            if (ctx.TotalSwipes < 6)
            {
                GD.Print("Blitzschnell! Bonus XP!");
                ctx.Player.GainExperience(15);
            }
        }
        
        public void OnExit(GameContext ctx)
        {
            GD.Print($"Verlasse {Name}");
        }
        
        public bool HasBoss(int level) => level == 10;
        
        public EnemyType GetBossType() => EnemyType.GoblinKing;
        
        private void SpawnGravestones(GameContext ctx)
        {
            int count = ctx.Rng.Next(2, 4);
            for (int i = 0; i < count; i++)
            {
                var pos = ctx.RandomFreeCell();
                ctx.Gravestones.Add(new Tiles.Gravestone(pos.X, pos.Y));
            }
        }
        
        private void SpawnTorch(GameContext ctx)
        {
            var pos = ctx.RandomFreeCell();
            ctx.Torches.Add(new Tiles.Torch(pos.X, pos.Y));
        }
    }
}