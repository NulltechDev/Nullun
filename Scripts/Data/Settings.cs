using Godot;
using Json = Nullun.Scripts.Utils.Json;
namespace Nullun.Scripts.Data;

public partial class Settings : Node
{
    private static readonly string Path = "Resource/Settings.json";
    public float Speed { get; set; }
    public float Offset { get; set; }
    public float PreStart { get; set; }
    public static readonly Settings Instance = Json.Load<Settings>(Path);

    public void Save(float speed, float offset, float preStart)
    {
        Instance.Speed = speed;
        Instance.Offset = offset;
        Instance.PreStart = preStart;
        Json.Export(Path,Instance);
    }
}