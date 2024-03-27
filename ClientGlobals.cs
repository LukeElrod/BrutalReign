using Godot;
using System;

public partial class ClientGlobals : Node
{
	private static ClientGlobals _instance;
	public static ClientGlobals Instance => _instance;

	public override void _EnterTree()
	{
		if(_instance != null)
		{
			QueueFree(); // The Singletone is already loaded, kill this instance
		}
		_instance = this;
	}
}
