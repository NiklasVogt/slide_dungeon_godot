using Dungeon2048.Core.Services;

namespace Dungeon2048.Core.Spells
{
    public interface ISpellBehavior
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        bool IsPermanent { get; }

        // Kontextbasiert, damit Effekte nicht in UI/Board liegen
        void Cast(Entities.Player player, GameContext ctx);
    }
}
