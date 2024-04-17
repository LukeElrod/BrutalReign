using Godot;
using System;

public partial class MainMenu : Node2D
{

	private Button ClientButton;
	private Button ServerButton;
	private Button StartButton;
	private Button QuitButton;
	private TextEdit IpTextEdit;
	private Label WaitingLabel;

	public override void _Ready()
	{
		WaitingLabel = GetNode<Label>("WaitingLabel");
		IpTextEdit = GetNode<TextEdit>("TextEdit");
		QuitButton = GetNode<Button>("QuitButton");
		ClientButton = GetNode<Button>("ClientButton");
		ServerButton = GetNode<Button>("ServerButton");
		StartButton = GetNode<Button>("StartButton");

		Multiplayer.ConnectedToServer += OnClientConnectedToServer;
		ClientButton.Pressed += ClientButtonPressed;
		ServerButton.Pressed += ServerButtonPressed;
		StartButton.Pressed += StartButtonPressed;
		QuitButton.Pressed += QuitButtonPressed;
	}

	private void QuitButtonPressed()
	{
		GetTree().Quit();
	}

	public override void _Process(double delta)
	{
		
	}

	private void ClientButtonPressed()
	{
		//dev
		if (IpTextEdit.Text == "")
		{
			var DevPeer = new ENetMultiplayerPeer();
			DevPeer.CreateClient("127.0.0.1", 7777);
			Multiplayer.MultiplayerPeer = DevPeer;

			GD.Print("client");
			return;
		}

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

	private void OnClientConnectedToServer()
	{
		StartButton.Visible = false;
		ClientButton.Visible = false;
		ServerButton.Visible = false;
		IpTextEdit.Visible = false;
		WaitingLabel.Visible = true;
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
