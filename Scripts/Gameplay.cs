using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Nullun.Scripts.Data;
using Json = Nullun.Scripts.Utils.Json;

namespace Nullun.Scripts;

public partial class Gameplay : NullunObject
{
	private List<Note> _notes =  new();
	private List<Hold> _holds = new();
	private List<Glide> _glides = new();
	private List<Flick> _flicks = new();
	
	private Note _note;
	private Hold _hold;
	private Glide _glide;
	private Flick _flick;

	private PackedScene _noteScene;
	private PackedScene _holdScene;
	private PackedScene _glideScene;
	private PackedScene _flickScene;
	
	private float DeltaPerTicks {get; set;}
	private List<BpmChange> BpmChanges { get; set; } = new();
	private Stopwatch CurrentTime {get; set;}

	private int BeatAccuracy { get; } = 100;

	private bool IsPlaying
	{
		get => ProcessMode != ProcessModeEnum.Disabled;
		set => ProcessMode = value ? ProcessModeEnum.Pausable : ProcessModeEnum.Disabled;
	}

	protected override void Declare()
	{
		base.Declare();
		_noteScene = (PackedScene)GD.Load("res://Scenes/Gameplay/Note.tscn");
		if(_noteScene == null) throw new Exception("Could not find Note Scene");
		_holdScene = (PackedScene)GD.Load("res://Scenes/Gameplay/Hold.tscn");
		if(_holdScene == null) throw new Exception("Could not find Hold Scene");
		_glideScene = (PackedScene)GD.Load("res://Scenes/Gameplay/Glide.tscn");
		if(_glideScene == null) throw new Exception("Could not find Glide Scene");
		_flickScene = (PackedScene)GD.Load("res://Scenes/Gameplay/Flick.tscn");
		if(_flickScene == null) throw new Exception("Could not find Flick Scene");
	}

	private void Start()
	{
		if (!LoadChart("Chart/TestChart.json", 0)) return;
		Play();
		CurrentTime = Stopwatch.StartNew();
	}

	private bool LoadChart(string filename,int difficulty)
	{
		ChartData data = Json.Load<ChartData>(filename);
		MetaData meta = data.Meta;
		ChartItem chart = data.Items[difficulty];
		InitBpm(meta);
		try
		{
			_notes.Clear();
			_holds.Clear();
			_glides.Clear();
			_flicks.Clear();
			foreach (var note in chart.Note)
				_notes.Add(LoadNote(note));
			foreach (var hold in chart.Hold)
				_holds.Add(LoadHold(hold));
			foreach (var glide in chart.Glide)
				_glides.Add(LoadGlide(glide));
			foreach (var flick in chart.Flick)
				_flicks.Add(LoadFlick(flick));
		}
		catch (Exception e)
		{
			GD.PrintErr(e.Message);
			return false;
		}
		return true;
	}

	private void Play()
	{
		IsPlaying = true;
	}

	private Note LoadNote(Note note = null)
	{
		if (note == null) throw new Exception("Couldn't load note");
		var n = (Note)_noteScene.Instantiate();
		n.Time = note.Time;
		n.Track = note.Track;
		return n;
	}

	private Hold LoadHold(Hold hold = null)
	{
		if (hold == null) throw new Exception("Couldn't load hold");
		var h = (Hold)_holdScene.Instantiate();
		h.Time = hold.Time;
		h.Track = hold.Track;
		h.Duration = hold.Duration;
		h.Path = hold.Path;
		return h;
	}

	private Glide LoadGlide(Glide glide = null)
	{
		if (glide == null) throw new Exception("Couldn't load glide");
		var g = (Glide)_glideScene.Instantiate();
		g.Time = glide.Time;
		g.Track = glide.Track;
		return g;
	}

	private Flick LoadFlick(Flick flick = null)
	{
		if (flick == null) throw new Exception("Couldn't load flick");
		var f = (Flick)_flickScene.Instantiate();
		f.Time = flick.Time;
		f.Track = flick.Track;
		f.Direction = flick.Direction;
		f.Length = flick.Length;
		return f;
	}

	private void InitBpm(MetaData meta)
	{
		DeltaPerTicks = 60000f / meta.Bpm;
		BpmChanges = meta.BpmChanges;
	}

	private void MonitorBpmChanges()
	{
		foreach (var change in BpmChanges.Where(change => ((int)(change.Time * BeatAccuracy)).Equals(GetBeat())))
			DeltaPerTicks = 60000f / change.Bpm;
	}

	private int GetBeat()
	{
		return (int)(CurrentTime.ElapsedMilliseconds / DeltaPerTicks * BeatAccuracy);
	}
}