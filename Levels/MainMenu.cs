using Godot;
using System;

public partial class MainMenu : Node2D
{

	Button ClientButton;
	Button ServerButton;
	Button StartButton;

	public override void _Ready()
	{
		ClientButton = GetNode<Button>("ClientButton");
		ServerButton = GetNode<Button>("ServerButton");
		StartButton = GetNode<Button>("StartButton");

		ClientButton.Pressed += ClientButtonPressed;
		ServerButton.Pressed += ServerButtonPressed;
		StartButton.Pressed += StartButtonPressed;
	}

	public override void _Process(double delta)
	{
	}

	private void ClientButtonPressed()
	{
		var Peer = new ENetMultiplayerPeer();
		Peer.CreateClient("127.0.0.1", 8080);
		Multiplayer.MultiplayerPeer = Peer;

		GD.Print("client");
	}

	private void ServerButtonPressed()
	{
		var Peer = new ENetMultiplayerPeer();
		Peer.CreateServer(8080);
		Multiplayer.MultiplayerPeer = Peer;

		GD.Print("server");
	}

	private void StartButtonPressed()
	{
		if (Multiplayer.IsServer())
		{
			Rpc(nameof(StartGame));
		}
	}

	[Rpc(CallLocal = true)]
	void StartGame()
	{
		GetTree().ChangeSceneToFile("Levels/Main.tscn");
	}
}
