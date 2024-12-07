using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LuminaGuard.Models
{
    public class ProfilesManager
    {
        private Dictionary<string, UserConfig> profiles = new Dictionary<string, UserConfig>();

        private string ProfilesPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles.json");

        public void LoadProfiles()
        {
            if (File.Exists(ProfilesPath))
            {
                var json = File.ReadAllText(ProfilesPath);
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, UserConfig>>(json);
                    if (dict != null) profiles = dict;
                }
                catch (Exception ex)
                {
                    Helpers.Logging.Log("Error loading profiles: " + ex.ToString());
                }
            }

            if (!profiles.ContainsKey("Default"))
            {
                profiles["Default"] = new UserConfig() { CurrentProfileName = "Default" };
            }
        }

        public void SaveProfiles()
        {
            try
            {
                var json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ProfilesPath, json);
            }
            catch (Exception ex)
            {
                Helpers.Logging.Log("Error saving profiles: " + ex.ToString());
            }
        }

        public IEnumerable<string> GetProfileNames()
        {
            return profiles.Keys;
        }

        public void SwitchProfile(string name)
        {
            if (profiles.ContainsKey(name))
            {
                App.CurrentConfig = profiles[name];
                App.CurrentConfig.CurrentProfileName = name;
            }
        }

        public void CreateProfile(string name)
        {
            if (!profiles.ContainsKey(name))
            {
                var newConfig = new UserConfig { CurrentProfileName = name };
                profiles[name] = newConfig;
            }
        }

        public void DeleteProfile(string name)
        {
            if (name == "Default") return; // can't delete default
            if (profiles.ContainsKey(name))
            {
                profiles.Remove(name);
            }
        }
    }
}
