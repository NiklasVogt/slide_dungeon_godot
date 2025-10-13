namespace Dungeon2048.Core.Objectives
{
    public interface IObjective
    {
        LevelType Type { get; }
        int Target { get; }
        int Current { get; set; }
        bool IsCompleted { get; }
        double Progress { get; }
        string Description { get; }
        string ProgressText { get; }
        string Icon { get; }
        void OnSwipe();
        void OnKillEnemy(Entities.Enemy enemy);
        void OnBossKilled();
    }

    public enum LevelType { Survival, Elimination, Boss }
}
