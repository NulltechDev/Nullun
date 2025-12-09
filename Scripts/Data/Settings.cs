using System;
using System.IO;
using Godot;
using Newtonsoft.Json;
using FileAccess = Godot.FileAccess;
using Json = Nullun.Scripts.Utils.Json;
namespace Nullun.Scripts.Data;

public class Settings
{
    private static string Path { get; set; }
    // 新增：静态字段缓存单例，避免重复加载
    private static Settings _instance;
    
    public float Speed { get; set; }
    public float Offset { get; set; }
    public float PreStart { get; set; }

    public Settings()
    {
        Path = ProjectSettings.GlobalizePath("user://Settings.json");
    }
    
    public static Settings Instance
    {
        get
        {
            // 改动1：先检查缓存的单例，不存在才加载/创建
            if (_instance == null)
            {
                // 改动2：替换System.IO.File为Godot的FileAccess，兼容安卓
                if (!FileAccess.FileExists(Path))
                {
                    _instance = new Settings()
                    {
                        Speed = 100,
                        Offset = 0,
                        PreStart = 4
                    };
                    Json.Export(Path, _instance);
                }
                else
                {
                    _instance = Json.Load<Settings>(Path);
                }
            }
            Console.WriteLine(Path);
            return _instance;
        }
    }

    public void Save(float speed, float offset, float preStart)
    {
        // 改动3：直接修改缓存的单例，而非重新获取Instance（避免覆盖）
        _instance.Speed = speed;
        _instance.Offset = offset;
        _instance.PreStart = preStart;
        Json.Export(Path, _instance);
    }

    public void Init()
    {
        // 改动4：复用Instance逻辑，避免重复代码
        if (_instance == null) _ = Instance;
    }
}