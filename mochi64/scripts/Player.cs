using Godot;
using System;

// first person view script

public partial class Player : CharacterBody3D
{
	private AnimationPlayer Anim;
	
	public const float WalkSpeed = 3.5f;
	public const float SprintSpeed = 7.5f;
	public const float CrouchSpeed = 2.5f;

	public const float JumpVelocity = 4.5f;

	public const float Acceleration = 12.0f;
	public const float Friction = 10.0f;
	public const float AirControl = 3.0f;

	public const float MouseSensitivity = 0.0025f;

	public const float CrouchHeightOffset = -0.3f;
	public const float CameraLerpSpeed = 10f;

	public const float LeanAngle = 10f;
	public const float LeanOffset = 1.0f;
	public const float LeanSpeed = 8f;

	public const float HeadBobFrequency = 15f;
	public const float HeadBobAmplitude = 0.025f;

	public bool ToggleCrouch = false;

	private Node3D Neck;
	private Node3D Pivot;

	private float xRotation = 0f;
	private float bobTime = 0f;

	private bool isCrouching = false;

	private Vector3 pivotBasePosition;

	public override void _Ready()
	{
		Anim = GetNode<AnimationPlayer>("model/AnimationPlayer");
		Neck = GetNode<Node3D>("neck");
		Pivot = GetNode<Node3D>("neck/pivot");

		pivotBasePosition = Pivot.Position;

		Input.MouseMode = Input.MouseModeEnum.Captured;

		FloorSnapLength = 0.2f;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motion)
		{
			RotateY(-motion.Relative.X * MouseSensitivity);

			xRotation -= motion.Relative.Y * MouseSensitivity;
			xRotation = Mathf.Clamp(xRotation, Mathf.DegToRad(-80), Mathf.DegToRad(80));

			Vector3 neckRotation = Neck.Rotation;
			neckRotation.X = xRotation;
			Neck.Rotation = neckRotation;
		}

		if (Input.IsActionJustPressed("ui_cancel"))
			Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		Vector3 velocity = Velocity;

		// Gravity
		if (!IsOnFloor())
			velocity += GetGravity() * dt;

		// Jump
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor() && !isCrouching)
		{	
			if (Anim.CurrentAnimation != "male_jumping") 
			{
				Anim.Play("male_jumping");
			}
			
			velocity.Y = JumpVelocity;
		}	
		// Crouch
		HandleCrouch();
		float currentSpeed = HandleSpeed();
		
		// Movement input
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 forward = Transform.Basis.Z;
		Vector3 right = Transform.Basis.X;

		forward.Y = 0;
		right.Y = 0;

		forward = forward.Normalized();
		right = right.Normalized();

		Vector3 direction = (right * inputDir.X + forward * inputDir.Y).Normalized();

		float control = IsOnFloor() ? Acceleration : AirControl;

		if (direction != Vector3.Zero)
		{
			if (!IsOnFloor())
			{
				if (Anim.CurrentAnimation != "t-pose")
					Anim.Play("t-pose");
			}
			else
			{
				if (currentSpeed == SprintSpeed)
				{
					if (Anim.CurrentAnimation != "male_running")
					{
						Anim.Play("male_running");
					}
				}
				else if (currentSpeed == CrouchSpeed) 
				{
					if (Anim.CurrentAnimation != "male_sneaking")
					{
						Anim.Play("male_sneaking");
					}
				}
				else 
				{
					if (Anim.CurrentAnimation != "male_walking")
					{
						Anim.Play("male_walking");
					}
				}
			}
			
			
			Vector3 targetVelocity = direction * currentSpeed;

			velocity.X = Mathf.MoveToward(velocity.X, targetVelocity.X, control * dt);
			velocity.Z = Mathf.MoveToward(velocity.Z, targetVelocity.Z, control * dt);
		}
		else if (IsOnFloor())
		{
			if (isCrouching)
			{
				if (Anim.CurrentAnimation != "male_crouching")
				{
					Anim.Play("male_crouching");
				}
			}
			else 
			{
				if (Anim.CurrentAnimation != "male_idle")
				{
					Anim.Play("male_idle");
				}
			}
				
			
			velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * dt);
			velocity.Z = Mathf.MoveToward(velocity.Z, 0, Friction * dt);
		}
		
		Velocity = velocity;
		MoveAndSlide();

		UpdateHeadBob(dt);
		UpdateLean(dt);
		UpdateCrouchCamera(dt);
	}

	private void HandleCrouch()
	{
		
		if (!IsOnFloor()) 
		{
			isCrouching = false;
		}
		else if (ToggleCrouch)
		{
			if (Input.IsActionJustPressed("crouch"))
				isCrouching = !isCrouching;
		}
		else
		{
			isCrouching = Input.IsActionPressed("crouch");
		}
	}

	private float HandleSpeed()
	{
		if (isCrouching)
			return CrouchSpeed;

		if (Input.IsActionPressed("sprint"))
			return SprintSpeed;

		return WalkSpeed;
	}

	private void UpdateCrouchCamera(float dt)
	{
		Vector3 target = pivotBasePosition;

		if (isCrouching)
			target.Y += CrouchHeightOffset;

		Pivot.Position = Pivot.Position.Lerp(target, CameraLerpSpeed * dt);
	}

	private void UpdateLean(float dt)
	{
		float targetTilt = 0f;
		float targetOffset = 0f;

		if (Input.IsActionPressed("lean_left"))
		{
			targetTilt = LeanAngle;
			targetOffset = -LeanOffset;
		}

		if (Input.IsActionPressed("lean_right"))
		{
			targetTilt = -LeanAngle;
			targetOffset = LeanOffset;
		}

		Vector3 rot = Pivot.RotationDegrees;
		rot.Z = Mathf.Lerp(rot.Z, targetTilt, LeanSpeed * dt);

		Vector3 pos = Pivot.Position;
		pos.X = Mathf.Lerp(pos.X, pivotBasePosition.X + targetOffset, LeanSpeed * dt);

		Pivot.RotationDegrees = rot;
		Pivot.Position = pos;
	}

	private void UpdateHeadBob(float dt)
	{
		
		if (!IsOnFloor() || !Input.IsActionPressed("sprint") || isCrouching)
			return;
		
		Vector2 vel = new Vector2(Velocity.X, Velocity.Z);

		if (vel.Length() < 0.1f)
			return;

		bobTime += dt * HeadBobFrequency;

		Vector3 pos = Pivot.Position;
		
		pos.X += pivotBasePosition.X + Mathf.Cos(bobTime * 0.75f) * HeadBobAmplitude * 1.5f;
		pos.Y += pivotBasePosition.Y + Mathf.Sin(bobTime * 1.5f)  * HeadBobAmplitude;

		Pivot.Position = pos;
	}
}
