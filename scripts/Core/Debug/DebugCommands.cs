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
            GD.Print("=== Spawning Test Enemies ===");
            
            // Prüfe welcher Akt
            int act = Objectives.ObjectiveService.GetActForLevel(ctx.CurrentLevel);
            
            if (act == 1)
            {
                GD.Print("Spawning Akt 1 Enemies...");
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
            else if (act == 2)
            {
                GD.Print("Spawning Akt 2 Enemies...");
                var types = new[]
                {
                    EnemyType.Orc,
                    EnemyType.Kultist,
                    EnemyType.Gargoyle,
                    EnemyType.SoulLeech,
                    EnemyType.MirrorKnight,
                    EnemyType.HexWitch
                };
                
                foreach (var type in types)
                {
                    var pos = ctx.RandomFreeCell();
                    var enemy = new Enemy(pos.X, pos.Y, type, ctx.CalculateEnemyLevel());
                    
                    // Mirror Knight braucht Sync
                    if (type == EnemyType.MirrorKnight)
                    {
                        enemy.SyncMirrorKnightStats(ctx.Player);
                    }
                    
                    ctx.Enemies.Add(enemy);
                    GD.Print($"Spawned {enemy.DisplayName} at ({pos.X}, {pos.Y})");
                }
            }
            else
            {
                GD.Print($"Akt {act} noch nicht implementiert!");
            }
        }
        
        public static void JumpToLevel(GameContext ctx, int level)
        {
            ctx.CurrentLevel = level;
            ctx.BiomeSystem.UpdateBiome(level);
            
            // Clear old entities
            ctx.Enemies.Clear();
            ctx.Stones.Clear();
            ctx.SpellDrops.Clear();
            ctx.Gravestones.Clear();
            ctx.Torches.Clear();
            ctx.BonePiles.Clear();
            ctx.Teleporters.Clear();
            ctx.RuneTraps.Clear();
            ctx.MagicBarriers.Clear();
            ctx.Door = null;
            
            // Reset Player HP
            ctx.Player.Hp = ctx.Player.MaxHp;
            
            // Generate new objective
            ctx.Objective = Objectives.ObjectiveService.Generate(ctx.Rng, level);
            
            // Spawn initial stones (wie bei normalem Level-Start)
            SpawnInitialStones(ctx);
            
            // Biome-spezifische Level-Start-Logik ausführen
            ctx.BiomeSystem.CurrentBiome?.OnLevelStart(ctx);
            
            GD.Print($"=== Jumped to Level {level} ===");
            GD.Print($"Biome: {ctx.BiomeSystem.CurrentBiome.Name}");
            GD.Print($"Objective: {ctx.Objective.Description}");
        }
        
        // Helper-Methode für Initial Stones
        private static void SpawnInitialStones(GameContext ctx)
        {
            int count = ctx.Rng.Next(1, 4);
            for (int i = 0; i < count; i++)
            {
                var p = ctx.RandomFreeCell();
                ctx.Stones.Add(new Entities.Stone(p.X, p.Y));
            }
            GD.Print($"Spawned {count} initial stones");
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
            GD.Print($"Spawn Rate: {biome.SpawnRateMultiplier}x");
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
                GD.Print($"Try jumping to boss level: {(Objectives.ObjectiveService.GetActForLevel(ctx.CurrentLevel) * 10)}");
            }
        }
        
        public static void HealPlayer(GameContext ctx)
        {
            ctx.Player.Hp = ctx.Player.MaxHp;
            GD.Print($"Player healed to full: {ctx.Player.Hp}/{ctx.Player.MaxHp} HP");
        }
        
        public static void GiveSouls(GameContext ctx, int amount)
        {
            ctx.SoulManager.AddSouls(amount);
            GD.Print($"Added {amount} souls. Total: {ctx.SoulManager.CurrentSouls}");
        }
        
        public static void SkipLevel(GameContext ctx)
        {
            if (ctx.Door != null && ctx.Door.IsActive)
            {
                ctx.InteractWithDoor();
                GD.Print("Skipped to next level!");
            }
            else
            {
                GD.Print("No door available! Complete objective first.");
            }
        }
        
        public static void ToggleHexCurse(GameContext ctx)
        {
            if (ctx.IsHexCursed)
            {
                ctx.HexCurseTurnsRemaining = 0;
                GD.Print("Hex Curse removed!");
            }
            else
            {
                ctx.HexCurseTurnsRemaining = 5;
                GD.Print("Hex Curse applied for 5 turns!");
            }
        }
        
        public static void SpawnTeleporters(GameContext ctx)
        {
            var pos1 = ctx.RandomFreeCell();
            var pos2 = FindValidTeleporterPosition(ctx, pos1);
            
            var t1 = new Tiles.Teleporter(pos1.X, pos1.Y);
            var t2 = new Tiles.Teleporter(pos2.X, pos2.Y);
            
            t1.LinkedTeleporterId = t2.Id;
            t2.LinkedTeleporterId = t1.Id;
            
            ctx.Teleporters.Add(t1);
            ctx.Teleporters.Add(t2);
            
            GD.Print($"Spawned teleporter pair at ({pos1.X},{pos1.Y}) <-> ({pos2.X},{pos2.Y})");
        }
        
        // Helper für gültige Teleporter-Position
        private static (int X, int Y) FindValidTeleporterPosition(GameContext ctx, (int X, int Y) firstPos)
        {
            const int maxAttempts = 50;
            int attempts = 0;
            
            while (attempts < maxAttempts)
            {
                var pos = ctx.RandomFreeCell();
                
                // Nicht auf gleicher X- oder Y-Achse
                if (pos.X != firstPos.X && pos.Y != firstPos.Y)
                {
                    return pos;
                }
                
                attempts++;
            }
            
            GD.PrintErr("⚠️ Konnte keine gültige Teleporter-Position finden (Debug)");
            return ctx.RandomFreeCell();
        }
    }
}