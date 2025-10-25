// scripts/Core/World/BiomeSystem.cs
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Dungeon2048.Core.World
{
    public sealed class BiomeSystem
    {
        private readonly Services.GameContext _ctx;
        private readonly Dictionary<BiomeType, IBiome> _biomes = new();
        private IBiome _currentBiome;

        public BiomeSystem(Services.GameContext ctx)
        {
            _ctx = ctx;
            RegisterBiomes();
        }

        private void RegisterBiomes()
        {
            var catacombs = new CatacombsBiome();
            _biomes[BiomeType.Catacombs] = catacombs;

            var forgottenHalls = new ForgottenHallsBiome();
            _biomes[BiomeType.ForgottenHalls] = forgottenHalls;

            var volcanForge = new VolcanForgeBiome();
            _biomes[BiomeType.VolcanForge] = volcanForge;

            // Weitere Biome später
        }

        public IBiome CurrentBiome => _currentBiome;

        public IBiome GetBiomeForLevel(int level)
        {
            foreach (var biome in _biomes.Values)
            {
                if (level >= biome.StartLevel && level <= biome.EndLevel)
                    return biome;
            }
            // Fallback
            return _biomes[BiomeType.Catacombs];
        }

        public void UpdateBiome(int level)
        {
            var newBiome = GetBiomeForLevel(level);
            
            if (_currentBiome != newBiome)
            {
                _currentBiome?.OnExit(_ctx);
                _currentBiome = newBiome;
                _currentBiome.OnEnter(_ctx);
                
                GD.Print($"Biome gewechselt zu: {_currentBiome.Name}");
            }
            
            _currentBiome.OnLevelStart(_ctx);
        }

        public void OnLevelComplete()
        {
            _currentBiome?.OnLevelComplete(_ctx);
        }

        public Color GetBackgroundColor() => _currentBiome?.BackgroundColor ?? Colors.Black;
        public Color GetGridColor() => _currentBiome?.GridColor ?? Colors.Gray;
        public Color GetAmbientColor() => _currentBiome?.AmbientColor ?? Colors.White;
    }
}