using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using Nullun.Scripts.Utils;
using FileAccess = Godot.FileAccess;
using Json = Nullun.Scripts.Utils.Json;

namespace Nullun.Scripts;

public partial class Main : NullunObject
{
	private static Menu _menu = new();
	private static SongList _songList = new();
	private static ColorRect _background;
	private static Download _download;

	protected override void InitContent()
	{
		base.InitContent();
		_background.Size = GetWindow().Size;
		Clear();
		Download();
	}

	protected override void Declare()
	{
		base.Declare();
		_background = GetNode<ColorRect>("ColorRect");
		_download = GetNode<Download>("Download");
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

	private async void Download()
	{
		_download.Show();
		await ChartDownloader.DownloadAndExtractAsync("TestChart");
		Clear();
		_menu = GD.Load<PackedScene>("res://Scenes/Menu.tscn").Instantiate<Menu>();
		_songList = GD.Load<PackedScene>("res://Scenes/SongList.tscn").Instantiate<SongList>();
		_menu.Position = GetWindow().Size / 2;
		_menu.Start += MenuOnStart;
		AddChild(_songList);
		AddChild(_menu);
		_songList.LoadSongList();
		_menu.Show();
	}
}