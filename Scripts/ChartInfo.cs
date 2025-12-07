using System;
using System.IO;
using System.Threading.Tasks;
using Godot;
using NAudio.Wave;
using Nullun.Scripts.Data;
using Json = Nullun.Scripts.Utils.Json;

namespace Nullun.Scripts;

public partial class ChartInfo : Control
{
    private string Title { get; set; }
    private string Artist { get; set; }
    private float Bpm { get; set; }
    private string Audio { get; set; }
    private string Background { get; set; }
    
    private bool _isSelected = false;
    private string Chart { get; set; }

    private int Level { get; set; } = 0;
    
    private Color[] _levelColors = [
        Color.FromHsv(150 / 360f,0.5f,0.9f),
        Color.FromHsv(210 / 360f,0.8f,0.9f),
        Color.FromHsv(30 / 360f,1f,0.95f),
        Color.FromHsv(0 / 360f,0.9f,0.8f)
    ];
    
    private readonly WaveOutEvent _output = new();
    public float PosOffset;

    public void SetInfo(string chartDirectory)
    {
        var charts = Json.Load<ChartData>($"{chartDirectory}/Chart.json").Chart;
        var info = Json.Load<ChartData>($"{chartDirectory}/Chart.json").Meta;
        Chart = chartDirectory;
        Title = info.Title;
        Artist = info.Artist;
        Bpm = info.Bpm;
        
        var difficultyColor = GetNode<ColorRect>("TextureButton/ColorRect");
        difficultyColor.Color = _levelColors[Level];
        
        var titleLabel = GetNode<Label>("TextureButton/Title");
        titleLabel.Text = Title;
        var texture = GetNode<TextureRect>("TextureButton/TextureRect");
        texture.Texture = GD.Load<Texture2D>($"{chartDirectory}/Preview.jpeg");

        var difficulty = GetNode<Label>("TextureButton/Difficulty");
        foreach (var chart in charts)
            if (chart.Level == Level)
                difficulty.Text = $"DIFFICULTY - {chart.Difficulty}";
    }

    public override void _Ready()
    {
        base._Ready();
        var button = GetNode<TextureButton>("TextureButton");
        button.Pressed += ButtonOnPressed;
        button.FocusExited += ButtonOnFocusExited;
        PivotOffset = Size / 2;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Position = new Vector2(150 - (float)Math.Sin(GetGlobalPosition().Y / GetWindow().Size.Y * Math.PI) * 150 + PosOffset, Position.Y);
    }

    private void ButtonOnFocusExited()
    {
        Deselect();
    }

    private void ButtonOnPressed()
    {
        if (!_isSelected) Select();
        else
        {
            var scene = GD.Load<PackedScene>("res://Scenes/Gameplay.tscn");
            if (Chart == null) throw new Exception("Chart not found chart");
            var gameplay = scene.Instantiate<Gameplay>();
            GetTree().GetRoot().GetNode<SongList>("Main/SongList").Hide();
            GetTree().GetRoot().GetNode<Main>("Main").AddChild(gameplay);
            gameplay.Start(Chart);
        }
    }

    public void Select()
    {
        GetTree().CreateTween().TweenProperty(this, "PosOffset", 100,0.2f).SetTrans(Tween.TransitionType.Sine);
        GetTree().CreateTween().TweenProperty(this, "scale", new Vector2(1.1f,1.1f),0.2f).SetTrans(Tween.TransitionType.Sine);
        _isSelected = true;
        AudioFileReader audio = new AudioFileReader($"{Chart}/Audio.wav");
        PlayAudio(audio);
    }

    public void Deselect()
    {
        GetTree().CreateTween().TweenProperty(this, "PosOffset", 0,0.4f).SetTrans(Tween.TransitionType.Cubic);
        GetTree().CreateTween().TweenProperty(this, "scale", Vector2.One,0.4f).SetTrans(Tween.TransitionType.Cubic);
        _isSelected = false;
        _output.Stop();
    }

    private void PlayAudio(AudioFileReader audio)
    {
        
        _output.Init(audio);
        audio.Seek(audio.Length / 2, SeekOrigin.Begin);
        audio.Volume = 0;
        Task.Run(async () =>
        {
            // 1. 降低步长+延长单次延迟，减少CPU占用，让音量变化更线性
            // 总渐入时长≈1.0/0.0002 * 5 = 25秒（可按需求调整step/delay）
            float step = 0.002f; 
            int delayMs = 5;      

            // 2. 加锁+浮点上限限制，避免音量超1.0和线程冲突
            while (true)
            {
                lock (audio) // 锁定audio，避免Volume多线程读写冲突
                {
                    if (audio.Volume >= 1.0f)
                    {
                        audio.Volume = 1.0f; // 强制封顶，避免失真
                        break;
                    }
                    audio.Volume += step;
                }
                await Task.Delay(delayMs); // 异步延迟，不阻塞线程
            }
        });
        _output.Play();
    }
}