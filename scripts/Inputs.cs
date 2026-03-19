using Godot;
using System;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace InputSystem
{
	public enum InputState
	{
		Idle,
		Pressed,
		Held,
		Released
	}

	public class InputStates
	{
		public InputState State;
		public bool IsIdle => State == InputState.Idle;
		public bool IsPressed => State == InputState.Pressed;
		public bool IsHeld => State == InputState.Held;
		public bool IsReleased => State == InputState.Released;
	}

	public class InputManager
	{
		// Basic Action
		public InputStates Crouch;
		public InputStates Jump;

		// Basic Movement
		public InputStates Forward;
		public InputStates Backward;
		public InputStates StrafeLeft;
		public InputStates StrafeRight;

		// Speed Modifiers
		public InputStates Sneak;
		public InputStates Sprint;

		public bool IsMoving()
		{
			if (!Forward.IsIdle || !Backward.IsIdle || !StrafeLeft.IsIdle || !StrafeRight.IsIdle)
				return true;
			return false;
		}
		
		public bool IsSneaking()
		{
			if (IsMoving() && Sprint.IsIdle && !Sneak.IsIdle)
				return true;
			return false;
		}

		public bool IsWalking()
		{
			if (IsMoving() && Sprint.IsIdle && Sneak.IsIdle)
				return true;
			return false;
		}

	}
}










public partial class Inputs : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
