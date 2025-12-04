using System.Collections.Generic;

namespace Nullun.Scripts.Data;

public class BpmChange
{
    public float Time { get; set; }
    public float Bpm { get; set; }
}
public class MetaData
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Illustrator { get; set; }
    public string ChartArtist { get; set; }
    public float Bpm { get; set; }
    public List<BpmChange> BpmChanges { get; set; }
    public float Offset { get; set; }
    public string Audio { get; set; }
    public string Background { get; set; }
    public float TotalTime { get; set; }
}

public class ChartItem
{
    public int Difficulty { get; set; }
    public int Level { get; set; }
    public List<Note> Note { get; set; }
    public List<Hold> Hold { get; set; }
    public List<Glide> Glide { get; set; }
    public List<Flick> Flick { get; set; }
}
public class ChartData
{
    public MetaData Meta { get; set; }
    public List<ChartItem> Chart { get; set; }
}