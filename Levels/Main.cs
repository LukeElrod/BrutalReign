using Godot;
using System;

public partial class Main : Node2D
{
	[Export]
	private PackedScene CharacterScene;
	private MultiplayerSpawner Spawner;
	public override void _Ready()
	{
		Spawner = GetNode<MultiplayerSpawner>(nameof(Spawner));
		Spawner.SpawnFunction = Callable.From<Variant, Node>(CharacterSpawnFunc);
		if (Multiplayer.IsServer())
		{
			Spawner.Spawn(1);
			foreach(int id in Multiplayer.GetPeers())
			{
				Spawner.Spawn(id);
			}
		}
		ClientGlobals.Instance.ActiveCamera = GetNode<Camera2D>("Camera2D");
	}

	Node CharacterSpawnFunc(Variant Data)
	{
		int id = Data.As<int>();

		Character Player = CharacterScene.Instantiate<Character>();
		Player.SetMultiplayerAuthority(id);
		Player.Name = "Peer" + Multiplayer.GetUniqueId();
		return Player;
	}
}