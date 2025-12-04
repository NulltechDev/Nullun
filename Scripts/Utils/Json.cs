using System;
using System.IO;
using Godot;
using Newtonsoft.Json;

namespace Nullun.Scripts.Utils;

public static class Json
{
    public static void Export(string path, object data)
    {
        string dir = Path.GetDirectoryName(path);
        if(!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static T Load<T>(string path)
    {
        if(!File.Exists(path))
            throw new FileNotFoundException($"Couldn't find json file!",path);
        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        });
    }
}