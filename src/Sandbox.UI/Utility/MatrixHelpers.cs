using System.Numerics;

namespace Sandbox.UI;

/// <summary>
/// Matrix helper utilities for CSS transforms
/// </summary>
public static class MatrixHelpers
{
	/// <summary>
	/// Transform a 2D point by a 4x4 matrix.
	/// </summary>
	public static Vector2 Transform(this Matrix4x4 matrix, Vector2 point)
	{
		// Transform as a 3D point with z=0, w=1
		var vec3 = Vector3.Transform(new Vector3(point.x, point.y, 0), matrix);
		return new Vector2(vec3.X, vec3.Y);
	}

	/// <summary>
	/// Get the inverse of a matrix.
	/// </summary>
	public static Matrix4x4 Inverted(this Matrix4x4 matrix)
	{
		Matrix4x4.Invert(matrix, out var result);
		return result;
	}

	/// <summary>
	/// Create a 4x4 matrix from 2D matrix values (6 values)
	/// </summary>
	public static Matrix4x4 CreateMatrix(float[] values)
	{
		if (values.Length < 6)
			return Matrix4x4.Identity;
			
		// 2D affine transformation matrix:
		// | a  c  tx |
		// | b  d  ty |
		// | 0  0  1  |
		
		return new Matrix4x4(
			values[0], values[1], 0, 0,  // a, b
			values[2], values[3], 0, 0,  // c, d
			0, 0, 1, 0,
			values[4], values[5], 0, 1   // tx, ty
		);
	}
	
	/// <summary>
	/// Create a 4x4 matrix from 3D matrix values (16 values)
	/// </summary>
	public static Matrix4x4 CreateMatrix3D(float[] values)
	{
		if (values.Length < 16)
			return Matrix4x4.Identity;
			
		return new Matrix4x4(
			values[0], values[1], values[2], values[3],
			values[4], values[5], values[6], values[7],
			values[8], values[9], values[10], values[11],
			values[12], values[13], values[14], values[15]
		);
	}
	
	/// <summary>
	/// Linear interpolation between two matrices
	/// </summary>
	public static Matrix4x4 Lerp(Matrix4x4 a, Matrix4x4 b, float t)
	{
		t = Math.Clamp(t, 0f, 1f);
		
		return new Matrix4x4(
			MathX.Lerp(a.M11, b.M11, t), MathX.Lerp(a.M12, b.M12, t), MathX.Lerp(a.M13, b.M13, t), MathX.Lerp(a.M14, b.M14, t),
			MathX.Lerp(a.M21, b.M21, t), MathX.Lerp(a.M22, b.M22, t), MathX.Lerp(a.M23, b.M23, t), MathX.Lerp(a.M24, b.M24, t),
			MathX.Lerp(a.M31, b.M31, t), MathX.Lerp(a.M32, b.M32, t), MathX.Lerp(a.M33, b.M33, t), MathX.Lerp(a.M34, b.M34, t),
			MathX.Lerp(a.M41, b.M41, t), MathX.Lerp(a.M42, b.M42, t), MathX.Lerp(a.M43, b.M43, t), MathX.Lerp(a.M44, b.M44, t)
		);
	}
	
	/// <summary>
	/// Create a rotation matrix from euler angles (in degrees)
	/// </summary>
	public static Matrix4x4 CreateRotation(Vector3 eulerDegrees)
	{
		// Convert degrees to radians
		float pitch = MathX.DegreeToRadian(eulerDegrees.X);
		float yaw = MathX.DegreeToRadian(eulerDegrees.Y);
		float roll = MathX.DegreeToRadian(eulerDegrees.Z);
		
		// Create rotation matrix from Euler angles (XYZ order)
		return Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);
	}
}
