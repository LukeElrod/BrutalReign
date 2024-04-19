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
	private Label ServerWaitingLabel;
	private int ConnectedPlayers = 0;


	public override void _Ready()
	{
		ServerWaitingLabel = GetNode<Label>("ServerWaitingLabel");
		WaitingLabel = GetNode<Label>("WaitingLabel");
		IpTextEdit = GetNode<TextEdit>("TextEdit");
		QuitButton = GetNode<Button>("QuitButton");
		ClientButton = GetNode<Button>("ClientButton");
		ServerButton = GetNode<Button>("ServerButton");
		StartButton = GetNode<Button>("StartButton");

		ClientButton.Pressed += ClientButtonPressed;
		ServerButton.Pressed += ServerButtonPressed;
		StartButton.Pressed += StartButtonPressed;
		QuitButton.Pressed += QuitButtonPressed;

		Multiplayer.ConnectedToServer += OnConnectedToServer;
		Multiplayer.PeerConnected += OnPeerConnected;
	}

	private void QuitButtonPressed()
	{
		GetTree().Quit();
	}

	private void ClientButtonPressed()
	{
		//dev
		if (IpTextEdit.Text == "")
		{
			var DevPeer = new ENetMultiplayerPeer();
			DevPeer.CreateClient("127.0.0.1", 7777);
			Multiplayer.MultiplayerPeer = DevPeer;
			return;
		}

		var Peer = new ENetMultiplayerPeer();
		Peer.CreateClient(IpTextEdit.Text, 7777);
		Multiplayer.MultiplayerPeer = Peer;
	}

	private void ServerButtonPressed()
	{
		var Peer = new ENetMultiplayerPeer();
		Peer.CreateServer(7777);
		Multiplayer.MultiplayerPeer = Peer;

		ClientButton.Visible = false;
		ServerButton.Visible = false;
		IpTextEdit.Visible = false;
		ServerWaitingLabel.Visible = true;
	}

	private void OnConnectedToServer()
	{
		StartButton.Visible = false;
		ClientButton.Visible = false;
		ServerButton.Visible = false;
		IpTextEdit.Visible = false;
		WaitingLabel.Visible = true;
	}

	private void OnPeerConnected(long id)
    {
        ConnectedPlayers++;
        ServerWaitingLabel.Text = $"Waiting for players...\n{ConnectedPlayers} connected";
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
