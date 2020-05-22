using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace WASAPINETCore.OpenGL
{
    class Quad
    {
        private int _vao = -1;
        private int _vboVertices = -1;
        private int _vboNormals = -1;
        private int _vboUVs = -1;

        private static float[] Vertices = new float[]
        {
            -0.5f,0,0,
            +0.5f,0,0,
            +0.5f,1,0,
            -0.5f,1,0
        };

        private static float[] Normals = new float[]
        {
            0,0,1,
            0,0,1,
            0,0,1,
            0,0,1
        };

        private static float[] UVs = new float[]
        {
            0,0,
            1,0,
            1,1,
            0,1
        };

        public Quad()
        {
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vboVertices = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboVertices);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * 4, Vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            _vboNormals = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboNormals);
            GL.BufferData(BufferTarget.ArrayBuffer, Normals.Length * 4, Normals, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(0);

            /*_vboUB = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboVertices);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * 4, Vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            */

        }
        
        public int GetId()
        {
            return _vao;
        }

    }
}
