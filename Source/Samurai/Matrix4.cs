﻿using System;
using System.Runtime.InteropServices;

namespace Samurai
{
	public struct Matrix4
	{
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Matrix4));

		public static readonly Matrix4 Zero = new Matrix4();

		public static readonly Matrix4 Identity = new Matrix4() { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f, M44 = 1.0f };

		public static readonly Matrix4 InvertedYAxis = new Matrix4() { M11 = 1.0f, M22 = -1.0f, M33 = 1.0f, M44 = 1.0f };

		public float M11;
		public float M12;
		public float M13;
		public float M14;
		public float M21;
		public float M22;
		public float M23;
		public float M24;
		public float M31;
		public float M32;
		public float M33;
		public float M34;
		public float M41;
		public float M42;
		public float M43;
		public float M44;

		public Matrix4(float m11, float m12, float m13, float m14,
					  float m21, float m22, float m23, float m24,
					  float m31, float m32, float m33, float m34,
					  float m41, float m42, float m43, float m44)
		{
			this.M11 = m11;
			this.M12 = m12;
			this.M13 = m13;
			this.M14 = m14;
			this.M21 = m21;
			this.M22 = m22;
			this.M23 = m23;
			this.M24 = m24;
			this.M31 = m31;
			this.M32 = m32;
			this.M33 = m33;
			this.M34 = m34;
			this.M41 = m41;
			this.M42 = m42;
			this.M43 = m43;
			this.M44 = m44;
		}

		public static Matrix4 CreateRotationX(float radians)
		{
			Matrix4 returnMatrix = Matrix4.Identity;

			returnMatrix.M22 = (float)Math.Cos(radians);
			returnMatrix.M23 = (float)Math.Sin(radians);
			returnMatrix.M32 = -returnMatrix.M23;
			returnMatrix.M33 = returnMatrix.M22;

			return returnMatrix;
		}

		public static void CreateRotationX(float radians, out Matrix4 result)
		{
			result = Matrix4.Identity;

			result.M22 = (float)Math.Cos(radians);
			result.M23 = (float)Math.Sin(radians);
			result.M32 = -result.M23;
			result.M33 = result.M22;
		}

		public static Matrix4 CreateRotationY(float radians)
		{
			Matrix4 returnMatrix = Matrix4.Identity;

			returnMatrix.M11 = (float)Math.Cos(radians);
			returnMatrix.M13 = (float)Math.Sin(radians);
			returnMatrix.M31 = -returnMatrix.M13;
			returnMatrix.M33 = returnMatrix.M11;

			return returnMatrix;
		}

		public static void CreateRotationY(float radians, out Matrix4 result)
		{
			result = Matrix4.Identity;

			result.M11 = (float)Math.Cos(radians);
			result.M13 = (float)Math.Sin(radians);
			result.M31 = -result.M13;
			result.M33 = result.M11;
		}

		public static Matrix4 CreateRotationZ(float radians)
		{
			Matrix4 returnMatrix = Matrix4.Identity;

			returnMatrix.M11 = (float)Math.Cos(radians);
			returnMatrix.M12 = (float)Math.Sin(radians);
			returnMatrix.M21 = -returnMatrix.M12;
			returnMatrix.M22 = returnMatrix.M11;

			return returnMatrix;
		}

		public static void CreateRotationZ(float radians, out Matrix4 result)
		{
			result = Matrix4.Identity;

			result.M11 = (float)Math.Cos(radians);
			result.M12 = (float)Math.Sin(radians);
			result.M21 = -result.M12;
			result.M22 = result.M11;
		}

		public static Matrix4 CreateScale(float scale)
		{
			Matrix4 returnMatrix = Matrix4.Identity;

			returnMatrix.M11 = scale;
			returnMatrix.M22 = scale;
			returnMatrix.M33 = scale;

			return returnMatrix;
		}

		public static void CreateScale(float scale, out Matrix4 result)
		{
			result = Matrix4.Identity;

			result.M11 = scale;
			result.M22 = scale;
			result.M33 = scale;
		}

		public static Matrix4 CreateScale(float xScale, float yScale, float zScale)
		{
			Matrix4 returnMatrix = Matrix4.Identity;

			returnMatrix.M11 = xScale;
			returnMatrix.M22 = yScale;
			returnMatrix.M33 = zScale;

			return returnMatrix;
		}

		public static void CreateScale(float xScale, float yScale, float zScale, out Matrix4 result)
		{
			result = Matrix4.Identity;

			result.M11 = xScale;
			result.M22 = yScale;
			result.M33 = zScale;
		}

		public static Matrix4 CreateScale(Vector2 scales)
		{
			Matrix4 returnMatrix = Matrix4.Identity;

			returnMatrix.M11 = scales.X;
			returnMatrix.M22 = scales.Y;

			return returnMatrix;
		}

		public static void CreateScale(ref Vector2 scales, out Matrix4 result)
		{
			result = Matrix4.Identity;

			result.M11 = scales.X;
			result.M22 = scales.Y;
		}

		public static Matrix4 CreateScale(Vector3 scales)
		{
			Matrix4 returnMatrix = Matrix4.Identity;

			returnMatrix.M11 = scales.X;
			returnMatrix.M22 = scales.Y;
			returnMatrix.M33 = scales.Z;

			return returnMatrix;
		}

		public static void CreateScale(ref Vector3 scales, out Matrix4 result)
		{
			result = Matrix4.Identity;

			result.M11 = scales.X;
			result.M22 = scales.Y;
			result.M33 = scales.Z;
		}

		public static Matrix4 CreateTranslation(float xPosition, float yPosition)
		{
			Matrix4 returnMatrix = Matrix4.Identity;

			returnMatrix.M41 = xPosition;
			returnMatrix.M42 = yPosition;

			return returnMatrix;
		}

		public static void CreateTranslation(float xPosition, float yPosition, out Matrix4 result)
		{
			result = Matrix4.Identity;

			result.M41 = xPosition;
			result.M42 = yPosition;
		}

		public static Matrix4 CreateTranslation(float xPosition, float yPosition, float zPosition)
		{
			Matrix4 returnMatrix = Matrix4.Identity;

			returnMatrix.M41 = xPosition;
			returnMatrix.M42 = yPosition;
			returnMatrix.M43 = zPosition;

			return returnMatrix;
		}

		public static void CreateTranslation(float xPosition, float yPosition, float zPosition, out Matrix4 result)
		{
			result = Matrix4.Identity;

			result.M41 = xPosition;
			result.M42 = yPosition;
			result.M43 = zPosition;
		}

		public static Matrix4 CreateTranslation(Vector2 position)
		{
			Matrix4 returnMatrix = Matrix4.Identity;

			returnMatrix.M41 = position.X;
			returnMatrix.M42 = position.Y;

			return returnMatrix;
		}

		public static void CreateTranslation(ref Vector2 position, out Matrix4 result)
		{
			result = Matrix4.Identity;

			result.M41 = position.X;
			result.M42 = position.Y;
		}

		public static Matrix4 CreateTranslation(Vector3 position)
		{
			Matrix4 returnMatrix = Matrix4.Identity;

			returnMatrix.M41 = position.X;
			returnMatrix.M42 = position.Y;
			returnMatrix.M43 = position.Z;

			return returnMatrix;
		}

		public static void CreateTranslation(ref Vector3 position, out Matrix4 result)
		{
			result = Matrix4.Identity;

			result.M41 = position.X;
			result.M42 = position.Y;
			result.M43 = position.Z;
		}

		public static void InvertYAxis(ref Matrix4 matrix)
		{
			matrix.M22 = -matrix.M22;
		}

		/// <summary>
		/// Determines the product of two matrices.
		/// </summary>
		/// <param name="left">The first matrix to multiply.</param>
		/// <param name="right">The second matrix to multiply.</param>
		/// <param name="result">The product of the two matrices.</param>
		public static void Multiply(ref Matrix4 left, ref Matrix4 right, out Matrix4 result)
		{
			result = new Matrix4();
			result.M11 = (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31) + (left.M14 * right.M41);
			result.M12 = (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32) + (left.M14 * right.M42);
			result.M13 = (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33) + (left.M14 * right.M43);
			result.M14 = (left.M11 * right.M14) + (left.M12 * right.M24) + (left.M13 * right.M34) + (left.M14 * right.M44);
			result.M21 = (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31) + (left.M24 * right.M41);
			result.M22 = (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32) + (left.M24 * right.M42);
			result.M23 = (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33) + (left.M24 * right.M43);
			result.M24 = (left.M21 * right.M14) + (left.M22 * right.M24) + (left.M23 * right.M34) + (left.M24 * right.M44);
			result.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31) + (left.M34 * right.M41);
			result.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32) + (left.M34 * right.M42);
			result.M33 = (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33) + (left.M34 * right.M43);
			result.M34 = (left.M31 * right.M14) + (left.M32 * right.M24) + (left.M33 * right.M34) + (left.M34 * right.M44);
			result.M41 = (left.M41 * right.M11) + (left.M42 * right.M21) + (left.M43 * right.M31) + (left.M44 * right.M41);
			result.M42 = (left.M41 * right.M12) + (left.M42 * right.M22) + (left.M43 * right.M32) + (left.M44 * right.M42);
			result.M43 = (left.M41 * right.M13) + (left.M42 * right.M23) + (left.M43 * right.M33) + (left.M44 * right.M43);
			result.M44 = (left.M41 * right.M14) + (left.M42 * right.M24) + (left.M43 * right.M34) + (left.M44 * right.M44);
		}

		/// <summary>
		/// Determines the product of two matrices.
		/// </summary>
		/// <param name="left">The first matrix to multiply.</param>
		/// <param name="right">The second matrix to multiply.</param>
		/// <returns>The product of the two matrices.</returns>
		public static Matrix4 Multiply(Matrix4 left, Matrix4 right)
		{
			Matrix4 result;
			Multiply(ref left, ref right, out result);
			return result;
		}

		public static Matrix4 Perspective(float left, float right, float top, float bottom, float near, float far)
		{
			float range = far / (far - near);

			return new Matrix4()
			{
				M11 = 2.0f * near / (right - left),
				M22 = 2.0f * near / (top - bottom),
				M31 = (left + right) / (left - right),
				M32 = (top + bottom) / (bottom - top),
				M33 = range,
				M34 = 1.0f,
				M43 = -near * range
			};
		}

		public static Matrix4 PerspectiveFOV(float fov, float aspect, float near, float far)
		{
			float yScale = (float)(1.0 / Math.Tan(fov * 0.5f));
			float xScale = yScale / aspect;

			float halfWidth = near / xScale;
			float halfHeight = near / yScale;

			return Perspective(-halfWidth, halfWidth, -halfHeight, halfHeight, near, far);
		}

		public static Matrix4 LookAt(ref Vector3 eye, ref Vector3 target, ref Vector3 up)
		{
			Vector3 xAxis, yAxis, zAxis;
			
			Vector3.Subtract(ref target, ref eye, out zAxis);
			zAxis.Normalize();
			
			Vector3.Cross(ref up, ref zAxis, out xAxis);
			xAxis.Normalize();
			
			Vector3.Cross(ref zAxis, ref xAxis, out yAxis);

			Matrix4 result = Matrix4.Identity;
			
			result.M11 = xAxis.X;
			result.M21 = xAxis.Y;
			result.M31 = xAxis.Z;
			
			result.M12 = yAxis.X;
			result.M22 = yAxis.Y;
			result.M32 = yAxis.Z;
			
			result.M13 = zAxis.X;
			result.M23 = zAxis.Y;
			result.M33 = zAxis.Z;
			
			result.M41 = -Vector3.Dot(ref xAxis, ref eye);
			result.M42 = -Vector3.Dot(ref yAxis, ref eye);
			result.M43 = -Vector3.Dot(ref zAxis, ref eye);

			return result;
		}

		public static Matrix4 operator *(Matrix4 matrix1, Matrix4 matrix2)
		{
			Matrix4 returnMatrix = new Matrix4();
			Matrix4.Multiply(ref matrix1, ref matrix2, out returnMatrix);
			return returnMatrix;
		}
	}
}
