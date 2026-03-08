using Godot;
using System;

public partial class Player : Camera3D
{
	[Export] public float MouseSensitivity = 0.002f;
	[Export] public float StickSensitivity = 3.0f;
	[Export] public float PitchLimit = 85.0f;

	private float yaw = 0f;
	private float pitch = 0f;

	public override void _Ready()
	{	
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	
	public override void _Input(InputEvent @event)
	{
		
		// quits session
		if (@event.IsActionPressed("ui_cancel"))
		{
			GetTree().Quit();
		}
		
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			yaw -= mouseMotion.Relative.X * MouseSensitivity;
			pitch -= mouseMotion.Relative.Y * MouseSensitivity;

			pitch = Mathf.Clamp(
				pitch,
				Mathf.DegToRad(-PitchLimit),
				Mathf.DegToRad(PitchLimit)
			);
		}
	}

	public override void _Process(double delta)
	{
		float lookX =
			Input.GetActionStrength("look_right") -
			Input.GetActionStrength("look_left");

		float lookY =
			Input.GetActionStrength("look_down") -
			Input.GetActionStrength("look_up");

		yaw -= lookX * StickSensitivity * (float)delta;
		pitch -= lookY * StickSensitivity * (float)delta;

		pitch = Mathf.Clamp(
			pitch,
			Mathf.DegToRad(-PitchLimit),
			Mathf.DegToRad(PitchLimit)
		);

		Rotation = new Vector3(pitch, yaw, 0);
	}
}
