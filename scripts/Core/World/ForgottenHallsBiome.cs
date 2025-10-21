// scripts/Core/World/ForgottenHallsBiome.cs
using System.Collections.Generic;
using Godot;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Services;
using Dungeon2048.Core.Objectives;
using Dungeon2048.Core.Tiles;

namespace Dungeon2048.Core.World
{
    public sealed class ForgottenHallsBiome : IBiome
    {
        public BiomeType Type => BiomeType.ForgottenHalls;
        public string Name => "Die Vergessenen Hallen";
        public string Description => "Magische Bibliotheken voller arkaner Energie";
        public int StartLevel => 11;
        public int EndLevel => 20;
        
        // Dunkle Bibliothek mit magischem Glühen
        public Color BackgroundColor => new Color(0.05f, 0.05f, 0.15f);
        public Color GridColor => new Color(0.2f, 0.15f, 0.3f);
        public Color AmbientColor => new Color(0.5f, 0.4f, 0.7f); // Lila Schimmer
        
        public float EnemyHealthMultiplier => 1.2f; // Stärker als Akt 1
        public float EnemyDamageMultiplier => 1.3f;
        public float SpawnRateMultiplier => 0.9f; // Weniger aber stärker
        
        public List<EnemyType> StandardEnemies => new()
        {
            EnemyType.Orc,
            EnemyType.Kultist,
            EnemyType.Gargoyle
        };
        
        public List<EnemyType> RareEnemies => new()
        {
            EnemyType.SoulLeech,
            EnemyType.MirrorKnight,
            EnemyType.HexWitch
        };
        
        public string[] SpecialTileTypes => new[] { "Teleporter", "RuneTrap", "MagicBarrier" };
        
        public void OnEnter(GameContext ctx)
        {
            GD.Print($"✨ Betrete {Name} ✨");
            GD.Print("Vorsicht vor magischen Fallen und fernkämpfenden Kultisten!");
            SpawnMagicBarriers(ctx);
        }
        
        public void OnLevelStart(GameContext ctx)
        {
            // Boss-Level: Keine Teleporter
            if (ObjectiveService.IsBossLevel(ctx.CurrentLevel))
            {
                GD.Print("🔮 Der Lich-Magier erwartet dich... 🔮");
                return;
            }
            
            // Teleporter spawnen (30% Chance)
            if (ctx.Rng.NextDouble() < 0.3)
            {
                SpawnTeleporterPair(ctx);
            }
            
            // Runen-Fallen spawnen (40% Chance)
            if (ctx.Rng.NextDouble() < 0.4)
            {
                SpawnRuneTraps(ctx);
            }
        }
        
        public void OnLevelComplete(GameContext ctx)
        {
            // Bonus für keine ATK-Verluste (Soul Leech)
            if (ctx.Player.Atk >= ctx.Player.CalculatedAtk)
            {
                GD.Print("Deine Stärke bleibt unberührt! Bonus XP!");
                ctx.Player.GainExperience(20);
            }
        }
        
        public void OnExit(GameContext ctx)
        {
            GD.Print($"Verlasse {Name}");
            
            // ATK-Verluste von Soul Leeches bleiben permanent!
            if (ctx.Player.Atk < ctx.Player.CalculatedAtk)
            {
                GD.Print($"⚠️ Soul Leeches haben deine Kraft geschwächt! ATK: {ctx.Player.Atk}");
            }
        }
        
        public bool HasBoss(int level) => level == 20;
        
        public EnemyType GetBossType() => EnemyType.LichMage;
        
        private void SpawnMagicBarriers(GameContext ctx)
        {
            int count = ctx.Rng.Next(2, 4);
            for (int i = 0; i < count; i++)
            {
                var pos = ctx.RandomFreeCell();
                ctx.MagicBarriers.Add(new MagicBarrier(pos.X, pos.Y));
            }
        }
        
        private void SpawnTeleporterPair(GameContext ctx)
        {
            var pos1 = ctx.RandomFreeCell();
            var pos2 = ctx.RandomFreeCell();
            
            var teleporter1 = new Teleporter(pos1.X, pos1.Y);
            var teleporter2 = new Teleporter(pos2.X, pos2.Y);
            
            // Verlinken
            teleporter1.LinkedTeleporterId = teleporter2.Id;
            teleporter2.LinkedTeleporterId = teleporter1.Id;
            
            ctx.Teleporters.Add(teleporter1);
            ctx.Teleporters.Add(teleporter2);
            
            GD.Print("🌀 Teleporter-Paar gespawnt!");
        }
        
        private void SpawnRuneTraps(GameContext ctx)
        {
            int count = ctx.Rng.Next(2, 5);
            for (int i = 0; i < count; i++)
            {
                var pos = ctx.RandomFreeCell();
                ctx.RuneTraps.Add(new RuneTrap(pos.X, pos.Y));
            }
            GD.Print($"⚡ {count} Runen-Fallen platziert!");
        }
    }
}