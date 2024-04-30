using Godot;

struct AttackPacket
{
	public float Damage;
	public bool bFromRight;
}

public partial class Character : CharacterBody2D
{
	[Export]
	private const float SPEED = 200.0f;
    [Export]
	private const float GRAVITY = 800.0f;
    [Export]
	private const float JUMP_FORCE = 400.0f;
	private PackedScene RagdollScene;

	private AnimationPlayer AnimPlayer;
	private Timer SwordAttackCD;
	private Timer RagdollCD;
	private AudioStreamPlayer SwordSwingPlayer;
	private AudioStreamPlayer SwordHitPlayer;
	private AudioStreamPlayer SwordBlockPlayer;
	private Sprite2D CharSprite;
	private RayCast2D AttackRay;
	private GpuParticles2D BloodParticles;
	private RigidBody2D Ragdoll;
	private CollisionShape2D Collision;
	private Vector2 InputVec = Vector2.Zero;
	//exported for multiplayer sync
	[Export]
	private bool bIsBlocking = false;
	private bool bIsAttacking = false;
	private bool bIsRagdolled = false;
	private bool bIsJumping = false;
	private double PositionUpdate = 0.0;
	private float Health = 100.0f;

	public override void _Ready()
	{
		RagdollScene = GD.Load<PackedScene>("res://Characters/Ragdoll.tscn");
		SwordBlockPlayer = GetNode<AudioStreamPlayer>(nameof(SwordBlockPlayer));
		SwordSwingPlayer = GetNode<AudioStreamPlayer>(nameof(SwordSwingPlayer));
		SwordHitPlayer = GetNode<AudioStreamPlayer>(nameof(SwordHitPlayer));
		Collision = GetNode<CollisionShape2D>(nameof(Collision));
		RagdollCD = GetNode<Timer>(nameof(RagdollCD));
		SwordAttackCD = GetNode<Timer>(nameof(SwordAttackCD));
		CharSprite = GetNode<Sprite2D>(nameof(CharSprite));
		AttackRay = GetNode<RayCast2D>(nameof(AttackRay));
		BloodParticles = GetNode<GpuParticles2D>(nameof(BloodParticles));

		RagdollCD.Timeout += ExitRagdoll;

		if (Multiplayer.IsServer())
		{
			AnimPlayer = IsMultiplayerAuthority() ? GetNode<AnimationPlayer>("YellowAnimationPlayer") : GetNode<AnimationPlayer>("GreenAnimationPlayer");
		}
		else
		{
			AnimPlayer = IsMultiplayerAuthority() ? GetNode<AnimationPlayer>("GreenAnimationPlayer") : GetNode<AnimationPlayer>("YellowAnimationPlayer");
		}


		if (IsMultiplayerAuthority())
		{

			Position = Multiplayer.IsServer() ? Position = new Vector2(100, 100) : Position = new Vector2(GetViewportRect().Size.X - 100, 100);

			AnimPlayer.AnimationStarted += OnAnimationStarted;
			AnimPlayer.AnimationFinished += OnAnimFinished;
		}
	}

    public override void _Process(double delta)
    {
		if (bIsRagdolled || !IsMultiplayerAuthority())
			return;
		
		//sync pos 20 times a second
		PositionUpdate += delta;
		if (PositionUpdate >= 0.05)
		{
			Rpc(nameof(NotifyPeersPos), Position);
			PositionUpdate = 0.0;
		}
    }
	public override void _PhysicsProcess(double delta)
	{
		if (bIsRagdolled)
		{
			Position = Ragdoll.Position;
			return;
		}
		if (!IsMultiplayerAuthority())
			return;

		Vector2 NewVelocity = Velocity;

		NewVelocity.Y += GRAVITY * (float)delta;
		NewVelocity.X = InputVec.X * SPEED;

		if (IsOnFloor() && InputVec.Y < 0)
		{
			NewVelocity.Y = -JUMP_FORCE;
		}

		Velocity = NewVelocity;

		ProcessAnimation();
		MoveAndSlide();
	}

    public override void _Input(InputEvent @event)
    {
		if (bIsRagdolled || !IsMultiplayerAuthority())
			return;

		if (@event.IsActionPressed("Block"))
		{
			bIsBlocking = true;
			InputVec = Vector2.Zero;
			AnimPlayer.Play("Block");
			return;
		}
		else if (@event.IsActionReleased("Block"))
		{
			bIsBlocking = false;
			if (bIsJumping)
				AnimPlayer.Play("Jump");
		}

		if (!bIsBlocking)
		{
			if (@event.IsAction("LightAttack") && SwordAttackCD.IsStopped())
			{
				SwordAttackCD.Start();
				bIsAttacking = true;
				AnimPlayer.Play("Attack");

				if (AttackRay.IsColliding())
				{
					Character Victim = (Character)AttackRay.GetCollider();
					if (Victim.bIsBlocking)
					{
						Rpc(nameof(NotifyAudio), "Block");
					}else
					{
						Victim.Rpc(nameof(TakeDamage), 25);
						Rpc(nameof(NotifyAudio), "LightAttack");
					}
				}
				else
				{
					Rpc(nameof(NotifyAudio), "Miss");
				}
			}

			InputVec = Vector2.Zero;
			if (Input.IsActionPressed("MoveLeft"))
				InputVec.X -= 1;
			if (Input.IsActionPressed("MoveRight"))
				InputVec.X += 1;
			if (Input.IsActionPressed("Jump"))
				InputVec.Y -= 1;
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
			Position = Multiplayer.IsServer() ? Position = new Vector2(100, 100) : Position = new Vector2(GetViewportRect().Size.X - 100, 100);
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
		if (!bIsAttacking && !bIsJumping && !bIsBlocking)
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
		switch (Anim)
		{
			case "Attack":
				bIsAttacking = false;
				if (bIsJumping)
					AnimPlayer.Play("Jump");
				break;
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
				SwordHitPlayer.Play();
				break;
			case "Block":
				SwordBlockPlayer.Play();
				break;
			case "Miss":
				SwordSwingPlayer.Play();
				break;
		}
	}

	[Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered, TransferChannel = 1)]
	private void NotifyPeersPos(Vector2 NewPos)
	{
		CreateTween().TweenProperty(this, "position", NewPos, 0.05);
	}

}
