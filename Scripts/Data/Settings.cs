using System;
using System.IO;
using Godot;
using Json = Nullun.Scripts.Utils.Json;
namespace Nullun.Scripts.Data;

public class Settings
{
    private static readonly string Path = ProjectSettings.GlobalizePath("user://Settings.json");
    public float Speed { get; set; }
    public float Offset { get; set; }
    public float PreStart { get; set; }
    public static Settings Instance
    {
        get
        {
            if(!File.Exists(Path))
            {
                var settings = new Settings()
                {
                    Speed = 100,
                    Offset = 0,
                    PreStart = 4
                };
                Json.Export(Path, settings);
            }
            Console.WriteLine(Path);
            return Json.Load<Settings>(Path);
        }
    }

    public void Save(float speed, float offset, float preStart)
    {
        Instance.Speed = speed;
        Instance.Offset = offset;
        Instance.PreStart = preStart;
        Json.Export(Path,Instance);
    }
}