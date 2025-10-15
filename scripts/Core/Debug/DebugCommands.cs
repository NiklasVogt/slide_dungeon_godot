// scripts/Core/Debug/DebugCommands.cs
using Godot;
using Dungeon2048.Core.Services;
using Dungeon2048.Core.Entities;

namespace Dungeon2048.Core.Debug
{
    public static class DebugCommands
    {
        public static void SpawnTestEnemies(GameContext ctx)
        {
            GD.Print("=== Spawning Test Enemies (Akt 1) ===");
            
            // Spawn einen von jedem Typ
            var types = new[] 
            { 
                EnemyType.Goblin, 
                EnemyType.Skeleton, 
                EnemyType.Rat, 
                EnemyType.Necrophage, 
                EnemyType.Mimic 
            };
            
            foreach (var type in types)
            {
                var pos = ctx.RandomFreeCell();
                var enemy = new Enemy(pos.X, pos.Y, type, 1);
                ctx.Enemies.Add(enemy);
                GD.Print($"Spawned {enemy.DisplayName} at ({pos.X}, {pos.Y})");
            }
        }
        
        public static void JumpToLevel(GameContext ctx, int level)
        {
            ctx.CurrentLevel = level;
            ctx.BiomeSystem.UpdateBiome(level);
            GD.Print($"Jumped to Level {level} - Biome: {ctx.BiomeSystem.CurrentBiome.Name}");
        }
        
        public static void PrintBiomeInfo(GameContext ctx)
        {
            var biome = ctx.BiomeSystem.CurrentBiome;
            GD.Print("=== Biome Info ===");
            GD.Print($"Name: {biome.Name}");
            GD.Print($"Type: {biome.Type}");
            GD.Print($"Levels: {biome.StartLevel}-{biome.EndLevel}");
            GD.Print($"Standard Enemies: {string.Join(", ", biome.StandardEnemies)}");
            GD.Print($"Rare Enemies: {string.Join(", ", biome.RareEnemies)}");
            GD.Print($"HP Mult: {biome.EnemyHealthMultiplier}x");
            GD.Print($"DMG Mult: {biome.EnemyDamageMultiplier}x");
        }
        
        public static void SpawnBoss(GameContext ctx)
        {
            var biome = ctx.BiomeSystem.CurrentBiome;
            if (biome.HasBoss(ctx.CurrentLevel))
            {
                var pos = ctx.RandomFreeCell();
                var bossType = biome.GetBossType();
                var boss = new Enemy(pos.X, pos.Y, bossType, ctx.CalculateEnemyLevel() + 2, true);
                ctx.Enemies.Add(boss);
                GD.Print($"Boss spawned: {boss.DisplayName}!");
            }
            else
            {
                GD.Print("No boss available for this level!");
            }
        }
    }
}