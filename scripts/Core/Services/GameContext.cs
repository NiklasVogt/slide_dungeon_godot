// scripts/Core/Services/GameContext.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Objectives;
using Dungeon2048.Core.Spells;
using Dungeon2048.Core.Enemies;
using Dungeon2048.Core.World;
using Dungeon2048.Core.Tiles;
using Dungeon2048.Core.Progression;
using Dungeon2048.Core.Tiles;

namespace Dungeon2048.Core.Services
{
    public sealed class GameContext
    {
        public const int GridSize = 6;

        public Player Player = null!;
        public readonly List<Enemy> Enemies = new();
        public readonly List<Stone> Stones = new();
        public readonly List<SpellDrop> SpellDrops = new();
        public readonly List<Teleporter> Teleporters = new();
        public readonly List<RuneTrap> RuneTraps = new();
        public readonly List<MagicBarrier> MagicBarriers = new();
        public Door? Door;

        // Tile-Listen für Akt 1
        public readonly List<Gravestone> Gravestones = new();
        public readonly List<Torch> Torches = new();
        public readonly List<BonePile> BonePiles = new();

        // Tile-Listen für Akt 3: Vulkanschmiede
        public readonly List<FireTile> FireTiles = new();
        public readonly List<FallingRock> FallingRocks = new();

        public IObjective Objective = null!;
        public int CurrentLevel = 1;
        public int TotalSwipes = 0;
        public int TotalEnemiesKilled = 0;
        public bool EnemiesFrozen = false;

        public int HexCurseTurnsRemaining = 0; // Hex Witch Fluch
        public bool IsHexCursed => HexCurseTurnsRemaining > 0;

        // === 3. Lich-Magier Boss State ===
        public int LichPhase2SpawnCounter = 0;

        // Boss-State
        public int GoblinKingSpawnCounter = 0;

        public Random Rng = new();

        // Biome System
        public BiomeSystem BiomeSystem { get; private set; }

        public SoulManager SoulManager { get; private set; }

        public GameContext()
        {
            Rng = new Random();

            // NEU: Soul Manager initialisieren
            SoulManager = new SoulManager();
            SoulManager.ResetRunSouls();

            Player = new Player(0, 0);
            Player.MaxHp = Player.CalculatedMaxHp;
            Player.Hp = Player.MaxHp;
            Player.Atk = Player.CalculatedAtk;

            BiomeSystem = new BiomeSystem(this);
            BiomeSystem.UpdateBiome(CurrentLevel);

            PlacePlayerRandomly();
            SpawnInitialStones();
            Objective = ObjectiveService.Generate(Rng, CurrentLevel);
        }

        public void RegisterSwipe()
        {
            TotalSwipes += 1;
            Objective.OnSwipe();

            // Boss-Mechanik: NUR Goblin King spawnt Adds
            var goblinKing = Enemies.FirstOrDefault(e => e.Type == EnemyType.GoblinKing && e.IsBoss);
            if (goblinKing != null)
            {
                GoblinKingSpawnCounter++;

                // Adds spawnen nur für Goblin King
                if (GoblinKingSpawnCounter >= 3)
                {
                    SpawnGoblinKingAdds();
                    GoblinKingSpawnCounter = 0;
                }
            }

            // Boss spawnen wenn Boss-Objective und Swipe-Ziel erreicht
            if (Objective is BossObjective bo && !bo.BossSpawned && bo.Current >= bo.Target)
            {
                SpawnBoss();
                bo.BossSpawned = true;
            }

            if (HexCurseTurnsRemaining > 0)
            {
                HexCurseTurnsRemaining--;
                if (HexCurseTurnsRemaining == 0)
                {
                    GD.Print("✨ Der Hex-Fluch wurde gebrochen!");
                }
            }

            // Akt 3: 30% Chance pro Swipe für Falling Rock
            if (BiomeSystem.CurrentBiome?.Type == World.BiomeType.VolcanForge)
            {
                if (Rng.NextDouble() < 0.30) // 30% Chance
                {
                    if (BiomeSystem.CurrentBiome is World.VolcanForgeBiome vfb)
                    {
                        vfb.SpawnFallingRock(this);
                    }
                }
            }

            RegenerateMagicBarriers();
            HandleLichTeleport();
            UpdateMirrorKnights();
            AgeBonePiles();
            ProcessFireTiles();
            ProcessFallingRocks();
            HandleFireGiantMechanics();

            // NEU: Teleporter am Ende des Zuges verarbeiten
            ProcessTeleporters();

            CheckAndSpawnDoor();

            foreach (var gargoyle in Enemies.Where(e => e.Type == EnemyType.Gargoyle))
            {
                gargoyle.HasMoved = !gargoyle.HasMoved;
            }
        }



        // NEU: BonePile Aging und Revival
        private void AgeBonePiles()
        {
            var toRevive = new List<BonePile>();

            foreach (var pile in BonePiles)
            {
                pile.SwipesAlive++;

                if (pile.ShouldRevive)
                {
                    toRevive.Add(pile);
                }
            }

            // Revive Skeletons
            foreach (var pile in toRevive)
            {
                BonePiles.Remove(pile);

                // Spawne Skelett an der Position
                var level = CalculateEnemyLevel();
                var skeleton = EnemyRegistry.Get(EnemyType.Skeleton).Create(pile.X, pile.Y, level);

                // Biome Modifiers anwenden
                var biome = BiomeSystem.CurrentBiome;
                skeleton.Hp = (int)(skeleton.Hp * biome.EnemyHealthMultiplier);
                skeleton.Atk = (int)(skeleton.Atk * biome.EnemyDamageMultiplier);

                Enemies.Add(skeleton);
                Godot.GD.Print($"💀 Knochenhaufen erwacht als Skelett! 💀");
            }
        }

        public void ProcessTeleporters()
        {
            if (Teleporters.Count == 0) return;

            var teleportQueue = new System.Collections.Generic.List<(Entities.EntityBase entity, int targetX, int targetY)>();

            // 1. Sammle alle Entities auf Teleportern

            // Player prüfen
            var playerTeleporter = Teleporters.FirstOrDefault(t =>
                t.IsActive && t.X == Player.X && t.Y == Player.Y && t.LinkedTeleporterId != null
            );

            if (playerTeleporter != null)
            {
                var target = Teleporters.FirstOrDefault(t => t.Id == playerTeleporter.LinkedTeleporterId);
                if (target != null)
                {
                    teleportQueue.Add((Player, target.X, target.Y));
                }
            }

            // Enemies prüfen
            foreach (var enemy in Enemies.ToList())
            {
                var enemyTeleporter = Teleporters.FirstOrDefault(t =>
                    t.IsActive && t.X == enemy.X && t.Y == enemy.Y && t.LinkedTeleporterId != null
                );

                if (enemyTeleporter != null)
                {
                    var target = Teleporters.FirstOrDefault(t => t.Id == enemyTeleporter.LinkedTeleporterId);
                    if (target != null)
                    {
                        teleportQueue.Add((enemy, target.X, target.Y));
                    }
                }
            }

            // 2. Führe alle Teleportationen durch
            foreach (var (entity, targetX, targetY) in teleportQueue)
            {
                var oldPos = (entity.X, entity.Y);
                entity.X = targetX;
                entity.Y = targetY;

                string entityName = entity is Entities.Player ? "Spieler" :
                                   entity is Entities.Enemy e ? e.DisplayName : "Entity";

                Godot.GD.Print($"🌀 {entityName} teleportiert von ({oldPos.X},{oldPos.Y}) → ({targetX},{targetY})!");
            }
        }


        public void SpawnEnemies()
        {
            int count = CalculateEnemySpawnCount();
            var biome = BiomeSystem.CurrentBiome;

            for (int i = 0; i < count; i++)
            {
                var e = SpawnBiomeEnemy(biome);
                if (e != null)
                {
                    Enemies.Add(e);

                    // FEATURE: Goblin spawnt immer in Paaren (das bleibt!)
                    if (e.Type == EnemyType.Goblin)
                    {
                        var pos2 = RandomFreeCell();
                        var goblin2 = EnemyRegistry.Get(EnemyType.Goblin).Create(pos2.X, pos2.Y, e.EnemyLevel);
                        goblin2.Hp = (int)(goblin2.Hp * biome.EnemyHealthMultiplier);
                        goblin2.Atk = (int)(goblin2.Atk * biome.EnemyDamageMultiplier);
                        Enemies.Add(goblin2);
                        Godot.GD.Print("Goblin-Paar spawnt!");
                    }
                }
            }

            // Spell Drop (unverändert)
            if (Rng.NextDouble() < 0.08)
            {
                var pos = RandomFreeCell();
                SpellDrops.Add(new SpellDrop(pos.X, pos.Y, SpellFactory.CreateRandom(Player.Level, Rng)));
            }
        }


        private Enemy SpawnBiomeEnemy(IBiome biome)
        {
            // Bestimme ob Standard oder Rare
            bool isRare = Rng.NextDouble() < 0.15; // 15% Rare Chance

            var pool = isRare ? biome.RareEnemies : biome.StandardEnemies;
            if (pool.Count == 0) return null;

            var selectedType = pool[Rng.Next(pool.Count)];
            var archetype = EnemyRegistry.Get(selectedType);

            int level = archetype.CalcLevel(this);
            var pos = RandomFreeCell();

            var enemy = archetype.Create(pos.X, pos.Y, level);

            // Biome Modifiers anwenden
            enemy.Hp = (int)(enemy.Hp * biome.EnemyHealthMultiplier);
            enemy.Atk = (int)(enemy.Atk * biome.EnemyDamageMultiplier);

            return enemy;
        }

        private void SpawnGoblinKingAdds()
        {
            // Goblin King spawnt 2 Goblins
            for (int i = 0; i < 2; i++)
            {
                var pos = RandomFreeCell();
                var goblin = EnemyRegistry.Get(EnemyType.Goblin).Create(pos.X, pos.Y, CalculateEnemyLevel());
                Enemies.Add(goblin);
            }
            Godot.GD.Print("Goblin-König ruft Verstärkung!");
        }

        public void RegisterPlayerKill(Enemy e)
        {
            TotalEnemiesKilled += 1;
            Objective.OnKillEnemy(e);

            // NEU: Seelen für Kill
            int souls = SoulCurrency.GetSoulReward(e.Type, e.EnemyLevel, e.IsBoss);
            SoulManager.AddSouls(souls);
            if (e.Type == EnemyType.HexWitch)
            {
                HexCurseTurnsRemaining = 5;
                GD.Print("🔮 Die Hex-Hexe verflucht dich! Heilung ist invertiert für 5 Züge!");
            }
            // Skelett: 30% Chance auf Bone Pile
            if (e.Type == EnemyType.Skeleton && Rng.NextDouble() < 0.3)
            {
                BonePiles.Add(new BonePile(e.X, e.Y));
                Godot.GD.Print($"💀 Skelett hinterlässt Knochenhaufen! (Revival in {BonePile.MaxSwipesAlive} Zügen)");
            }

            // Necrophage Healing
            foreach (var necro in Enemies.Where(en => en.Type == EnemyType.Necrophage))
            {
                necro.Hp += 3;
                necro.HealedThisRound += 3;
                Godot.GD.Print($"Necrophage heilt sich um 3 HP! (jetzt {necro.Hp} HP)");
            }
            if (e.Type == EnemyType.SoulLeech)
            {
                Player.Atk = System.Math.Max(1, Player.Atk - 1);
                GD.Print($"💀 Soul Leech saugt deine Kraft! ATK: {Player.Atk}");
            }

            // Pyromaniac: Explodiert beim Tod
            if (e.Type == EnemyType.Pyromaniac)
            {
                HandlePyromaniacExplosion(e.X, e.Y);
            }

            if (e.IsBoss && Objective is BossObjective bo)
                bo.BossKilled = true;
        }

        public void RegisterEnemyKill(Enemy e)
        {
            Objective.OnKillEnemy(e);

            // Skelett Bones
            if (e.Type == EnemyType.Skeleton && Rng.NextDouble() < 0.3)
            {
                BonePiles.Add(new BonePile(e.X, e.Y));
            }

            // Necrophage
            foreach (var necro in Enemies.Where(en => en.Type == EnemyType.Necrophage))
            {
                necro.Hp += 3;
                necro.HealedThisRound += 3;
            }

            // Pyromaniac: Explodiert beim Tod (auch wenn von anderem Enemy getötet)
            if (e.Type == EnemyType.Pyromaniac)
            {
                HandlePyromaniacExplosion(e.X, e.Y);
            }
        }

        public void RegisterSpellPickup(SpellDrop drop)
        {
            if (Player.AddSpell(drop.Spell))
            {
                SpellDrops.Remove(drop);
            }
        }

        public void InteractWithDoor()
        {
            if (Door == null || !Door.IsActive) return;

            BiomeSystem.OnLevelComplete();

            // NEU: Seelen-Bonus für Level-Completion
            int levelBonus = SoulCurrency.GetLevelCompletionBonus(CurrentLevel);
            int objectiveBonus = SoulCurrency.GetObjectiveCompletionBonus(Objective.Type);
            int totalBonus = levelBonus + objectiveBonus;

            SoulManager.AddSouls(totalBonus);
            Godot.GD.Print($"✨ Level {CurrentLevel} abgeschlossen! Bonus: {totalBonus} Seelen");

            CurrentLevel += 1;
            TotalSwipes = 0;
            GoblinKingSpawnCounter = 0;

            Enemies.Clear();
            Stones.Clear();
            SpellDrops.Clear();
            Gravestones.Clear();
            Torches.Clear();
            BonePiles.Clear();
            Teleporters.Clear();
            RuneTraps.Clear();
            MagicBarriers.Clear();
            FireTiles.Clear();
            FallingRocks.Clear();
            HexCurseTurnsRemaining = 0;
            Door = null;

            Player.Hp = Player.MaxHp;

            BiomeSystem.UpdateBiome(CurrentLevel);
            Objective = ObjectiveService.Generate(Rng, CurrentLevel);
            SpawnInitialStones();
        }

        public (int X, int Y) RandomFreeCell(bool ignorePlayer = false)
        {
            var cells = new List<(int X, int Y)>();
            for (int x = 0; x < GridSize; x++)
                for (int y = 0; y < GridSize; y++)
                    cells.Add((x, y));
            var free = cells.Where(p => !IsOccupied(p.X, p.Y, ignorePlayer)).ToList();
            if (free.Count == 0) return (0, 0);
            return free[Rng.Next(free.Count)];
        }

        public bool IsOccupied(int x, int y, bool ignorePlayer = false)
        {
            if (!ignorePlayer && Player.X == x && Player.Y == y) return true;
            if (Enemies.Any(e => e.X == x && e.Y == y)) return true;
            if (Stones.Any(s => s.X == x && s.Y == y)) return true;
            if (SpellDrops.Any(sd => sd.X == x && sd.Y == y)) return true;
            if (Gravestones.Any(g => g.X == x && g.Y == y)) return true;
            if (Torches.Any(t => t.X == x && t.Y == y)) return true;
            if (BonePiles.Any(b => b.X == x && b.Y == y)) return true;
            if (Door != null && Door.X == x && Door.Y == y) return true;
            if (Teleporters.Any(t => t.X == x && t.Y == y && t.IsActive)) return false; // Teleporter blockieren nicht
            if (RuneTraps.Any(r => r.X == x && r.Y == y && !r.IsTriggered)) return false; // Fallen blockieren nicht
            if (MagicBarriers.Any(m => m.X == x && m.Y == y && !m.IsDestroyed)) return true;
            return false;
        }

        private void RegenerateMagicBarriers()
        {
            foreach (var barrier in MagicBarriers)
            {
                if (barrier.IsDestroyed)
                {
                    barrier.SwipesSinceBroken++;

                    if (barrier.ShouldRegenerate)
                    {
                        barrier.Reset();
                        GD.Print($"✨ Magische Barriere regeneriert bei ({barrier.X}, {barrier.Y})!");
                    }
                }
            }
        }

        private void HandleLichTeleport()
        {
            var lich = Enemies.FirstOrDefault(e => e.Type == EnemyType.LichMage && e.IsBoss);
            if (lich == null) return;

            if (lich.ShouldLichTeleport())
            {
                var pos = RandomFreeCell(ignorePlayer: true);
                lich.X = pos.X;
                lich.Y = pos.Y;
                lich.LichTeleportCounter = 0;
                GD.Print("🌀 Der Lich-Magier teleportiert sich!");

                // Phase 2 bei 50% HP
                if (!lich.IsPhase2 && lich.Hp <= lich.MaxHp / 2)
                {
                    lich.IsPhase2 = true;
                    SpawnLichKultists();
                }
            }
        }

        private void SpawnLichKultists()
        {
            GD.Print("⚡ Der Lich-Magier beschwört Kultisten!");

            for (int i = 0; i < 4; i++)
            {
                var pos = RandomFreeCell();
                var kultist = EnemyRegistry.Get(EnemyType.Kultist).Create(pos.X, pos.Y, CalculateEnemyLevel());

                // Biome Modifiers
                var biome = BiomeSystem.CurrentBiome;
                kultist.Hp = (int)(kultist.Hp * biome.EnemyHealthMultiplier);
                kultist.Atk = (int)(kultist.Atk * biome.EnemyDamageMultiplier);

                Enemies.Add(kultist);
            }
        }

        private void UpdateMirrorKnights()
        {
            foreach (var mirror in Enemies.Where(e => e.Type == EnemyType.MirrorKnight))
            {
                mirror.SyncMirrorKnightStats(Player);
            }
        }

        private void ProcessFireTiles()
        {
            // Moloch Heilung auf Feuer-Tiles
            foreach (var enemy in Enemies.Where(e => e.Type == EnemyType.Moloch))
            {
                var onFire = FireTiles.Any(f => !f.IsExtinguished && f.X == enemy.X && f.Y == enemy.Y);
                enemy.StandingOnFire = onFire;

                if (onFire)
                {
                    enemy.HealOnFire(5); // Heilt 5 HP pro Zug auf Feuer
                }
            }

            // Feuer-Tiles die gelöscht sind entfernen
            FireTiles.RemoveAll(f => f.IsExtinguished);
        }

        private void ProcessFallingRocks()
        {
            var rocksToProcess = FallingRocks.ToList();

            foreach (var rock in rocksToProcess)
            {
                // Wenn noch Warnung läuft, zähle runter
                if (rock.IsWarning)
                {
                    rock.AdvanceTurn();
                }
                // Wenn bereit zu fallen, verursache Schaden
                else if (rock.ShouldFall)
                {
                    rock.Fall();

                    // Schaden an Player wenn auf Position
                    if (Player.X == rock.X && Player.Y == rock.Y)
                    {
                        Player.Hp -= FallingRock.FallDamage;
                        GD.Print($"💥 Fels fällt auf dich! {FallingRock.FallDamage} Schaden!");
                    }

                    // Schaden an Enemies auf Position
                    var enemiesHit = Enemies.Where(e => e.X == rock.X && e.Y == rock.Y).ToList();
                    foreach (var enemy in enemiesHit)
                    {
                        enemy.Hp -= FallingRock.FallDamage;
                        GD.Print($"💥 Fels fällt auf {enemy.DisplayName}! {FallingRock.FallDamage} Schaden!");

                        if (enemy.Hp <= 0)
                        {
                            RegisterEnemyKill(enemy);
                            Enemies.Remove(enemy);
                        }
                    }

                    // Rock entfernen nachdem er gefallen ist
                    FallingRocks.Remove(rock);
                }
            }
        }

        private void HandleFireGiantMechanics()
        {
            var fireGiant = Enemies.FirstOrDefault(e => e.Type == EnemyType.FireGiant && e.IsBoss);
            if (fireGiant == null) return;

            // Alle 2 Swipes: Hammer-Schlag (Diagonal-Kreuz-Pattern wird zu Feuer)
            if (TotalSwipes % 2 == 0)
            {
                // Diagonal cross pattern: 4 diagonale Richtungen
                var diagonals = new[] {
                    (1, 1),   // Unten-Rechts
                    (1, -1),  // Oben-Rechts
                    (-1, 1),  // Unten-Links
                    (-1, -1)  // Oben-Links
                };

                GD.Print("🔥🔨 FEUERGIGANT schwingt seinen Hammer! 🔨🔥");

                foreach (var (dx, dy) in diagonals)
                {
                    for (int dist = 1; dist <= 2; dist++) // 2 Tiles weit
                    {
                        int targetX = fireGiant.X + (dx * dist);
                        int targetY = fireGiant.Y + (dy * dist);

                        // Bounds check
                        if (targetX < 0 || targetX >= GridSize || targetY < 0 || targetY >= GridSize)
                            continue;

                        // Spawn Feuer-Tile
                        if (!FireTiles.Any(f => f.X == targetX && f.Y == targetY))
                        {
                            FireTiles.Add(new FireTile(targetX, targetY));
                            GD.Print($"🔥 Hammer-Schlag erzeugt Lava bei ({targetX},{targetY})");
                        }
                    }
                }
            }

            // Phase 2: Bei 50% HP spawne Feuer-Elementare
            if (!fireGiant.IsPhase2 && fireGiant.Hp <= fireGiant.MaxHp / 2)
            {
                fireGiant.IsPhase2 = true;
                SpawnFireGiantElementals();
            }
        }

        private void SpawnFireGiantElementals()
        {
            GD.Print("🔥🔥 FEUERGIGANT PHASE 2! Er beschwört Feuer-Elementare! 🔥🔥");

            // Spawne 3 Feuer-Elementare
            for (int i = 0; i < 3; i++)
            {
                var pos = RandomFreeCell();
                var elemental = EnemyRegistry.Get(EnemyType.FireElemental).Create(pos.X, pos.Y, CalculateEnemyLevel() + 2);

                // Biome Modifiers
                var biome = BiomeSystem.CurrentBiome;
                elemental.Hp = (int)(elemental.Hp * biome.EnemyHealthMultiplier);
                elemental.Atk = (int)(elemental.Atk * biome.EnemyDamageMultiplier);

                Enemies.Add(elemental);
            }
        }

        private void HandlePyromaniacExplosion(int x, int y)
        {
            const int explosionDamage = 10;
            GD.Print($"💥 PYROMANIAC EXPLODIERT! ({x},{y})");

            // Schaden an allen Entities in 1-Tile-Radius (inkl. diagonal)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // Nicht die Explosion-Position selbst

                    int targetX = x + dx;
                    int targetY = y + dy;

                    // Bounds check
                    if (targetX < 0 || targetX >= GridSize || targetY < 0 || targetY >= GridSize)
                        continue;

                    // Schaden an Player
                    if (Player.X == targetX && Player.Y == targetY)
                    {
                        Player.Hp -= explosionDamage;
                        GD.Print($"💥 Explosion trifft Spieler! {explosionDamage} Schaden!");
                    }

                    // Schaden an Enemies
                    var enemiesHit = Enemies.Where(e => e.X == targetX && e.Y == targetY).ToList();
                    foreach (var enemy in enemiesHit)
                    {
                        enemy.Hp -= explosionDamage;
                        GD.Print($"💥 Explosion trifft {enemy.DisplayName}! {explosionDamage} Schaden!");

                        // Tote Enemies entfernen
                        if (enemy.Hp <= 0)
                        {
                            RegisterEnemyKill(enemy);
                            Enemies.Remove(enemy);
                        }
                    }
                }
            }

            // Feuer-Tile an Explosions-Position spawnen
            if (!FireTiles.Any(f => f.X == x && f.Y == y))
            {
                FireTiles.Add(new FireTile(x, y));
                GD.Print($"🔥 Explosion hinterlässt Feuer bei ({x},{y})");
            }
        }

        private void PlacePlayerRandomly()
        {
            var p = RandomFreeCell(ignorePlayer: true);
            Player.X = p.X; Player.Y = p.Y;
        }

        private void SpawnInitialStones()
        {
            int count = Rng.Next(1, 4);
            for (int i = 0; i < count; i++)
            {
                var p = RandomFreeCell();
                Stones.Add(new Stone(p.X, p.Y));
            }
        }

        private void CheckAndSpawnDoor()
        {
            if (Door != null && Door.IsActive) return;
            bool shouldSpawn = Objective.Type switch
            {
                LevelType.Survival or LevelType.Elimination => Objective.IsCompleted,
                LevelType.Boss => (Objective as BossObjective)?.BossKilled ?? false,
                _ => false
            };
            if (shouldSpawn) SpawnDoorAtEdge();
        }

        private void SpawnDoorAtEdge()
        {
            if (Door != null && Door.IsActive) return;
            var edges = new List<(int X, int Y)>();
            for (int x = 0; x < GridSize; x++) { edges.Add((x, 0)); edges.Add((x, GridSize - 1)); }
            for (int y = 0; y < GridSize; y++) { edges.Add((0, y)); edges.Add((GridSize - 1, y)); }
            var free = edges.Where(p => !IsOccupied(p.X, p.Y)).ToList();
            var pos = (free.Count > 0 ? free[Rng.Next(free.Count)] : edges[Rng.Next(edges.Count)]);

            Stones.RemoveAll(s => s.X == pos.X && s.Y == pos.Y);
            Gravestones.RemoveAll(g => g.X == pos.X && g.Y == pos.Y);
            BonePiles.RemoveAll(b => b.X == pos.X && b.Y == pos.Y);
            SpellDrops.RemoveAll(sd => sd.X == pos.X && sd.Y == pos.Y);
            Enemies.RemoveAll(e => e.X == pos.X && e.Y == pos.Y && !e.IsBoss);

            if (Player.X == pos.X && Player.Y == pos.Y)
            {
                var alt = RandomFreeCell(ignorePlayer: true);
                Player.X = alt.X; Player.Y = alt.Y;
            }
            Door = new Door(pos.X, pos.Y) { IsActive = true };
        }

        // Langsamer Enemy Level
        public int CalculateEnemyLevel()
        {
            int baseLvl = System.Math.Max(1, (int)((CurrentLevel - 1) / 3.0));
            int progressBonus = (int)(Objective.Progress * 1.0 + 0.5);
            int typeBonus = Objective.Type == LevelType.Boss ? 1 : 0;
            return baseLvl + progressBonus + typeBonus;
        }

        // Weniger Enemy Spawns
        public int CalculateEnemySpawnCount()
        {
            double doorMod = (Door != null && Door.IsActive) ? 0.3 : 1.0;
            var biome = BiomeSystem.CurrentBiome;
            double biomeMod = biome?.SpawnRateMultiplier ?? 1.0;

            // ÄNDERUNG: Immer nur 1 Gegner spawnen
            // doorMod kann das auf 0 reduzieren wenn Tür aktiv ist
            int baseCount = Objective.Type switch
            {
                LevelType.Survival => (int)System.Math.Round(1 * doorMod * biomeMod),
                LevelType.Elimination => (int)System.Math.Round(1 * doorMod * biomeMod),
                LevelType.Boss => (int)System.Math.Round(1 * doorMod * biomeMod),
                _ => 1
            };

            // Mindestens 0, maximal 1 (außer Door macht es zu 0)
            return System.Math.Clamp(baseCount, 0, 1);
        }


        private void SpawnBoss()
        {
            var biome = BiomeSystem.CurrentBiome;

            // Prüfe ob das aktuelle Level ein Boss-Level für dieses Biome ist
            if (biome.HasBoss(CurrentLevel))
            {
                var bossType = biome.GetBossType();
                var archetype = EnemyRegistry.Get(bossType);
                var pos = RandomFreeCell();
                var lvl = archetype.CalcLevel(this) + 2;
                var boss = archetype.Create(pos.X, pos.Y, lvl, true);
                Enemies.Add(boss);
                Godot.GD.Print($"🔥 ACT BOSS SPAWNED: {boss.DisplayName}! 🔥");
            }
            else
            {
                // Sollte nicht passieren, aber Fallback
                Godot.GD.PrintErr($"Versuch Boss zu spawnen aber Level {CurrentLevel} ist kein Boss-Level!");
            }
        }
    }
}