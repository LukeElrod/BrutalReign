using Godot;

struct AttackPacket
{
	public float Damage;
	public bool bFromRight;
}

public partial class Character : CharacterBody2D
{
	private AnimationPlayer AnimPlayer;
	private Timer AttackCD;
	private Timer RagdollCD;
	private AudioStreamPlayer SwingPlayer;
	private AudioStreamPlayer ImpactPlayer;
	private Sprite2D CharSprite;
	private RayCast2D AttackRay;
	private GpuParticles2D BloodParticles;
	[Export]
	private PackedScene RagdollScene;
	private RigidBody2D Ragdoll;
	private CollisionShape2D Collision;
	private bool bIsAttacking = false;
	private bool bIsRagdolled = false;
	private bool bIsJumping = false;
	[Export]
	private const float SPEED = 200.0f;
    [Export]
	private const float GRAVITY = 800.0f;
    [Export]
	private const float JUMP_FORCE = 400.0f;
	private double PositionUpdate = 0.0;
	private float Health = 100.0f;

	public override void _Ready()
	{
		SwingPlayer = GetNode<AudioStreamPlayer>("SwingPlayer");
		ImpactPlayer = GetNode<AudioStreamPlayer>("ImpactPlayer");
		Collision = GetNode<CollisionShape2D>("CollisionShape2D");
		RagdollCD = GetNode<Timer>("RagdollCD");
		AttackCD = GetNode<Timer>("AttackCD");
		CharSprite = GetNode<Sprite2D>("Sprite2D");
		AttackRay = GetNode<RayCast2D>("RayCast2D");
		BloodParticles = GetNode<GpuParticles2D>("BloodParticles");

		RagdollCD.Timeout += ExitRagdoll;

		if (Multiplayer.IsServer())
		{
			if (IsMultiplayerAuthority())
			{
				AnimPlayer = GetNode<AnimationPlayer>("YellowAnimationPlayer");
			}else
			{
				AnimPlayer = GetNode<AnimationPlayer>("GreenAnimationPlayer");
			}
		}else
		{
			if (IsMultiplayerAuthority())
			{
				AnimPlayer = GetNode<AnimationPlayer>("GreenAnimationPlayer");
			}else
			{
				AnimPlayer = GetNode<AnimationPlayer>("YellowAnimationPlayer");
			}
		}


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
		if (bIsRagdolled)
			return;
		
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
		if (bIsRagdolled)
		{
			Position = Ragdoll.Position;
			return;
		}

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

			ProcessAnimation();
			MoveAndSlide();
		}
	}

    public override void _Input(InputEvent @event)
    {
		if (bIsRagdolled)
			return;
		
		if (IsMultiplayerAuthority())
		{
			if (@event.IsAction("LightAttack") && AttackCD.IsStopped())
			{
				AttackCD.Start();
				bIsAttacking = true;
				AnimPlayer.Play("Attack");

				if (AttackRay.IsColliding())
				{
					Character Victim = (Character)AttackRay.GetCollider();
					Victim.Rpc(nameof(TakeDamage), 25);
					Rpc(nameof(NotifyAudio), "LightAttack");
				}else
				{
					Rpc(nameof(NotifyAudio), "Miss");
				}
			}
		}		
    }

	private void EnterRagdoll(Vector2 Impulse)
	{
		Collision.Disabled = true;
		Ragdoll = RagdollScene.Instantiate<RigidBody2D>();
		GetParent().AddChild(Ragdoll);
		Ragdoll.Position = Position;
		Ragdoll.ApplyImpulse(Impulse);
		bIsRagdolled = true;
		RagdollCD.Start();
		AnimPlayer.Play("Dead");
	}

	private void ExitRagdoll()
	{
		Collision.Disabled = false;
		bIsRagdolled = false;
		Ragdoll.QueueFree();

		//respawn
		if (IsMultiplayerAuthority())
		{
			if (Multiplayer.IsServer())
			{
				Position = new Vector2(100, 100);
			}
			else
			{
				Position = new Vector2(GetViewportRect().Size.X - 100, 100);
			}
		}
	}

	private void ProcessAnimation()
	{
		//STATE MACHINE
		if (IsOnFloor())
		{
			bIsJumping = false;
		}

		//locomotion
		if (!bIsAttacking && !bIsJumping)
		{
			if (Mathf.Abs(Velocity.X) > 0)
			{
				AnimPlayer.Play("Run");
			}else
			{
				AnimPlayer.Play("Idle");
			}

		
			if (!IsOnFloor() && !bIsJumping)
			{
				AnimPlayer.Play("Jump");
				bIsJumping = true;
			}
		}
		
		//sprite flipping
		if (Velocity.X > 0)
		{
			CharSprite.FlipH = false;
			AttackRay.TargetPosition = new Vector2(43, 0);
		}else if (Velocity.X < 0)
		{
			CharSprite.FlipH = true;
			AttackRay.TargetPosition = new Vector2(-43, 0);
		}
	}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void TakeDamage(float DamageAmount)
	{
		Health -= DamageAmount;
		BloodParticles.Restart();

		if (Health <= 0)
		{
			//killed
			EnterRagdoll(new Vector2(GD.RandRange(-200, 200), -500));
			ClientGlobals.Instance.ShakeCamera(1f, .5f);
			
			Health = 100;
		}
	}

	private void OnAnimFinished(StringName Anim)
	{
		if (Anim == "Attack")
		{
			bIsAttacking = false;

			if (bIsJumping)
			{
				AnimPlayer.Play("Jump");
			}
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

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void NotifyAudio(StringName Sound)
	{
		switch (Sound)
		{
			case "LightAttack":
				ImpactPlayer.Play();
				break;
			case "Miss":
				SwingPlayer.Play();
				break;
		}
	}

	[Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered, TransferChannel = 1)]
	private void NotifyPeersPos(Vector2 NewPos)
	{
		CreateTween().TweenProperty(this, "position", NewPos, 0.05);
	}

}
