using Godot;
using System;

public partial class NetworkManager : Node
{
    private static NetworkManager _instance;
    public static NetworkManager Instance => _instance;

    public override void _EnterTree()
    {
        if(_instance != null)
        {
            QueueFree(); // The Singletone is already loaded, kill this instance
        }
        _instance = this;
    }
}
