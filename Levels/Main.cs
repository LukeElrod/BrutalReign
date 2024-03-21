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
			Host.Name = "Host";
			GetNode("Characters").AddChild(Host);

			foreach(int id in Multiplayer.GetPeers())
			{
				Character Other = CharacterScene.Instantiate<Character>();
				Other.Name = "Peer" + id;
				GetNode("Characters").AddChild(Other);
				Other.SetMultiplayerAuthority(id);
				Rpc(nameof(SetVisitorAuthority));
			}
		}
	}

	[Rpc]
	public void SetVisitorAuthority()
	{
		Character character = GetNode<Character>("Characters/Peer" + Multiplayer.GetUniqueId());
		if (character != null)
		{
			character.SetMultiplayerAuthority(Multiplayer.GetUniqueId());
		}
		else
		{
			GD.PrintErr("Character instance not found on client: " + Multiplayer.GetUniqueId());
		}
	}
}
