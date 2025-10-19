// scripts/Core/Progression/SoulManager.cs
using Godot;
// ENTFERNE: using System.IO;  <-- Diese Zeile NICHT verwenden

namespace Dungeon2048.Core.Progression
{
    public sealed class SoulManager
    {
        private int _currentSouls = 0;
        private int _totalSoulsEarned = 0;
        private int _totalSoulsSpent = 0;
        private int _soulsThisRun = 0;
        
        public int CurrentSouls => _currentSouls;
        public int TotalSoulsEarned => _totalSoulsEarned;
        public int TotalSoulsSpent => _totalSoulsSpent;
        public int SoulsThisRun => _soulsThisRun;
        
        public SoulManager()
        {
            Load();
        }
        
        public void AddSouls(int amount)
        {
            if (amount <= 0) return;
            
            _currentSouls += amount;
            _totalSoulsEarned += amount;
            _soulsThisRun += amount;
            
            GD.Print($"💎 +{amount} Seelen! (Gesamt: {_currentSouls})");
            Save();
        }
        
        public bool SpendSouls(int amount)
        {
            if (amount <= 0 || _currentSouls < amount) return false;
            
            _currentSouls -= amount;
            _totalSoulsSpent += amount;
            
            GD.Print($"💰 -{amount} Seelen ausgegeben (Verbleibend: {_currentSouls})");
            Save();
            return true;
        }
        
        public bool CanAfford(int cost)
        {
            return _currentSouls >= cost;
        }
        
        public void ResetRunSouls()
        {
            _soulsThisRun = 0;
        }
        
        public void Save()
        {
            var saveData = new SoulSaveData
            {
                CurrentSouls = _currentSouls,
                TotalSoulsEarned = _totalSoulsEarned,
                TotalSoulsSpent = _totalSoulsSpent
            };
            
            string json = System.Text.Json.JsonSerializer.Serialize(saveData);
            
            try
            {
                // Explizit Godot.FileAccess verwenden
                using var file = Godot.FileAccess.Open("user://souls.save", Godot.FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    GD.PrintErr("Fehler: Konnte Speicherdatei nicht öffnen");
                    return;
                }
                file.StoreString(json);
                GD.Print($"Seelen gespeichert: {_currentSouls}");
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"Fehler beim Speichern der Seelen: {e.Message}");
            }
        }
        
        public void Load()
        {
            if (!Godot.FileAccess.FileExists("user://souls.save"))
            {
                GD.Print("Keine Seelen-Speicherdatei gefunden, starte mit 0 Seelen");
                return;
            }
            
            try
            {
                using var file = Godot.FileAccess.Open("user://souls.save", Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr("Fehler: Konnte Speicherdatei nicht lesen");
                    return;
                }
                
                string json = file.GetAsText();
                
                var saveData = System.Text.Json.JsonSerializer.Deserialize<SoulSaveData>(json);
                
                _currentSouls = saveData.CurrentSouls;
                _totalSoulsEarned = saveData.TotalSoulsEarned;
                _totalSoulsSpent = saveData.TotalSoulsSpent;
                
                GD.Print($"Seelen geladen: {_currentSouls}");
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"Fehler beim Laden der Seelen: {e.Message}");
            }
        }
        
        private struct SoulSaveData
        {
            public int CurrentSouls { get; set; }
            public int TotalSoulsEarned { get; set; }
            public int TotalSoulsSpent { get; set; }
        }
    }
}