using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Nullun.Scripts.Data;

namespace Nullun.Scripts;

public partial class Track : NullunObject
{
    public int TrackWidth => 100;
    public int TrackHeight => 800;

    public Rect2[] TrackRects =>
    [
        new(-2*TrackWidth,-TrackHeight/2f,TrackWidth,TrackHeight),
        new(-TrackWidth,-TrackHeight/2f,TrackWidth,TrackHeight),
        new(0,-TrackHeight/2f,TrackWidth,TrackHeight),
        new(TrackWidth,-TrackHeight/2f,TrackWidth,TrackHeight)
    ];

    public Rect2[] FlickTrackRects =>
    [
        new(-3*TrackWidth,-TrackHeight/2f,TrackWidth,TrackHeight),
        new(2*TrackWidth,-TrackHeight/2f,TrackWidth,TrackHeight),
    ];
    
    public Color FlickTrackColor1 => Color.FromHsv(175/360f,1,1);
    public Color FlickTrackColor2 => Color.FromHsv(330/360f,1,1);
    
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
    public float PreStart;
    public float TotalTime;

    public override void _Process(double delta)
    {
        base._Process(delta);
        QueueRedraw();
    }

    public override void _Draw()
    {
        base._Draw();
        DrawRect(new Rect2(-3*TrackWidth-1,-TrackHeight/2f-1,TrackWidth*6+1,TrackHeight+1),Colors.White);
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

    public void SetTotalTime(float totalTime)
    {
        TotalTime = totalTime;
    }

    public void SetPreStart(float preStart)
    {
        PreStart = preStart;
    }
}