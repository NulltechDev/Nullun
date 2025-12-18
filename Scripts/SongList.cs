using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using Nullun.Scripts.Data;
using Nullun.Scripts.Utils;
using FileAccess = Godot.FileAccess;
using Json = Nullun.Scripts.Utils.Json;

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
    private Sprite2D _songBackgroundSprite;
    private Path2D _circleText;
    
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
        _songBackgroundSprite = GetNode<Sprite2D>("SongBackground");
        _circleText = GetNode<Path2D>("SongBackground/CircleText");
    }

    protected override void InitContent()
    {
        base.InitContent();
        _songListScrollContainer.Size = GetWindow().Size * 2;
        _songListScrollContainer.Position = new Vector2(GetWindow().Size.Y * 0.05f, GetWindow().Size.Y * 0.05f);
        _songListScrollContainer.Scale = new Vector2(GetWindow().Size.Y, GetWindow().Size.Y) / new Vector2(128f * 8f, 128f * 8f);
        _background.Scale = new Vector2(GetWindow().Size.Y,GetWindow().Size.Y) / new Vector2(2048f, 2048f) * new Vector2(1.2f,1.2f);
        _background.Position = new Vector2(GetWindow().Size.X * 0.9f, GetWindow().Size.Y / 2f);
        _background2.Scale = new Vector2(GetWindow().Size.Y,GetWindow().Size.Y) / new Vector2(2048f, 2048f) * new Vector2(1.2f,1.2f);
        _background2.Position = new Vector2(GetWindow().Size.X * 0.9f, GetWindow().Size.Y / 2f);
        _songBackgroundSprite.Position = new Vector2(GetWindow().Size.X * 0.9f, GetWindow().Size.Y / 2f);
        _songBackgroundSprite.Scale = new Vector2(GetWindow().Size.Y,GetWindow().Size.Y) / new Vector2(2048f, 2048f) * new Vector2(1.4f,1.4f);
        _songBackgroundSprite.Rotation = 15 / 180f * (float)Math.PI;
        _circleText.Rotation = 0;
        _circleText.Modulate = new Color(1f,1f,1f,0f);
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
    
    public bool SwitchFlag = false;
    
    public void Load(string path)
    {
        Stop();
        LoadAudio(path);
        _background2Texture.Texture = _backgroundTexture.Texture;
        _background2Sprite.Position = Vector2.Zero;
        _background2.Rotation = _background.Rotation;
        Path = path;
        TextAnimation();
        GetTree().CreateTween().TweenProperty(_background2Sprite, "global_position",
                new Vector2(GetWindow().Size.X, -GetWindow().Size.Y * 1.5f), 0.5f)
            .SetTrans(Tween.TransitionType.Sine);
        SwitchIn();
    }

    private void SwitchIn()
    {
        SwitchFlag = true;
        _backgroundSprite.Position = new Vector2(0, 2048) * _backgroundSprite.Scale;
        _background.Rotation = 0;
        const float duration = 0.6f;
        const float delay = 0.4f;
        if(FileAccess.FileExists($"{Path}/Background.jpg"))
        {
            var file = FileAccess.Open($"{Path}/Background.jpg", FileAccess.ModeFlags.Read);
            var image = new Image();
            var buffer = file.GetBuffer((long)file.GetLength());
            var loadResult = image.LoadJpgFromBuffer(buffer);
            if(loadResult == Error.Ok)
                _backgroundTexture.Texture = ImageTexture.CreateFromImage(image);
        }
        GetTree().CreateTween().TweenProperty(_backgroundSprite, "position", Vector2.Zero, duration)
            .SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out);
        Task.Run(async () =>
        {
            await Task.Delay((int)((duration + delay) * 1000));
            Play();
            SwitchFlag = false;
        });
    }

    private void TextAnimation()
    {
        var title = Json.Load<ChartData>($"{Path}/Chart.json").Meta.Title + " - " +
                    Json.Load<ChartData>($"{Path}/Chart.json").Meta.Artist;
        if(_songBackgroundSprite.Rotation > 0)
        {
            GetTree().CreateTween().TweenProperty(_songBackgroundSprite, "rotation",
                    0, 0.8f)
                .SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out);
            GetTree().CreateTween().TweenProperty(_circleText, "modulate", new Color(1f, 1f, 1f), 0.8f).SetTrans(Tween.TransitionType.Quint)
                .SetEase(Tween.EaseType.Out);
        }

        GetTree().CreateTween().TweenProperty(_circleText, "rotation",
                60 / 180f * (float)Math.PI - title.Length * 110 / 920f, 0.8f)
            .SetTrans(Tween.TransitionType.Sine);
        GetTree().CreateTween().TweenProperty(_circleText, "text",
                title, 0.4f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        
    }

    private AudioStreamPlayer _player = null;
    
    private void LoadAudio(string path)
    {
        if(_isPlaying)
            Stop();
        if (!FileAccess.FileExists($"{path}/Preview.ogg")) return;
        var audio = AudioStreamOggVorbis.LoadFromFile($"{path}/Preview.ogg");
        audio.Loop = true;
        // var audio = GD.Load<AudioStreamOggVorbis>($"{path}/Audio.ogg");
        _player = GetNodeOrNull<AudioStreamPlayer>("player");
        if(_player == null)
        {
            _player = new AudioStreamPlayer();
            AddChild(_player);
        }
        _player.Stream = audio;
        GD.Print("Load Audio");
    }

    private void Play()
    {
        if(_player is { Stream: not null } && !_player.IsPlaying())
            _player.Play();
        if(!_isPlaying)
            _isPlaying = true;
    }

    public void Stop()
    {
        if (_player is { Stream: not null } && _player.IsPlaying())
        {
            _player.Stop();
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