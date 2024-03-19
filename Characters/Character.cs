using Godot;
using System;

public partial class Character : Node2D
{
	public override void _Ready()
	{
		SetMultiplayerAuthority(Multiplayer.GetUniqueId());
	}

	public override void _Process(double delta)
	{
	}
}
