using Godot;
using System;

public partial class MainMenu : Node2D
{

	private Button ClientButton;
	private Button ServerButton;
	private Button StartButton;
	private TextEdit IpTextEdit;

	public override void _Ready()
	{
		IpTextEdit = GetNode<TextEdit>("TextEdit");
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
		Peer.CreateClient(IpTextEdit.Text, 7777);
		Multiplayer.MultiplayerPeer = Peer;

		GD.Print("client");
	}

	private void ServerButtonPressed()
	{
		var Peer = new ENetMultiplayerPeer();
		Peer.CreateServer(7777);
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
