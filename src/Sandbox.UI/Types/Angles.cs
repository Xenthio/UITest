using System;
using System.Numerics;

namespace Sandbox.UI;

/// <summary>
/// Represents rotation angles (pitch, yaw, roll) in degrees
/// </summary>
public struct Angles
{
	public float Pitch;
	public float Yaw;
	public float Roll;
	
	public Angles(float pitch, float yaw, float roll)
	{
		Pitch = pitch;
		Yaw = yaw;
		Roll = roll;
	}
	
	public static Angles Zero => new Angles(0, 0, 0);
	
	/// <summary>
	/// Linear interpolation between two angles
	/// </summary>
	public static Angles Lerp(Angles a, Angles b, float t)
	{
		return new Angles(
			MathX.Lerp(a.Pitch, b.Pitch, t),
			MathX.Lerp(a.Yaw, b.Yaw, t),
			MathX.Lerp(a.Roll, b.Roll, t)
		);
	}
	
	/// <summary>
	/// Convert angles to a rotation matrix
	/// </summary>
	public Matrix4x4 ToMatrix()
	{
		// Convert degrees to radians
		float pitch = Pitch * (MathF.PI / 180f);
		float yaw = Yaw * (MathF.PI / 180f);
		float roll = Roll * (MathF.PI / 180f);
		
		return Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);
	}
	
	/// <summary>
	/// Convert angles to Vector3
	/// </summary>
	public Vector3 AsVector3()
	{
		return new Vector3(Pitch, Yaw, Roll);
	}
}
