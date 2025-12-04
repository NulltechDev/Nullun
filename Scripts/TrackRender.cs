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

    private Color TrackColor => _parent.TrackColor;
    private Color NoteColor => _parent.NoteColor;
    private Color HoldColor => _parent.HoldColor;
    private Color GlideColor => _parent.GlideColor;
    private Color FlickColor => _parent.FlickColor;


    private List<Note> Notes => _parent.Notes;
    private List<Hold> Holds => _parent.Holds;
    private List<Glide> Glides => _parent.Glides;
    private List<Flick> Flicks => _parent.Flicks;

    private float Offset => _parent.Offset;
    private float Speed => _parent.Speed;

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
        foreach (var trackRect in TrackRects)
            DrawRect(trackRect,TrackColor,false);
        RenderItems();
    }
    private void RenderItems()
    {
        foreach (var note in Notes)
            DrawRect(new Rect2(note.Track * TrackWidth - TrackWidth * 2,TrackHeight/2f - note.Time * Speed, TrackWidth, -4), NoteColor);
        foreach (var hold in Holds)
            DrawRect(new Rect2(hold.Track*TrackWidth - TrackWidth * 2,TrackHeight/2f - hold.Time * Speed,TrackWidth,hold.Duration * Speed),HoldColor);
        foreach (var glide in Glides)
            DrawRect(new Rect2(glide.Track*TrackWidth - TrackWidth * 2,TrackHeight/2f - glide.Time * Speed,TrackWidth,-4),GlideColor);
        // foreach (var flick in Flicks)
        //     throw new NotImplementedException();
    }
}