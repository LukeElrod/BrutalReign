using Godot;
using System;

public partial class Character : CharacterBody2D
{
	private const float SPEED = 200.0f;
	public override void _Ready()
	{
		if (IsMultiplayerAuthority())
		{
			Position = new Vector2(GD.RandRange(0, 500), GD.RandRange(0, 500));
		}
	}

	public override void _Process(double delta)
	{
		if (IsMultiplayerAuthority())
		{
			Vector2 velocity = Vector2.Zero;

            // Get input direction
            if (Input.IsActionPressed("MoveLeft"))
                velocity.X -= 1;
            if (Input.IsActionPressed("MoveRight"))
                velocity.X += 1;
            if (Input.IsActionPressed("MoveUp"))
                velocity.Y -= 1;
            if (Input.IsActionPressed("MoveDown"))
                velocity.Y += 1;

            // Normalize velocity to prevent faster diagonal movement
            velocity = velocity.Normalized();

            // Apply movement
            Velocity = velocity * SPEED;
            MoveAndSlide();
		}
	}
}
