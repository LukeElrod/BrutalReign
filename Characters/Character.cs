using Godot;
using System;

public partial class Character : CharacterBody2D
{
	private AnimationPlayer AnimPlayer;
	private Sprite2D CharSprite;
	private RayCast2D AttackRay;
	private bool bIsAttacking = false;
	[Export]
	private const float SPEED = 20000.0f;
    [Export]
	private const float GRAVITY = 800.0f;
    [Export]
	private const float JUMP_FORCE = 400.0f;
	private double PositionUpdate = 0.0;

	public override void _Ready()
	{
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		CharSprite = GetNode<Sprite2D>("Sprite2D");
		AttackRay = GetNode<RayCast2D>("RayCast2D");
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

			AnimPlayer.AnimationStarted += OnAnimationStarted;
			AnimPlayer.AnimationFinished += OnAnimFinished;
		}
	}

    public override void _Process(double delta)
    {
        if (IsMultiplayerAuthority())
		{
			//sync pos every 0.05 seconds
			PositionUpdate += delta;
			if (PositionUpdate >= 0.05)
			{
				Rpc(nameof(NotifyPeersPos), Position);
				PositionUpdate = 0.0;
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

            NewVelocity.X = InputVec.X * SPEED * (float)delta;

            if (IsOnFloor() && Input.IsActionJustPressed("Jump"))
            {
                NewVelocity.Y = -JUMP_FORCE;
            }

            Velocity = NewVelocity;

			if (!bIsAttacking)
			{
				if (Velocity.X > 0)
				{
					AnimPlayer.Play("Run");
					CharSprite.FlipH = false;
					AttackRay.TargetPosition = new Vector2(43, 0);
				}else if (Velocity.X < 0)
				{
					AnimPlayer.Play("Run");
					CharSprite.FlipH = true;
					AttackRay.TargetPosition = new Vector2(-43, 0);
				}else
				{
					AnimPlayer.Play("Idle");
				}
			}
			
			MoveAndSlide();
		}
	}

    public override void _Input(InputEvent @event)
    {
		if (IsMultiplayerAuthority())
		{
			if (@event.IsAction("LightAttack"))
			{
				AnimPlayer.Play("Attack");
				bIsAttacking = true;

				if (AttackRay.IsColliding())
				{
					Character Victim = (Character)AttackRay.GetCollider();
					Victim.Rpc(nameof(Kill));
				}
			}
		}		
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void Kill()
	{
		if (Multiplayer.IsServer())
		{
			Position = new Vector2(100, 100);
		}else
		{
			Position = new Vector2(GetViewportRect().Size.X - 100, 100);
		}
	}

	private void OnAnimFinished(StringName Anim)
	{
		if (Anim == "Attack")
		{
			bIsAttacking = false;
		}
	}

	private void OnAnimationStarted(StringName Anim)
	{
		Rpc(nameof(NotifyPeersAnim), Anim);
	}

	[Rpc]
	private void NotifyPeersAnim(StringName Anim)
	{
		AnimPlayer.Play(Anim);
	}

	[Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered, TransferChannel = 1)]
	private void NotifyPeersPos(Vector2 NewPos)
	{
		CreateTween().TweenProperty(this, "position", NewPos, 0.05);
	}

}
