using System.Collections.Generic;
using Godot;

namespace Nullun.Scripts.Data;

public class HoldPathNode
{
	public float Time { get; set; }
	public int Track { get; set; }
}
public partial class Hold : NullunObject
{
	public float Time { get; set; }
	public int Track { get; set; }
	public float Duration { get; set; }
	public List<HoldPathNode> Path { get; set; }
}