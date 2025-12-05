using System;
using Godot;

namespace Nullun.Scripts;

public partial class Menu : NullunObject
{
	public delegate void MenuEventHandler(object sender,EventArgs e);
	public event MenuEventHandler Start;
	

	private void StartButtonOnPressed()
	{
		Start?.Invoke(null, EventArgs.Empty);
	}
}