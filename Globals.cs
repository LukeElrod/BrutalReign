using Godot;
using System;

public partial class Globals : Node
{
  private static Globals _instance;
  public static Globals Instance => _instance;
  
  public override void _EnterTree()
  {
    if(_instance != null){
       this.QueueFree(); // The Singletone is already loaded, kill this instance
    }
    _instance = this;
  }
}
