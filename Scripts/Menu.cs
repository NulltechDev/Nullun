using System;
using Godot;

namespace Nullun.Scripts;

public partial class Menu : NullunObject
{
	public delegate void MenuEventHandler(object sender,EventArgs e);
	public event MenuEventHandler Start;

	private bool _skipped = false;

	private void StartButtonOnPressed()
	{
		Start?.Invoke(null, EventArgs.Empty);
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		var animationPlayer = GetNode<AnimationPlayer>("StartButton/AnimationPlayer");
		if(animationPlayer.IsPlaying() && !_skipped)
		{
			animationPlayer.Seek(3.0);
			_skipped = true;
		}
	}
}