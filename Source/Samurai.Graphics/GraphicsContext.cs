﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Samurai.Graphics
{
	public class GraphicsContext : DisposableObject
	{
        IHostControl host;

		Color4 clearColor;
		Rectangle viewport;
		Rectangle? scissor;

		BlendState blendState;
		DepthBufferState depthState;
		RasterizerState rasterizerState;
								
		bool[] textures;
		uint nextTexture;

		List<GraphicsObject> graphicsObjects;

		internal GLContext GL
		{
			get;
			private set;
		}

		internal bool IsDisposing
		{
			get;
			private set;
		}

		public Rectangle Viewport
		{
			get { return this.viewport; }
			
			set
			{
				this.viewport = value;
                this.GL.Viewport(value.X, this.host.Height - value.Y - value.Height, value.Width, value.Height);
			}
		}

		public Rectangle? Scissor
		{
			get { return this.scissor; }

			set
			{
				this.scissor = value;

				if (this.scissor.HasValue)
				{
					this.ToggleCap(GLContext.ScissorTestCap, true);
					this.GL.Scissor(this.scissor.Value.X, this.host.Height - value.Value.Y - value.Value.Height, value.Value.Width, value.Value.Height);
				}
				else
				{
					this.ToggleCap(GLContext.ScissorTestCap, false);
				}
			}
		}

		public BlendState BlendState
		{
			get { return this.blendState; }

			set
			{
				if (value == null)
					throw new ArgumentNullException("BlendState");

				if (value != this.blendState)
				{
					this.blendState = value;
					this.ApplyBlendState();					
				}
			}
		}

		public DepthBufferState DepthBufferState
		{
			get { return this.depthState; }

			set
			{
				if (value == null)
					throw new ArgumentNullException("DepthState");

				if (value != this.depthState)
				{
					this.depthState = value;
					this.ApplyDepthBufferState();
				}
			}
		}

		public RasterizerState RasterizerState
		{
			get { return this.rasterizerState; }

			set
			{
				if (value == null)
					throw new ArgumentNullException("RasterizerState");

				if (value != this.rasterizerState)
				{
					this.rasterizerState = value;
					this.ApplyRasterizerState();
				}
			}
		}
								
		public GraphicsContext(IHostControl host)
		{
            if (host == null)
                throw new ArgumentNullException("host");

            this.host = host;

			this.GL = new GLContext(this.host.Handle);

			this.textures = new bool[32];

			this.scissor = null;
			this.ToggleCap(GLContext.ScissorTestCap, false);

			this.blendState = BlendState.Disabled;
			this.ApplyBlendState();

			this.depthState = DepthBufferState.Disabled;
			this.ApplyDepthBufferState();

			this.rasterizerState = RasterizerState.Default;
			this.ApplyRasterizerState();
						
			this.clearColor = Color4.CornflowerBlue;
			this.GL.ClearColor(this.clearColor.R / 255.0f, this.clearColor.G / 255.0f, this.clearColor.B / 255.0f, this.clearColor.A / 255.0f);
												
			this.graphicsObjects = new List<GraphicsObject>();
		}

		~GraphicsContext()
		{
			this.Dispose(false);
		}

		protected override void Dispose(bool disposing)
		{
			this.IsDisposing = true;
			base.Dispose(disposing);
		}

		protected override void DisposeManagedResources()
		{
			foreach (GraphicsObject obj in this.graphicsObjects)
			{
				if (!obj.IsDisposed)
					obj.Dispose();
			}

			this.graphicsObjects.Clear();

			this.GL.Dispose();
		}
		
		internal void RegisterGraphicsObject(GraphicsObject obj)
		{
			this.graphicsObjects.Add(obj);
		}

		internal void UnregisterGraphicsObject(GraphicsObject obj)
		{
			this.graphicsObjects.Remove(obj);
		}

		internal uint AllocateTextureIndex()
		{
			for (uint i = 0; i < this.textures.Length; i++)
			{
				uint index = this.nextTexture + i;

				if (index >= this.textures.Length)
					index -= (uint)this.textures.Length;

				if (!this.textures[index])
				{					
					this.textures[index] = true;
					this.nextTexture = index + 1;

					if (this.nextTexture >= this.textures.Length)
						this.nextTexture -= (uint)this.textures.Length;
					
					return index;
				}
			}

			throw new SamuraiException("Maximum number of Textures allocated. Dispose of at least one Texture before allocating another.");
		}

		internal void DeallocateTextureIndex(uint index)
		{
			this.textures[index] = false;
		}
		
		private void ToggleCap(uint cap, bool isEnabled)
		{
			if (isEnabled)
			{
				this.GL.Enable(cap);
			}
			else
			{
				this.GL.Disable(cap);
			}
		}

		private void ApplyBlendState()
		{
			this.ToggleCap(GLContext.BlendCap, this.blendState.Enabled);

			if (this.blendState.Enabled)
				this.GL.BlendFunc((uint)this.blendState.SourceFactor, (uint)this.blendState.DestinationFactor);
		}
				
		private void ApplyDepthBufferState()
		{
			this.ToggleCap(GLContext.DepthTestCap, this.depthState.Enabled);

			if (this.depthState.Enabled)
				this.GL.DepthFunc((uint)this.depthState.DepthFunc);
		}

		private void ApplyRasterizerState()
		{
			this.GL.FrontFace((uint)this.rasterizerState.FrontFace);

			if (this.rasterizerState.CullMode == CullMode.None)
			{
				this.ToggleCap(GLContext.CullFaceCap, false);
			}
			else
			{
				this.ToggleCap(GLContext.CullFaceCap, true);
				this.GL.CullFace((uint)this.rasterizerState.CullMode);
			}
		}

		public void Clear(Color4 color)
		{
			if (!color.Equals(this.clearColor))
			{
				this.clearColor = color;
				this.GL.ClearColor(this.clearColor.R / 255.0f, this.clearColor.G / 255.0f, this.clearColor.B / 255.0f, this.clearColor.A / 255.0f);
			}

			this.GL.Clear(GLContext.ColorBufferBit | GLContext.DepthBufferBit);
		}

		public void SwapBuffers()
		{
			this.GL.SwapBuffers();
		}
				
		public void SetShaderProgram(ShaderProgram shader)
		{
			if (shader == null)
				throw new ArgumentNullException("shader");

			this.GL.UseProgram(shader.Handle);
		}

		public void Draw<T>(PrimitiveType type, VertexBuffer<T> vertexBuffer)
			where T : struct
		{
			if (vertexBuffer == null)
				throw new ArgumentNullException("vertexBuffer");

			this.Draw(type, vertexBuffer, 0, vertexBuffer.Count);
		}

		public void Draw<T>(PrimitiveType type, VertexBuffer<T> vertexBuffer, int startVertex, int vertexCount)
			where T : struct
		{
			if (vertexBuffer == null)
				throw new ArgumentNullException("vertexBuffer");

			this.GL.BindVertexArray(vertexBuffer.vertexArray);
			this.GL.DrawArrays((uint)type, startVertex, vertexCount);
		}

		public void Draw<TVertex, TIndex>(PrimitiveType type, VertexBuffer<TVertex> vertexBuffer, IndexBuffer<TIndex> indexBuffer)
			where TVertex : struct
			where TIndex : struct
		{
			if (vertexBuffer == null)
				throw new ArgumentNullException("vertexBuffer");

			if (indexBuffer == null)
				throw new ArgumentNullException("indexBuffer");

			this.Draw(type, vertexBuffer, indexBuffer, 0, indexBuffer.Count);
		}

		public void Draw<TVertex, TIndex>(PrimitiveType type, VertexBuffer<TVertex> vertexBuffer, IndexBuffer<TIndex> indexBuffer, int startIndex, int indexCount)
			where TVertex : struct
			where TIndex : struct
		{
			if (vertexBuffer == null)
				throw new ArgumentNullException("vertexBuffer");

			if (indexBuffer == null)
				throw new ArgumentNullException("indexBuffer");

			int sizeOfTIndex = Marshal.SizeOf(typeof(TIndex));

			this.GL.BindVertexArray(vertexBuffer.vertexArray);
			this.GL.BindBuffer(GLContext.ElementArrayBuffer, indexBuffer.buffer);
			this.GL.DrawElements((uint)type, indexCount, indexBuffer.dataType, (IntPtr)(startIndex * sizeOfTIndex));
		}
	}
}
