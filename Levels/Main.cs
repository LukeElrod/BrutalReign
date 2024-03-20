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
			Character Host = CharacterScene.Instantiate<Character>();
			GetNode("Characters").AddChild(Host, true);

			foreach(int id in Multiplayer.GetPeers())
			{
				Character Other = CharacterScene.Instantiate<Character>();
				GetNode("Characters").AddChild(Other, true);
				Other.SetMultiplayerAuthority(id);
			}
		}
	}

	public override void _Process(double delta)
	{
	}
}
