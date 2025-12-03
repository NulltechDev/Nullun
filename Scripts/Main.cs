using System;
using Godot;

namespace Nullun.Resource.Scripts;

public partial class Main : NullunObject
{
	protected static Node2D Menu;
	public override void _Ready()
	{
	}
	
	public override void _Process(double delta)
	{
	}

	protected override void Declare()
	{
		base.Declare();
		try
		{
			Menu = GetNode<Node2D>("Menu");
		}
		catch (InvalidOperationException @exception)
		{
			Console.Error.WriteLine(@exception.Message);
		}
	}
}