using Godot;
using System;

public partial class Main : Node2D
{
	[Export]
	PackedScene CharacterScene;
	public override void _Ready()
	{
		if (Multiplayer.IsServer())
		{
			foreach(int id in Multiplayer.GetPeers())
			{
				Character cha = CharacterScene.Instantiate<Character>();
				GetNode("Characters").AddChild(cha);
			}
		}
	}

	public override void _Process(double delta)
	{
	}
}
