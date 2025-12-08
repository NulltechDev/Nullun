using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using Nullun.Scripts.Utils;

namespace Nullun.Scripts;

public partial class SongList : NullunObject
{
    private PackedScene _chartInfoScene;
    private readonly List<ChartInfo> _songList = new();
    private VBoxContainer _songListContainer;
    private ScrollContainer _songListScrollContainer;

    protected override void Declare()
    {
        base.Declare();
        _chartInfoScene = (PackedScene)GD.Load("res://Scenes/ChartInfo.tscn");
        _songListScrollContainer = GetNode<ScrollContainer>("ScrollContainer");
        _songListContainer = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
    }

    protected override void InitContent()
    {
        base.InitContent();
        _songListScrollContainer.Size = GetWindow().Size - new Vector2(0, 80);
        _songListScrollContainer.Position = new Vector2(40, 40);
    }

    public new void Show()
    {
        base.Show();
        var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Play("init");
    }

    public void LoadSongList()
    {
        string chartDirectory = ProjectSettings.GlobalizePath("user://Chart/");
        string[] chartFile = Directory.GetDirectories(chartDirectory);
        _songList.Clear();
        foreach (string file in chartFile)
        {
            var song = _chartInfoScene.Instantiate<ChartInfo>();
            song.SetInfo(file);
            _songList.Add(song);
            GD.Print($"Load File{file}");
        }

        foreach (ChartInfo chartInfo in _songList)
        {
            _songListContainer.AddChild(chartInfo);
        }
    }
}