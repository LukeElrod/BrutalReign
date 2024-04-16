using Godot;
using System;

public partial class ClientGlobals : Node
{
	private static ClientGlobals _instance;
	public static ClientGlobals Instance => _instance;
	//camera
	public Camera2D ActiveCamera;
    private Vector2 OriginalPosition = new Vector2(0, 0);
    private float ShakeIntensity;
    private float ShakeDuration;
    private float ShakeTimer;

	public override void _EnterTree()
	{
		if(_instance != null)
		{
			QueueFree(); // The Singletone is already loaded, kill this instance
		}
		_instance = this;
	}

    public void ShakeCamera(float intensity, float duration)
    {
        if (ActiveCamera != null)
        {
            ShakeIntensity = intensity;
            ShakeDuration = duration;
            ShakeTimer = 0f;
        }
    }

    public override void _Process(double delta)
    {
		if (ActiveCamera != null)
		{
			if (ShakeTimer < ShakeDuration)
			{
				// Calculate the random offset for shaking
				Vector2 randomOffset = new Vector2(
					(float)GD.RandRange(-ShakeIntensity, ShakeIntensity),
					(float)GD.RandRange(-ShakeIntensity, ShakeIntensity)
				);

				// Apply the shake offset to the camera's position
				ActiveCamera.GlobalPosition = OriginalPosition + randomOffset;

				// Update the shake timer
				ShakeTimer += (float)delta;
			}
			else
			{
				// Reset the camera's position to the original position
				ActiveCamera.GlobalPosition = OriginalPosition;
			}
		}
    }
}
