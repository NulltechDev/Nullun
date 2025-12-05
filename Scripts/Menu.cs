using System;
using Godot;

namespace Nullun.Scripts;

public partial class Menu : NullunObject
{
	private static TextureButton _startButton;
	public delegate void MenuEventHandler(object sender,EventArgs e);
	public event MenuEventHandler Start;
	
	protected override void Declare()
	{
		base.Declare();
		_startButton = GetNode<TextureButton>("StartButton");
	}

	protected override void Event()
	{
		base.Event();
		_startButton.Pressed += StartButtonOnPressed;
	}

	protected override void InitContent()
	{
		base.InitContent();
		_startButton.PivotOffset = _startButton.Size / 2;
	}

	private void StartButtonOnPressed()
	{
		Start?.Invoke(null, EventArgs.Empty);
	}
}