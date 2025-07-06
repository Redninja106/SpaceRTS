using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class UserOptions
{
    public float ScrollSpeed { get; set; }
    public float GUIScale { get; set; }
    public bool Fullscreen { get; set; }
    public bool VSync { get; set; }

    public static UserOptions CreateDefaultForCurrentMachine()
    {
        UserOptions settings = new();
        settings.ScrollSpeed = 2;
        settings.GUIScale = float.Round(float.Max(1, Application.PrimaryDisplay.Bounds.Width / 1920f), 1);
        settings.Fullscreen = false;
        settings.VSync = false;
        return settings;
    }

    public UserOptions CreateCopy()
    {
        return (UserOptions)MemberwiseClone();
    }

    public bool TrySave()
    {
        try
        {
            string settingsFile = "./options.json";
            string jsonText = JsonConvert.SerializeObject(this);
            File.WriteAllText(settingsFile, jsonText);
            return true;
        }
        catch (Exception ex)
        {
            DebugLog.Warning($"Could not save user settings ({ex.GetType().Name})");
            return false;
        }
    }

    public static UserOptions LoadOrCreate()
    {
        UserOptions options = CreateDefaultForCurrentMachine();
        try
        {
            JsonSerializerSettings serializerSettings = new()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            string settingsFile = "./options.json";
            string jsonText = File.ReadAllText(settingsFile);
            JsonConvert.PopulateObject(jsonText, options);
            return options;
        }
        catch (Exception ex)
        {
            DebugLog.Warning($"Could not load user settings ({ex.GetType().Name})");
            options.TrySave();
            return options;
        }
    }
}
