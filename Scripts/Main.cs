using System;
using Godot;
using Json = Nullun.Scripts.Utils.Json;

namespace Nullun.Scripts;

public partial class Main : NullunObject
{
	private static Menu _menu;
	private static SongList _songList;

	protected override void InitContent()
	{
		base.InitContent();
		Clear();
		_menu.Show();
	}

	protected override void Declare()
	{
		base.Declare();
		_menu = GetNode<Menu>("Menu");
		_songList = GetNode<SongList>("SongList");
	}

	protected override void Event()
	{
		base.Event();
		_menu.Start += MenuOnStart;
	}

	private void MenuOnStart(object sender, EventArgs e)
	{
		Clear();
		_songList.Show();
	}
	
	/// <summary>
	/// 清屏
	/// </summary>
	private void Clear()
	{
		foreach (var node in GetChildren())
		{
			if(node is Node2D node2D)
				node2D.Hide();
		}
	}
}