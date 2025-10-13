namespace Dungeon2048.Core.Objectives
{
    public sealed class SurvivalObjective : IObjective
    {
        public LevelType Type => LevelType.Survival;
        public int Target { get; }
        public int Current { get; set; }
        public bool IsCompleted => Current >= Target;
        public double Progress => System.Math.Min(1.0, (double)Current / Target);
        public string Description => $"Überlebe {Target} Swipes";
        public string ProgressText => $"{Current}/{Target} Swipes";
        public string Icon => "⏱️";
        public SurvivalObjective(int target) { Target = target; }
        public void OnSwipe() { Current += 1; }
        public void OnKillEnemy(Entities.Enemy enemy) { }
        public void OnBossKilled() { }
    }

    public sealed class EliminationObjective : IObjective
    {
        public LevelType Type => LevelType.Elimination;
        public int Target { get; }
        public int Current { get; set; }
        public bool IsCompleted => Current >= Target;
        public double Progress => System.Math.Min(1.0, (double)Current / Target);
        public string Description => $"Töte {Target} Gegner";
        public string ProgressText => $"{Current}/{Target} Gegner";
        public string Icon => "⚔️";
        public EliminationObjective(int target) { Target = target; }
        public void OnSwipe() { }
        public void OnKillEnemy(Entities.Enemy enemy) { Current += 1; }
        public void OnBossKilled() { }
    }

    public sealed class BossObjective : IObjective
    {
        public LevelType Type => LevelType.Boss;
        public int Target { get; }
        public int Current { get; set; }
        public bool BossSpawned { get; set; }
        public bool BossKilled { get; set; }
        public bool IsCompleted => BossKilled;
        public double Progress => BossKilled ? 1.0 : BossSpawned ? 0.95 : System.Math.Min(1.0, (double)Current / Target);
        public string Description => $"Boss nach {Target} Swipes";
        public string ProgressText => Current >= Target ? "Boss besiegen!" : $"{Current}/{Target} Swipes bis Boss";
        public string Icon => "👑";
        public BossObjective(int target) { Target = target; }
        public void OnSwipe()
        {
            if (!BossSpawned) Current += 1;
        }
        public void OnKillEnemy(Entities.Enemy enemy) { if (enemy.IsBoss) BossKilled = true; }
        public void OnBossKilled() { BossKilled = true; }
    }
}
