using System.IO;
using Godot;

namespace Nullun.Scripts;

public partial class SongList : NullunObject
{
    protected override void InitContent()
    {
        base.InitContent();
        string chartDirectory = "Chart/";
        string[] chartFile = Directory.GetFiles(chartDirectory,"*.json");
        foreach (string file in chartFile)
        {
            GD.Print(file);
        }
    }
}