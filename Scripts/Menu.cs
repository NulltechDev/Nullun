using System;
using Godot;

namespace Nullun.Scripts;

public partial class Menu : NullunObject
{
	private static Button _startButton;
	public delegate void MenuEventHandler(object sender,EventArgs e);
	public event MenuEventHandler Start;

	protected override void Declare()
	{
		base.Declare();
		_startButton = GetNode<Button>("StartButton");
	}

	protected override void Event()
	{
		base.Event();
		_startButton.Pressed += StartButtonOnPressed;
	}

	private void StartButtonOnPressed()
	{
		Start?.Invoke(null, EventArgs.Empty);
	}
}