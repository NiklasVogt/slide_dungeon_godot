// scripts/Core/World/FrostDepthsBiome.cs
using System.Collections.Generic;
using System.Linq;
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
        public string Description => "Tödliche Kälte und eisige Bedrohungen";
        public int StartLevel => 31;
        public int EndLevel => 40;

        // Eis-Farbschema: Blau, Weiß, Eisig
        public Color BackgroundColor => new Color(0.05f, 0.1f, 0.2f); // Dunkelblau
        public Color GridColor => new Color(0.3f, 0.5f, 0.7f); // Hellblau
        public Color AmbientColor => new Color(0.6f, 0.8f, 1.0f); // Eisblau Glimmer

        public float EnemyHealthMultiplier => 1.6f; // Noch stärker als Akt 3
        public float EnemyDamageMultiplier => 1.7f;
        public float SpawnRateMultiplier => 0.8f; // Weniger aber gefährlicher

        public List<EnemyType> StandardEnemies => new()
        {
            EnemyType.FrostGoblin,      // Frost-Goblin (5 stack resistance)
            EnemyType.Yeti,             // Schneewanderer (7 stack resistance, drains campfire)
            EnemyType.IceShard,         // Eiswolf (immune to cold)
            EnemyType.Frostbite         // Frostbite Wraith (+4 stacks, flies)
        };

        public List<EnemyType> RareEnemies => new()
        {
            EnemyType.Snowblind,        // Blizzard Shaman (8 stacks, drains campfire every 3 turns)
            EnemyType.GlacialSentinel,  // Permafrost Golem (immobile, aura damage)
            EnemyType.PermafrostLich,   // Chill Touch Stalker (stationary, axis damage)
            EnemyType.FrostbiteMimic    // Frostbite Mimic (fake campfire)
        };

        public string[] SpecialTileTypes => new[] { "CampfireTile" };

        public void OnEnter(GameContext ctx)
        {
            GD.Print($"🧊 Betrete {Name} 🧊");
            GD.Print("Die Kälte kriecht in deine Knochen! Bleib in der Nähe von Wärmequellen!");
        }

        public void OnLevelStart(GameContext ctx)
        {
            if (ObjectiveService.IsBossLevel(ctx.CurrentLevel))
            {
                GD.Print("🧊❄️ Das Gefrorene Herz wartet... ❄️🧊");
            }

            // ALLE Level: Genau 1 Lagerfeuer + 2 Fackeln
            SpawnCampfires(ctx, 1);
            SpawnFrostTorches(ctx, 2);
        }

        public void OnLevelComplete(GameContext ctx)
        {
            // Bonus wenn der Spieler wenig Frostbite hatte
            if (ctx.Player.FrostbiteStacks <= 2)
            {
                GD.Print("✨ Du hast der Kälte widerstanden! Bonus!");
                // TODO: Seelen-Bonus oder HP-Bonus
            }
            GD.Print("Level geschafft! Die Kälte weicht zurück...");
        }

        public void OnExit(GameContext ctx)
        {
            GD.Print($"Verlasse {Name}");
            GD.Print("Das Eis schmilzt, aber die Erinnerung bleibt gefroren...");

            // Reset Frostbite beim Verlassen des Bioms
            ctx.Player.FrostbiteStacks = 0;
            foreach (var enemy in ctx.Enemies)
            {
                enemy.FrostbiteStacks = 0;
            }
        }

        public bool HasBoss(int level) => level == 40;

        public EnemyType GetBossType() => EnemyType.IceDragon; // "Das Gefrorene Herz"

        private void SpawnCampfires(GameContext ctx, int count)
        {
            GD.Print($"🔥 Spawne {count} Lagerfeuer");

            for (int i = 0; i < count; i++)
            {
                var pos = ctx.RandomFreeCell();
                ctx.CampfireTiles.Add(new CampfireTile(pos.X, pos.Y));
            }
        }

        private void SpawnFrostTorches(GameContext ctx, int count)
        {
            GD.Print($"🔥 Spawne {count} Fackeln");

            for (int i = 0; i < count; i++)
            {
                var pos = ctx.RandomFreeCell();
                ctx.FrostTorches.Add(new FrostTorch(pos.X, pos.Y));
            }
        }

        /// <summary>
        /// Respawnt ein Lagerfeuer an einer neuen zufälligen Position
        /// Wird aufgerufen wenn ein Lagerfeuer erlischt
        /// </summary>
        public void RespawnCampfire(GameContext ctx, CampfireTile extinguishedCampfire)
        {
            // Entferne das erloschene Lagerfeuer
            ctx.CampfireTiles.Remove(extinguishedCampfire);

            // Spawne ein neues an zufälliger Position (im nächsten Zug)
            var pos = ctx.RandomFreeCell();
            var newCampfire = new CampfireTile(pos.X, pos.Y);
            ctx.CampfireTiles.Add(newCampfire);

            GD.Print($"🔥 Ein neues Lagerfeuer entzündet sich bei ({pos.X},{pos.Y})!");
        }

        /// <summary>
        /// Prozessiert die Frostbite-Mechaniken am Ende jedes Zuges
        /// Wird in GameContext.RegisterSwipe() aufgerufen
        /// </summary>
        public void ProcessFrostbiteSystem(GameContext ctx)
        {
            // 1. Ambient Cold: +1 Stack alle 3 Züge
            ctx.AmbientColdTurnCounter++;

            int ambientColdStacks = 0;
            if (ctx.AmbientColdTurnCounter >= 3)
            {
                ambientColdStacks = 1;
                ctx.AmbientColdTurnCounter = 0;
            }

            // 2. Night Phase: Doppelte Kälte-Rate ab Zug 30
            if (ctx.IsNightPhase)
            {
                ambientColdStacks *= 2;
                if (ctx.TotalSwipes == 30)
                {
                    GD.Print("🌙 Die NACHT bricht an! Kälte-Rate verdoppelt! 🌙");
                }
            }

            // 3. Frostwind Event: +2 Stacks alle 10 Züge
            ctx.FrostwindEventCounter++;
            if (ctx.FrostwindEventCounter >= 10)
            {
                GD.Print("❄️💨 FROSTWIND! Alle Entities bekommen +2 Kälte-Stacks! 💨❄️");
                ApplyFrostbiteToAll(ctx, 2);
                ctx.FrostwindEventCounter = 0;
            }

            // 4. Ambient Cold anwenden
            if (ambientColdStacks > 0)
            {
                GD.Print($"❄️ Umgebungskälte: +{ambientColdStacks} Stack für alle");
                ApplyFrostbiteToAll(ctx, ambientColdStacks);
            }

            // 5. Prüfe auf Tod durch Frostbite
            CheckFrostbiteDeath(ctx);

            // 6. Campfire Respawn System
            ProcessCampfireRespawn(ctx);

            // 7. Spezial-Gegner Mechaniken
            ProcessSpecialEnemyMechanics(ctx);
        }

        private void ApplyFrostbiteToAll(GameContext ctx, int stacks)
        {
            // Spieler
            ApplyFrostbiteToEntity(ctx.Player, stacks, "Spieler");

            // Alle Gegner (außer immune)
            foreach (var enemy in ctx.Enemies.ToList())
            {
                // Eiswolf (IceShard) ist immun
                if (enemy.Type == EnemyType.IceShard)
                    continue;

                // Permafrost Golem (GlacialSentinel) ist immun
                if (enemy.Type == EnemyType.GlacialSentinel)
                    continue;

                // Boss ist immun
                if (enemy.Type == EnemyType.IceDragon)
                    continue;

                ApplyFrostbiteToEntity(enemy, stacks, enemy.DisplayName);
            }
        }

        private void ApplyFrostbiteToEntity(EntityBase entity, int stacks, string name)
        {
            if (entity is Player player)
            {
                player.FrostbiteStacks += stacks;
                if (player.FrostbiteStacks > 10)
                    player.FrostbiteStacks = 10;
            }
            else if (entity is Enemy enemy)
            {
                enemy.FrostbiteStacks += stacks;
                // Cap at resistance
                if (enemy.FrostbiteStacks > enemy.FrostbiteResistance)
                    enemy.FrostbiteStacks = enemy.FrostbiteResistance;
            }
        }

        private void CheckFrostbiteDeath(GameContext ctx)
        {
            // Spieler Tod bei 10 Stacks
            if (ctx.Player.FrostbiteStacks >= 10)
            {
                ctx.Player.Hp = 0;
                GD.Print("💀❄️ Du bist ERFROREN! 10 Kälte-Stacks erreicht! ❄️💀");
            }

            // Gegner Tod bei Resistance erreicht
            var deadEnemies = ctx.Enemies.Where(e => e.FrostbiteStacks >= e.FrostbiteResistance).ToList();
            foreach (var enemy in deadEnemies)
            {
                enemy.Hp = 0;
                GD.Print($"❄️💀 {enemy.DisplayName} ist ERFROREN! ({enemy.FrostbiteStacks}/{enemy.FrostbiteResistance} Stacks) 💀❄️");
            }
        }

        private void ProcessCampfireRespawn(GameContext ctx)
        {
            // Entferne erloschene Campfires aus der Tracking-Liste und spawne neue
            var toRespawn = ctx.ExtinguishedCampfires.ToList();
            foreach (var campfire in toRespawn)
            {
                ctx.ExtinguishedCampfires.Remove(campfire);
                RespawnCampfire(ctx, campfire);
            }
        }

        private void ProcessSpecialEnemyMechanics(GameContext ctx)
        {
            // Schneewanderer (Yeti): Entzieht Lagerfeuer-Ladung bei angrenzender Position
            foreach (var yeti in ctx.Enemies.Where(e => e.Type == EnemyType.Yeti).ToList())
            {
                foreach (var campfire in ctx.CampfireTiles.ToList())
                {
                    if (campfire.IsAdjacent(yeti.X, yeti.Y) && !campfire.IsExtinguished)
                    {
                        campfire.ConsumeCharge();
                        GD.Print($"❄️ {yeti.DisplayName} entzieht Lagerfeuer-Wärme! ({campfire.Charges} Ladungen verbleibend)");

                        if (campfire.IsExtinguished)
                        {
                            ctx.ExtinguishedCampfires.Add(campfire);
                        }
                        break; // Nur ein Lagerfeuer pro Zug
                    }
                }
            }

            // Blizzard Shaman (Snowblind): Drain Lagerfeuer alle 3 Züge
            foreach (var shaman in ctx.Enemies.Where(e => e.Type == EnemyType.Snowblind).ToList())
            {
                shaman.CampfireDrainCounter++;
                if (shaman.CampfireDrainCounter >= 3)
                {
                    shaman.CampfireDrainCounter = 0;

                    // Finde nächstes Lagerfeuer und drain es
                    var nearestCampfire = ctx.CampfireTiles
                        .Where(c => !c.IsExtinguished)
                        .OrderBy(c => System.Math.Abs(c.X - shaman.X) + System.Math.Abs(c.Y - shaman.Y))
                        .FirstOrDefault();

                    if (nearestCampfire != null)
                    {
                        nearestCampfire.ConsumeCharge();
                        GD.Print($"🌨️ {shaman.DisplayName} entzieht Lagerfeuer-Energie aus der Ferne!");

                        if (nearestCampfire.IsExtinguished)
                        {
                            ctx.ExtinguishedCampfires.Add(nearestCampfire);
                        }
                    }
                }
            }

            // Permafrost Golem (GlacialSentinel): Kälte-Aura +1 Stack/Zug für angrenzende Entities
            foreach (var golem in ctx.Enemies.Where(e => e.Type == EnemyType.GlacialSentinel).ToList())
            {
                // Prüfe Spieler
                if (IsAdjacent(golem.X, golem.Y, ctx.Player.X, ctx.Player.Y))
                {
                    ctx.Player.FrostbiteStacks++;
                    GD.Print($"🧊 {golem.DisplayName} Aura: Spieler +1 Kälte-Stack");
                }

                // Prüfe andere Gegner
                foreach (var enemy in ctx.Enemies.Where(e => e.Type != EnemyType.GlacialSentinel
                    && e.Type != EnemyType.IceShard
                    && e.Type != EnemyType.IceDragon).ToList())
                {
                    if (IsAdjacent(golem.X, golem.Y, enemy.X, enemy.Y))
                    {
                        enemy.FrostbiteStacks++;
                        if (enemy.FrostbiteStacks > enemy.FrostbiteResistance)
                            enemy.FrostbiteStacks = enemy.FrostbiteResistance;
                    }
                }
            }

            // Chill Touch Stalker (PermafrostLich): +2 Stacks wenn auf gleicher Achse
            foreach (var stalker in ctx.Enemies.Where(e => e.Type == EnemyType.PermafrostLich).ToList())
            {
                // Prüfe ob Spieler auf gleicher Achse mit freier Sichtlinie ist
                if (IsOnSameAxis(stalker.X, stalker.Y, ctx.Player.X, ctx.Player.Y))
                {
                    if (HasLineOfSight(ctx, stalker.X, stalker.Y, ctx.Player.X, ctx.Player.Y))
                    {
                        ctx.Player.FrostbiteStacks += 2;
                        GD.Print($"👻 {stalker.DisplayName} projiziert Kälte: Spieler +2 Stacks");
                    }
                }
            }
        }

        private bool IsAdjacent(int x1, int y1, int x2, int y2)
        {
            int dx = System.Math.Abs(x1 - x2);
            int dy = System.Math.Abs(y1 - y2);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1) || (dx == 1 && dy == 1);
        }

        private bool IsOnSameAxis(int x1, int y1, int x2, int y2)
        {
            return x1 == x2 || y1 == y2;
        }

        private bool HasLineOfSight(GameContext ctx, int x1, int y1, int x2, int y2)
        {
            // Einfache Sichtlinien-Prüfung: Keine Gegner dazwischen
            if (x1 == x2) // Gleiche X-Achse
            {
                int minY = System.Math.Min(y1, y2);
                int maxY = System.Math.Max(y1, y2);

                for (int y = minY + 1; y < maxY; y++)
                {
                    if (ctx.Enemies.Any(e => e.X == x1 && e.Y == y))
                        return false;
                }
                return true;
            }
            else if (y1 == y2) // Gleiche Y-Achse
            {
                int minX = System.Math.Min(x1, x2);
                int maxX = System.Math.Max(x1, x2);

                for (int x = minX + 1; x < maxX; x++)
                {
                    if (ctx.Enemies.Any(e => e.X == x && e.Y == y1))
                        return false;
                }
                return true;
            }

            return false;
        }
    }
}
