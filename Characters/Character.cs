using Godot;
using System;

public partial class Character : CharacterBody2D
{
	private const float SPEED = 200.0f;
    private const float GRAVITY = 800.0f;
    private const float JUMP_FORCE = 400.0f;
	public override void _Ready()
	{
		if (IsMultiplayerAuthority())
		{
			if (Multiplayer.IsServer())
			{
				// Host spawns on the left side
				Position = new Vector2(100, 100);
			}
			else
			{
				// Visitor spawns on the right side
				Position = new Vector2(GetViewportRect().Size.X - 100, 100);
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
        if (IsMultiplayerAuthority())
        {
            Vector2 NewVelocity = Velocity;

            NewVelocity.Y += GRAVITY * (float)delta;

            Vector2 InputVec = Vector2.Zero;
            if (Input.IsActionPressed("MoveLeft"))
                InputVec.X -= 1;
            if (Input.IsActionPressed("MoveRight"))
                InputVec.X += 1;

            NewVelocity.X = InputVec.X * SPEED;

            if (IsOnFloor() && Input.IsActionJustPressed("Jump"))
            {
                NewVelocity.Y = -JUMP_FORCE;
            }

            Velocity = NewVelocity;
            MoveAndSlide();
		}
	}
}
