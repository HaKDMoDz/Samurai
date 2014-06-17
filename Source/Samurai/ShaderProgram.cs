﻿using System;
using System.Collections.Generic;

namespace Samurai
{
	public sealed class ShaderProgram : DisposableObject
	{
		GraphicsContext graphicsDevice;
		VertexShader vertexShader;
		FragmentShader fragmentShader;

		internal uint Handle
		{
			get;
			private set;
		}
		
		public ShaderProgram(GraphicsContext graphics, VertexShader vertexShader, FragmentShader fragmentShader)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			if (vertexShader == null)
				throw new ArgumentNullException("vertexShader");

			if (fragmentShader == null)
				throw new ArgumentNullException("fragmentShader");

			this.graphicsDevice = graphics;
			this.Handle = GL.CreateProgram();

			this.vertexShader = vertexShader;
			GL.AttachShader(this.Handle, this.vertexShader.Handle);

			this.fragmentShader = fragmentShader;
			GL.AttachShader(this.Handle, this.fragmentShader.Handle);

			GL.LinkProgram(this.Handle);
		}

		~ShaderProgram()
		{
			this.Dispose(false);
		}

		protected override void DisposeManagedResources()
		{
			if (!this.vertexShader.IsDisposed)
				this.vertexShader.Dispose();

			if (!this.fragmentShader.IsDisposed)
				this.fragmentShader.Dispose();
		}

		protected override void DisposeUnmanagedResources()
		{
			GL.DeleteProgram(this.Handle);
		}
		
		public void SetValue(string name, float value)
		{
			int location = GL.GetUniformLocation(this.Handle, name);
			GL.Uniform1f(location, value);
		}

		public void SetValue(string name, ref Matrix4 value)
		{
			int location = GL.GetUniformLocation(this.Handle, name);
			GL.UniformMatrix4(location, ref value);
		}

		public void SetSampler(string name, Texture texture)
		{
			int location = GL.GetUniformLocation(this.Handle, name);
			GL.Uniform1i(location, (int)texture.Index);
		}
	}
}
