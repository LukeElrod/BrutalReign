using Godot;
using System;

public partial class Character : CharacterBody2D
{
	public override void _Ready()
	{
		/*
		SetMultiplayerAuthority(Multiplayer.GetUniqueId());
		//is not host
		if (!Multiplayer.IsServer())
		{
			Position = new Vector2(500, 500);
		}
		*/

		Position = new Vector2(GD.RandRange(0, 500), GD.RandRange(0, 500));
	}

	public override void _Process(double delta)
	{
	}
}
