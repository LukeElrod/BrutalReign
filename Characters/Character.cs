using Godot;
using System;

public partial class Character : CharacterBody2D
{
	public override void _Ready()
	{
		if (IsMultiplayerAuthority())
		{
			Position = new Vector2(GD.RandRange(0, 500), GD.RandRange(0, 500));
		}
	}

	public override void _Process(double delta)
	{
		if (IsMultiplayerAuthority())
		{
			
		}
	}
}
