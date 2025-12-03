using System;
using Godot;

namespace Nullun.Resource.Scripts;

public partial class Menu : NullunObject
{
	protected static Button StartButton;
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
			StartButton = GetNode<Button>("StartButton");
		}
		catch (InvalidOperationException @exception)
		{
			Console.WriteLine(@exception);
		}
	}
}