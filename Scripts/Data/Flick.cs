namespace Nullun.Scripts.Data;

public enum FlickDirection { Left, Right, Up, Down }
public partial class Flick : NullunObject
{
	public float Time { get; set; }
	public int Track { get; set; }
	public FlickDirection Direction { get; set; }
	public float Length { get; set; }
}