// scripts/Core/Entities/Enemy.cs
namespace Dungeon2048.Core.Entities
{
    public enum EnemyType
    {
        // Akt 1: Die Katakomben
        Goblin,
        Skeleton,
        Rat,
        Necrophage,
        Mimic,
        GoblinKing,     // Boss
        
        // Akt 2: Die Vergessenen Hallen (für später)
        Orc,
        Kultist,
        Gargoyle,
        SoulLeech,
        MirrorKnight,
        HexWitch,
        LichMage,       // Boss
        
        // Akt 3: Die Vulkanschmiede (für später)
        FireElemental,
        Moloch,
        SchmiedGolem,
        Pyromaniac,
        ObsidianWarrior,
        ForgeMaster,
        FireGiant,      // Boss
        
        // Akt 4: Die Frostigen Tiefen (für später)
        FrostGoblin,
        Yeti,
        IceShard,
        Frostbite,
        Snowblind,
        GlacialSentinel,
        PermafrostLich,
        IceDragon,      // Boss
        
        // Akt 5: Der Abgrund (für später)
        Dragon,
        VoidSpawn,
        ChaosKnight,
        Doppelganger,
        Parasite,
        RealityBender,
        SoulEater,
        Paradox,
        DungeonLord,    // Final Boss
        
        // Legacy/Special
        Boss,           // Generic Boss
        Masochist,
        Thorns
    }

    public sealed class Enemy : EntityBase
    {
        public EnemyType Type;
        public int EnemyLevel;
        public bool IsBoss;

        // Spezielle Mechanik-Properties
        public bool IsDisguised = false;

        public int MimicHitCount = 0;              // NEU: Wie oft wurde getarnter Mimic getroffen
        public const int MimicHitsToReveal = 3;      // Mimic: Getarnt als Spell Drop
        public int HealedThisRound = 0;            // Necrophage: Tracking für UI-Display
        public int MovesThisRound = 0;             // Rat: Tracking für Doppelbewegung
        public int FrozenTurnsRemaining = 0;       // Für Freeze-Effekte
        public bool HasMoved = false;              // Gargoyle: Statue-Mechanik
        public int ShieldStacks = 0;               // Für defensive Buffs
        public int AttachedToPlayerId = -1;        // Parasite: Attached state
        public string ClonedFromId = null;
        public int LichTeleportCounter = 0;        // Lich-Magier Teleport Tracking
        public int HexCurseDuration = 0;           // Hex Witch: Verbleibende Züge des Fluchs
        public bool IsPhase2 = false;         // Doppelganger: Original tracking

        // Akt 3: Vulkanschmiede Status Effects & Mechanics
        public int BurningStacks = 0;              // Burning: Stapelbarer Schaden über Zeit
        public int GolemMoveCounter = 0;           // Schmied-Golem: Bewegt sich nur jeden 3. Zug
        public bool StandingOnFire = false;        // Moloch: Tracking für Heilung auf Lava
        public int ForgeBuffStacks = 0;            // Forge Master: Wie oft wurde dieser Gegner gebuffed

        public int MaxHp { get; private set; }
        public Enemy(int x, int y, EnemyType type, int enemyLevel, bool isBoss = false)
            : base(x, y, CalcHp(type, enemyLevel, isBoss), CalcAtk(type, enemyLevel, isBoss))
        {
            Type = type;
            EnemyLevel = enemyLevel;
            IsBoss = isBoss;
            
            // NEU: MaxHp setzen
            MaxHp = CalcHp(type, enemyLevel, isBoss);
            Hp = MaxHp;
            
            // Type-spezifische Initialisierung
            if (type == EnemyType.Mimic)
            {
                IsDisguised = true;
            }
        }

        static int CalcHp(EnemyType type, int level, bool isBoss)
        {
            int baseHp = type switch
            {
                // Akt 1
                EnemyType.Goblin            => 8,
                EnemyType.Skeleton          => 10,
                EnemyType.Rat               => 5,
                EnemyType.Necrophage        => 18,
                EnemyType.Mimic             => 12,
                EnemyType.GoblinKing        => 60,
                
                // Akt 2
                EnemyType.Orc               => 15,
                EnemyType.Kultist           => 8,
                EnemyType.Gargoyle          => 25,
                EnemyType.SoulLeech         => 14,
                EnemyType.MirrorKnight      => 20,
                EnemyType.HexWitch          => 10,
                EnemyType.LichMage          => 70,
                
                // Akt 3
                EnemyType.FireElemental     => 16,
                EnemyType.Moloch            => 30,
                EnemyType.SchmiedGolem      => 35,
                EnemyType.Pyromaniac        => 8,
                EnemyType.ObsidianWarrior   => 22,
                EnemyType.ForgeMaster       => 18,
                EnemyType.FireGiant         => 80,
                
                // Akt 4
                EnemyType.FrostGoblin       => 10,
                EnemyType.Yeti              => 40,
                EnemyType.IceShard          => 6,
                EnemyType.Frostbite         => 15,
                EnemyType.Snowblind         => 12,
                EnemyType.GlacialSentinel   => 45,
                EnemyType.PermafrostLich    => 20,
                EnemyType.IceDragon         => 90,
                
                // Akt 5
                EnemyType.Dragon            => 25,
                EnemyType.VoidSpawn         => 18,
                EnemyType.ChaosKnight       => 20,
                EnemyType.Doppelganger      => 15,
                EnemyType.Parasite          => 5,
                EnemyType.RealityBender     => 16,
                EnemyType.SoulEater         => 12,
                EnemyType.Paradox           => 25,
                EnemyType.DungeonLord       => 150,
                
                // Legacy
                EnemyType.Boss              => 50,
                EnemyType.Masochist         => 18,
                EnemyType.Thorns            => 16,
                _ => 10
            };
            
            double levelMul = System.Math.Pow(1 + level * 0.4, 1.2);
            double bossMul = isBoss ? 2.5 : 1.0;
            return (int)System.Math.Round(baseHp * levelMul * bossMul);
        }

        static int CalcAtk(EnemyType type, int level, bool isBoss)
        {
            int baseAtk = type switch
            {
                // Akt 1
                EnemyType.Goblin            => 2,
                EnemyType.Skeleton          => 3,
                EnemyType.Rat               => 1,
                EnemyType.Necrophage        => 2,
                EnemyType.Mimic             => 8,
                EnemyType.GoblinKing        => 10,
                
                // Akt 2
                EnemyType.Orc               => 4,
                EnemyType.Kultist           => 5,
                EnemyType.Gargoyle          => 3,
                EnemyType.SoulLeech         => 2,
                EnemyType.MirrorKnight      => 5,
                EnemyType.HexWitch          => 0,
                EnemyType.LichMage          => 12,
                
                // Akt 3
                EnemyType.FireElemental     => 4,
                EnemyType.Moloch            => 3,
                EnemyType.SchmiedGolem      => 15,
                EnemyType.Pyromaniac        => 3,
                EnemyType.ObsidianWarrior   => 5,
                EnemyType.ForgeMaster       => 2,
                EnemyType.FireGiant         => 18,
                
                // Akt 4
                EnemyType.FrostGoblin       => 2,
                EnemyType.Yeti              => 8,
                EnemyType.IceShard          => 4,
                EnemyType.Frostbite         => 3,
                EnemyType.Snowblind         => 10,
                EnemyType.GlacialSentinel   => 0,
                EnemyType.PermafrostLich    => 6,
                EnemyType.IceDragon         => 20,
                
                // Akt 5
                EnemyType.Dragon            => 7,
                EnemyType.VoidSpawn         => 5,
                EnemyType.ChaosKnight       => 8,
                EnemyType.Doppelganger      => 5,
                EnemyType.Parasite          => 0,
                EnemyType.RealityBender     => 3,
                EnemyType.SoulEater         => 8,
                EnemyType.Paradox           => 6,
                EnemyType.DungeonLord       => 25,
                
                // Legacy
                EnemyType.Boss              => 12,
                EnemyType.Masochist         => 1,
                EnemyType.Thorns            => 0,
                _ => 1
            };
            
            double mult = type switch
            {
                // Akt 1
                EnemyType.Goblin            => 1.2,
                EnemyType.Skeleton          => 1.3,
                EnemyType.Rat               => 0.8,
                EnemyType.Necrophage        => 1.0,
                EnemyType.Mimic             => 1.5,
                EnemyType.GoblinKing        => 2.8,
                
                // Akt 2
                EnemyType.Orc               => 1.8,
                EnemyType.Kultist           => 1.4,
                EnemyType.Gargoyle          => 1.0,
                EnemyType.SoulLeech         => 0.8,
                EnemyType.MirrorKnight      => 1.5,
                EnemyType.HexWitch          => 0.0,
                EnemyType.LichMage          => 3.0,
                
                // Akt 3
                EnemyType.FireElemental     => 1.3,
                EnemyType.Moloch            => 1.1,
                EnemyType.SchmiedGolem      => 2.0,
                EnemyType.Pyromaniac        => 1.2,
                EnemyType.ObsidianWarrior   => 1.6,
                EnemyType.ForgeMaster       => 0.9,
                EnemyType.FireGiant         => 3.2,
                
                // Akt 4
                EnemyType.FrostGoblin       => 1.1,
                EnemyType.Yeti              => 2.0,
                EnemyType.IceShard          => 1.3,
                EnemyType.Frostbite         => 1.2,
                EnemyType.Snowblind         => 2.2,
                EnemyType.GlacialSentinel   => 0.0,
                EnemyType.PermafrostLich    => 1.7,
                EnemyType.IceDragon         => 3.5,
                
                // Akt 5
                EnemyType.Dragon            => 2.5,
                EnemyType.VoidSpawn         => 1.5,
                EnemyType.ChaosKnight       => 1.8,
                EnemyType.Doppelganger      => 1.5,
                EnemyType.Parasite          => 0.0,
                EnemyType.RealityBender     => 1.1,
                EnemyType.SoulEater         => 2.0,
                EnemyType.Paradox           => 1.6,
                EnemyType.DungeonLord       => 4.0,
                
                // Legacy
                EnemyType.Boss              => 3.0,
                EnemyType.Masochist         => 1.0,
                EnemyType.Thorns            => 0.5,
                _ => 1.0
            };
            
            double levelBonus = level * mult;
            double bossMul = isBoss ? 1.8 : 1.0;
            return (int)System.Math.Round((baseAtk + levelBonus) * bossMul);
        }

        public int XpReward => Player.CalculateXpReward(Type, EnemyLevel, IsBoss);
        public bool IsElite => IsBoss || EnemyLevel >= 5;

        public string DisplayName
        {
            get
            {
                if (IsBoss && Type != EnemyType.GoblinKing && Type != EnemyType.LichMage && 
                    Type != EnemyType.FireGiant && Type != EnemyType.IceDragon && 
                    Type != EnemyType.DungeonLord)
                {
                    return "Boss";
                }
                
                return Type switch
                {
                    // Akt 1
                    EnemyType.Goblin            => "Goblin",
                    EnemyType.Skeleton          => "Skelett",
                    EnemyType.Rat               => "Ratte",
                    EnemyType.Necrophage        => "Nekrophage",
                    EnemyType.Mimic             => IsDisguised ? "Zauber" : "Mimic",
                    EnemyType.GoblinKing        => "Goblin-König",
                    
                    // Akt 2
                    EnemyType.Orc               => "Orc",
                    EnemyType.Kultist           => "Kultist",
                    EnemyType.Gargoyle          => "Gargoyle",
                    EnemyType.SoulLeech         => "Seelensauger",
                    EnemyType.MirrorKnight      => "Spiegelritter",
                    EnemyType.HexWitch          => "Hexe",
                    EnemyType.LichMage          => "Lich-Magier",
                    
                    // Akt 3
                    EnemyType.FireElemental     => "Feuer-Elementar",
                    EnemyType.Moloch            => "Moloch",
                    EnemyType.SchmiedGolem      => "Schmied-Golem",
                    EnemyType.Pyromaniac        => "Pyromane",
                    EnemyType.ObsidianWarrior   => "Obsidian-Krieger",
                    EnemyType.ForgeMaster       => "Schmiedemeister",
                    EnemyType.FireGiant         => "Feuergigant",
                    
                    // Akt 4
                    EnemyType.FrostGoblin       => "Frost-Goblin",
                    EnemyType.Yeti              => "Yeti",
                    EnemyType.IceShard          => "Eis-Splitter",
                    EnemyType.Frostbite         => "Frostbiss",
                    EnemyType.Snowblind         => "Schneeblind",
                    EnemyType.GlacialSentinel   => "Gletscher-Wächter",
                    EnemyType.PermafrostLich    => "Permafrost-Lich",
                    EnemyType.IceDragon         => "Eisdrache",
                    
                    // Akt 5
                    EnemyType.Dragon            => "Drache",
                    EnemyType.VoidSpawn         => "Void-Brut",
                    EnemyType.ChaosKnight       => "Chaos-Ritter",
                    EnemyType.Doppelganger      => "Doppelgänger",
                    EnemyType.Parasite          => "Parasit",
                    EnemyType.RealityBender     => "Realitäts-Bieger",
                    EnemyType.SoulEater         => "Seelenfresser",
                    EnemyType.Paradox           => "Paradoxon",
                    EnemyType.DungeonLord       => "Dungeon-Lord",
                    
                    // Legacy
                    EnemyType.Boss              => "Boss",
                    EnemyType.Masochist         => "Masochist",
                    EnemyType.Thorns            => "Dornen",
                    _ => "Gegner"
                };
            }
        }
        
        // Hilfsmethoden für spezielle Mechaniken
public bool CanMove()
{
    if (FrozenTurnsRemaining > 0) return false;
    if (Type == EnemyType.Gargoyle && !HasMoved) return false;
    if (Type == EnemyType.Kultist) return false; // Kultist bewegt sich nie
    if (Type == EnemyType.GlacialSentinel) return false;
    if (Type == EnemyType.ForgeMaster) return false;
    if (Type == EnemyType.HexWitch) return false; // NEU: Hex Witch bewegt sich langsam/selten

    return true;
}

public bool CanAttack()
{
    // Schmied-Golem greift nur jeden 3. Zug an (aber bewegt sich immer)
    if (Type == EnemyType.SchmiedGolem)
    {
        return GolemMoveCounter >= 3;
    }

    return true;
}
        
        public bool ShouldMoveDouble()
        {
            return Type == EnemyType.Rat && MovesThisRound == 0;
        }
        
        public bool HasSpecialAbility()
        {
            return Type switch
            {
                EnemyType.Necrophage => true,
                EnemyType.Mimic => IsDisguised,
                EnemyType.Thorns => true,
                EnemyType.Masochist => true,
                EnemyType.SoulLeech => true,
                EnemyType.MirrorKnight => true,
                EnemyType.HexWitch => true,
                EnemyType.Pyromaniac => true,
                EnemyType.ForgeMaster => true,
                EnemyType.Frostbite => true,
                EnemyType.PermafrostLich => true,
                EnemyType.Parasite => true,
                EnemyType.RealityBender => true,
                EnemyType.SoulEater => true,
                EnemyType.Doppelganger => true,
                _ => false
            };
        }
        
        public string GetAbilityDescription()
        {
            return Type switch
            {
                EnemyType.Skeleton          => "30% Chance Knochenhaufen bei Tod",
                EnemyType.Rat               => "Bewegt sich doppelt",
                EnemyType.Necrophage        => "Heilt +3 HP bei jedem Tod",
                EnemyType.Mimic             => IsDisguised ? "Getarnt als Zauber" : "Hoher Schaden",
                EnemyType.GoblinKing        => "Spawnt Goblins alle 3 Züge",
                
                EnemyType.Kultist           => "Schießt Kreuz-Pattern",
                EnemyType.Gargoyle          => "Bewegt nur jeden 2. Zug",
                EnemyType.SoulLeech         => "Reduziert permanant ATK",
                EnemyType.MirrorKnight      => "Spiegelt deine Stats",
                EnemyType.HexWitch          => "Verflucht Heilung",
                
                EnemyType.FireElemental     => "Hinterlässt Lava",
                EnemyType.Moloch            => "Heilt auf Lava",
                EnemyType.SchmiedGolem      => "Langsam aber tödlich",
                EnemyType.Pyromaniac        => "Explodiert bei Tod",
                EnemyType.ObsidianWarrior   => "Immun gegen Feuer",
                EnemyType.ForgeMaster       => "Buffet andere Gegner",
                
                EnemyType.FrostGoblin       => "Verlangsamt bei Treffer",
                EnemyType.Yeti              => "Immun gegen Freeze",
                EnemyType.IceShard          => "Gleitet weiter",
                EnemyType.Frostbite         => "Vereist Tiles",
                EnemyType.Snowblind         => "Unsichtbar aus Distanz",
                EnemyType.GlacialSentinel   => "Blockiert Nachbar-Tiles",
                EnemyType.PermafrostLich    => "Respawnt nach 3 Zügen",
                
                EnemyType.VoidSpawn         => "Bewegt sich diagonal",
                EnemyType.ChaosKnight       => "Stats ändern sich",
                EnemyType.Doppelganger      => "Kopiert deine Züge",
                EnemyType.Parasite          => "Attached 2 Schaden/Zug",
                EnemyType.RealityBender     => "Teleportiert Gegner",
                EnemyType.SoulEater         => "Absorbiert tote Gegner",
                EnemyType.Paradox           => "Existiert doppelt",
                
                EnemyType.Masochist         => "Nimmt Schaden durch Bewegung",
                EnemyType.Thorns            => "Reflektiert Schaden",
                _ => ""
            };
        }

        public void ResetTurnCounters()
        {
            MovesThisRound = 0;
            HealedThisRound = 0;
            HasMoved = false;

            if (FrozenTurnsRemaining > 0)
                FrozenTurnsRemaining--;

            // Burning Schaden anwenden und stacken
            if (BurningStacks > 0)
            {
                int burnDamage = BurningStacks * 2; // 2 Schaden pro Stack
                Hp -= burnDamage;
                Godot.GD.Print($"{DisplayName} nimmt {burnDamage} Burning-Schaden! ({BurningStacks} Stacks)");
            }

            // Schmied-Golem Counter inkrementieren jeden Zug
            // (wird in MovementPipeline auf 0 gesetzt nach Angriff)
            if (Type == EnemyType.SchmiedGolem)
            {
                GolemMoveCounter++;
                Godot.GD.Print($"🔨 Schmied-Golem Counter: {GolemMoveCounter}/3");
            }
        }
        
        public void SyncMirrorKnightStats(Entities.Player player)
{
    if (Type != EnemyType.MirrorKnight) return;
    
    Hp = player.Hp;
    Atk = player.Atk;
}

// Gargoyle Schaden-Reduktion:
public int ApplyGargoyleDamageReduction(int incomingDamage)
{
    if (Type != EnemyType.Gargoyle) return incomingDamage;
    
    // 50% Schadensreduktion
    return (int)System.Math.Max(1, incomingDamage * 0.5);
}

// Lich Teleport Check:
public bool ShouldLichTeleport()
{
    if (Type != EnemyType.LichMage) return false;

    LichTeleportCounter++;
    return LichTeleportCounter >= 2; // Alle 2 Züge
}

// Moloch Heilung auf Feuer:
public void HealOnFire(int healAmount)
{
    if (Type != EnemyType.Moloch) return;
    if (!StandingOnFire) return;

    Hp = System.Math.Min(Hp + healAmount, MaxHp);
    Godot.GD.Print($"{DisplayName} heilt {healAmount} HP auf Lava!");
}

// Obsidian Warrior: Wird stärker durch Feuer-Schaden
public void AbsorbFireDamage(int damageAmount)
{
    if (Type != EnemyType.ObsidianWarrior) return;

    // Anstatt Schaden zu nehmen, wird er stärker
    int atkBoost = damageAmount / 2; // 50% des Schadens wird zu ATK
    Atk += atkBoost;
    Godot.GD.Print($"{DisplayName} absorbiert Feuer-Energie! +{atkBoost} ATK");
}

// Forge Master: Bufft benachbarte Gegner
public void ApplyForgeBuffs()
{
    if (Type != EnemyType.ForgeMaster) return;
    ForgeBuffStacks++;
}
    }
}