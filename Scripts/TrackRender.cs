using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Godot;
using Nullun.Scripts.Data;

namespace Nullun.Scripts;

public partial class TrackRender : NullunObject
{
    private Track _parent;
    
    private int TrackWidth => _parent.TrackWidth;
    private int TrackHeight => _parent.TrackHeight;
    private Rect2[] TrackRects => _parent.TrackRects;
    private Rect2[] FlickTrackRects => _parent.FlickTrackRects;
    private Color FlickTrackColor1 => _parent.FlickTrackColor1;
    private Color FlickTrackColor2 => _parent.FlickTrackColor2;

    private Color TrackColor => _parent.TrackColor;
    private Color NoteColor => _parent.NoteColor;
    private Color HoldColor => _parent.HoldColor;
    private Color GlideColor => _parent.GlideColor;
    private Color FlickColor => _parent.FlickColor;
    
    private Color _gridColor = Color.FromHsv(0,0,1,.2f);


    private List<Note> Notes => _parent.Notes;
    private List<Hold> Holds => _parent.Holds;
    private List<Glide> Glides => _parent.Glides;
    private List<Flick> Flicks => _parent.Flicks;

    private float Offset => _parent.Offset;
    private float Speed => _parent.Speed;
    private float Progress => _parent.Progress;
    private float PreStart => _parent.PreStart;
    private float TotalTime => _parent.TotalTime;

    public override void _Process(double delta)
    {
        base._Process(delta);
        QueueRedraw();
    }

    protected override void Declare()
    {
        base.Declare();
        _parent = GetParent<Track>();
    }

    public override void _Draw()
    {
        base._Draw();
        
        DrawGrid(TotalTime);
        DrawTrack();
        RenderItems();
    }

    private void DrawTrack()
    {
        foreach (var trackRect in TrackRects)
            DrawRect(trackRect,TrackColor,false);
        DrawRect(FlickTrackRects[0],FlickTrackColor1,false);
        DrawRect(FlickTrackRects[1],FlickTrackColor2,false);
    }

    private void RenderItems()
    {
        foreach (var note in Notes)
            DrawNote(note,Progress);
        foreach (var hold in Holds)
            DrawHold(hold,Progress);
        foreach (var glide in Glides)
            DrawGlide(glide,Progress);
        foreach (var flick in Flicks)
            DrawFlick(flick,Progress);
    }

    private void DrawGrid(float totalTime)
    {
        for (int i = -(int)PreStart; i < totalTime; i++)
        {
            DrawLine(new Vector2(-2*TrackWidth,TrackHeight / 2f - (i - Progress) * Speed),
                new Vector2(2*TrackWidth,TrackHeight / 2f - (i - Progress) * Speed),_gridColor);
        }
    }

    private void DrawNote(Note note, float progress)
    {
        DrawRect(
            new Rect2(note.Track * TrackWidth - TrackWidth * 2, TrackHeight / 2f - (note.Time - progress) * Speed,
                TrackWidth, -4), NoteColor);
    }

    private void DrawHold(Hold hold, float progress)
    {
        DrawRect(
            new Rect2(hold.Track * TrackWidth - TrackWidth * 2, TrackHeight / 2f - (hold.Time - progress) * Speed,
                TrackWidth,
                hold.Duration * Speed), HoldColor);
    }

    private void DrawGlide(Glide glide, float progress)
    {
        DrawRect(
            new Rect2(glide.Track * TrackWidth - TrackWidth * 2, TrackHeight / 2f - (glide.Time - progress) * Speed,
                TrackWidth, -4),
            GlideColor);
    }

    private void DrawFlick(Flick flick, float progress)
    {
        switch (flick.Track)
        {
            case 0:
                DrawRect(
                    new Rect2(-TrackWidth - TrackWidth * 2, TrackHeight / 2f - (flick.Time - progress) * Speed,
                        TrackWidth, -4),
                    FlickColor);
                break;
            case 1:
                DrawRect(
                    new Rect2(4*TrackWidth - TrackWidth * 2, TrackHeight / 2f - (flick.Time - progress) * Speed,
                        TrackWidth, -4),
                    FlickColor);
                break;
        }
    }
}