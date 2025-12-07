using System;
using System.Threading.Tasks;
using Godot;
using Nullun.Scripts.Data;
using Json = Nullun.Scripts.Utils.Json;

namespace Nullun.Scripts;

public partial class ChartInfo : Control
{
    private string Title { get; set; }
    private string Artist { get; set; }
    private float Bpm { get; set; }
    private string Genre { get; set; }
    
    private bool _isSelected = false;
    
    private string Chart { get; set; }

    public void SetInfo(string chartDirectory)
    {
        var info = Json.Load<ChartData>($"{chartDirectory}/Chart.json").Meta;
        Chart = chartDirectory;
        Title = info.Title;
        Artist = info.Artist;
        Bpm = info.Bpm;
        
        var titleLabel = GetNode<Label>("TextureButton/Title");
        titleLabel.Text = Title;
    }

    public override void _Ready()
    {
        base._Ready();
        var button = GetNode<TextureButton>("TextureButton");
        button.Pressed += ButtonOnPressed;
    }

    private void ButtonOnPressed()
    {
        if (_isSelected) Select();
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
        Position = new Vector2(20, 0);
        _isSelected = true;
    }

    public void Deselect()
    {
        Position = new Vector2(0, 0);
        _isSelected = false;
    }
}