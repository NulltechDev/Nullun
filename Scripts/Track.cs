using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Nullun.Scripts.Data;

namespace Nullun.Scripts;

[Tool]
public partial class Track : NullunObject
{
    public int TrackWidth => 50;
    public int TrackHeight => 500;

    public Rect2[] TrackRects =>
    [
        new(-2*TrackWidth,-TrackHeight/2f,TrackWidth,TrackHeight),
        new(-TrackWidth,-TrackHeight/2f,TrackWidth,TrackHeight),
        new(0,-TrackHeight/2f,TrackWidth,TrackHeight),
        new(TrackWidth,-TrackHeight/2f,TrackWidth,TrackHeight)
    ];


    public Color TrackColor => Color.FromHsv(0, 0, 1);
    public Color NoteColor => Color.FromHsv(.5f, 1, 1);
    public Color HoldColor => Color.FromHsv(.2f, 1, 1);
    public Color GlideColor => Color.FromHsv(.1f, 1, 1);
    public Color FlickColor => Color.FromHsv(.7f, 1, 1);


    public List<Note> Notes { get; set; } = new();
    public List<Hold> Holds { get; set; } = new();
    public List<Glide> Glides { get; set; } = new();
    public List<Flick> Flicks { get; set; } = new();

    public float Offset => Settings.Instance.Offset;
    public float Speed => Settings.Instance.Speed;
    public float Progress;

    public override void _Process(double delta)
    {
        base._Process(delta);
        QueueRedraw();
    }

    public override void _Draw()
    {
        base._Draw();
        DrawRect(new Rect2(-2*TrackWidth-1,-TrackHeight/2f-1,TrackWidth*4+1,TrackHeight+1),Colors.White);
    }

    public void Load(params IEnumerable<object>[] lists)
    {
        foreach (var list in lists)
        {
            try
            {
                switch (list)
                {
                    case List<Note> notes:
                        Notes = notes;
                        break;
                    case List<Hold> holds:
                        Holds = holds;
                        break;
                    case List<Glide> glides:
                        Glides = glides;
                        break;
                    case List<Flick> flicks:
                        Flicks = flicks;
                        break;
                }
            }catch(Exception e)
            {
                GD.PrintErr(e.Message);
            }
        }
    }

    public void SetProgress(float progress)
    {
        Progress = progress;
    }
    
}