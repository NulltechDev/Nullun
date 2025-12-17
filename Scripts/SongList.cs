using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using Nullun.Scripts.Utils;
using FileAccess = Godot.FileAccess;

namespace Nullun.Scripts;

public partial class SongList : NullunObject
{
    private PackedScene _chartInfoScene;
    private readonly List<ChartInfo> _songList = new();
    private VBoxContainer _songListContainer;
    private ScrollContainer _songListScrollContainer;
    private Node2D _background;
    private TextureRect _backgroundTexture;
    private Sprite2D _backgroundSprite;
    private bool _isPlaying = false;
    private Node2D _background2;
    private TextureRect _background2Texture;
    private Sprite2D _background2Sprite;
    
    private string Path { get; set; }

    protected override void Declare()
    {
        base.Declare();
        _chartInfoScene = (PackedScene)GD.Load("res://Scenes/ChartInfo.tscn");
        _songListScrollContainer = GetNode<ScrollContainer>("ScrollContainer");
        _songListContainer = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
        _background = GetNode<Node2D>("Background");
        _backgroundTexture = GetNode<TextureRect>("Background/Background/TextureRect");
        _backgroundSprite = GetNode<Sprite2D>("Background/Background");
        _background2 =  GetNode<Node2D>("Background2");
        _background2Texture = GetNode<TextureRect>("Background2/Background/TextureRect");
        _background2Sprite = GetNode<Sprite2D>("Background2/Background");
    }

    protected override void InitContent()
    {
        base.InitContent();
        _songListScrollContainer.Size = GetWindow().Size - new Vector2(0, 80);
        _songListScrollContainer.Position = new Vector2(40, 40);
        _songListScrollContainer.Scale = new Vector2(GetWindow().Size.Y, GetWindow().Size.Y) / new Vector2(128f * 8f, 128f * 8f);
        _background.Scale = new Vector2(GetWindow().Size.Y,GetWindow().Size.Y) / new Vector2(2048f, 2048f) * new Vector2(1.2f,1.2f);
        _background.Position = new Vector2(GetWindow().Size.X * 0.9f, GetWindow().Size.Y / 2f);
        _background2.Scale = new Vector2(GetWindow().Size.Y,GetWindow().Size.Y) / new Vector2(2048f, 2048f) * new Vector2(1.2f,1.2f);
        _background2.Position = new Vector2(GetWindow().Size.X * 0.9f, GetWindow().Size.Y / 2f);
    }

    public new void Show()
    {
        base.Show();
        var animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Play("init");
    }

    public void InitSelection()
    {
        var chartName = "test2";
            
        foreach (var node in _songListContainer.GetChildren())
        {
            var child = (ChartInfo)node;
            if (child.GetChartName().Equals(chartName))
            {
                child.Select();
            }
        }
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
            GD.Print($"Load File: {file}");
        }

        foreach (ChartInfo chartInfo in _songList)
        {
            _songListContainer.AddChild(chartInfo);
        }
    }

    public void Load(string path)
    {
        Stop();
        _background2Texture.Texture = _backgroundTexture.Texture;
        _background2Sprite.Position = Vector2.Zero;
        _background2.Rotation = _background.Rotation;
        GetTree().CreateTween().TweenProperty(_background2Sprite, "global_position", new Vector2(GetWindow().Size.X, -GetWindow().Size.Y * 2), 1f)
            .SetTrans(Tween.TransitionType.Sine);
        Path = path;
        SwitchIn();
    }

    private void SwitchIn()
    {
        _backgroundSprite.Position = new Vector2(0, GetWindow().Size.Y * 2);
        _background.Rotation = 0;
        if(FileAccess.FileExists($"{Path}/Background.jpg"))
        {
            var file = FileAccess.Open($"{Path}/Background.jpg", FileAccess.ModeFlags.Read);
            var image = new Image();
            var buffer = file.GetBuffer((long)file.GetLength());
            var loadResult = image.LoadJpgFromBuffer(buffer);
            if(loadResult == Error.Ok)
            {
                _backgroundTexture.Texture = ImageTexture.CreateFromImage(image);
            }
        }
        GetTree().CreateTween().TweenProperty(_backgroundSprite, "position", Vector2.Zero, 1f)
            .SetTrans(Tween.TransitionType.Sine);
        Task.Run(async () =>
        {
            while(_backgroundSprite.Position.Y > 0)
                await Task.Delay(10);
            await Task.Delay(2000);
            Play();
        });
    }

    public void LoadAudio(string path)
    {
        if(_isPlaying)
            Stop();
        if (!FileAccess.FileExists($"{path}/Audio.wav")) return;
        var audio = GD.Load<AudioStreamOggVorbis>($"{path}/Audio.wav");
        var player = new AudioStreamPlayer();
        player.Stream = audio;
        player.Name = "Player";
        AddChild(player);
    }

    private void Play()
    {
        AudioStreamPlayer player = new();
        try
        {
            player = GetNode<AudioStreamPlayer>("Player");
        }
        catch (Exception e)
        {
            // ignored
        }
        if(player is { Stream: not null } && !player.IsPlaying())
            player.Play();
        if(!_isPlaying)
            _isPlaying = true;
    }

    public void Stop()
    {
        AudioStreamPlayer player = new();
        try
        {
            player = GetNode<AudioStreamPlayer>("Player");
        }
        catch (Exception e)
        {
            // ignored
        }

        if (player is { Stream: not null } && player.IsPlaying())
        {
            player.Stop();
        }
        if(_isPlaying)
            _isPlaying = false;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_isPlaying)
        {
            _background.Rotation+=0.01f;
        }
    }
}