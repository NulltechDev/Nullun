using System;
using System.IO;
using Godot;
using Newtonsoft.Json;
using FileAccess = Godot.FileAccess;

namespace Nullun.Scripts.Utils;

public static class Json
{
    public static void Export(string path, object data)
    {
        string dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !DirAccess.DirExistsAbsolute(dir))
        {
            DirAccess.MakeDirRecursiveAbsolute(dir);
        }

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        
        // 修复：明确指定FileAccess的Mode枚举（兼容所有Godot版本）
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(json);
    }

    public static T Load<T>(string path)
    {
        if (!FileAccess.FileExists(path))
            throw new FileNotFoundException($"Couldn't find json file!", path);

        // 修复：明确指定FileAccess的Mode枚举（兼容所有Godot版本）
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();
        
        return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        });
    }
}