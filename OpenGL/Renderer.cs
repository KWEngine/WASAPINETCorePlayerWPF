using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using System.Reflection;

namespace WASAPINETCore.OpenGL
{
    class Renderer
    {
        private int _program = -1;
        private int _vertexProgram = -1;
        private int _fragmentProgram = -1;

        private int _uniformMVP = -1;
        private int _uniformBaseColor = -1;

        private Quad _quad;
        private double[] _fft;

        public void SetFFTData(double[] data)
        {
            _fft = data;
        }

        public Renderer()
        {
            _program = GL.CreateProgram();

            string resourceNameFragmentShader = "WASAPINETCore.OpenGL.Shaders.shader_fragment_simple.glsl";
            string resourceNameVertexShader = "WASAPINETCore.OpenGL.Shaders.shader_vertex_simple.glsl";
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
            {
                _vertexProgram = LoadShader(s, ShaderType.VertexShader, _program);
            }

            using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
            {
                _fragmentProgram = LoadShader(s, ShaderType.FragmentShader, _program);
            }

            if (_vertexProgram >= 0 && _fragmentProgram >= 0)
            {
                GL.BindAttribLocation(_program, 0, "aPosition");

                GL.BindFragDataLocation(_program, 0, "color");
                //GL.BindFragDataLocation(_program, 1, "bloom");
                GL.LinkProgram(_program);
            }
            else
            {
                throw new Exception("Creating and linking shaders failed.");
            }


            _uniformMVP = GL.GetUniformLocation(_program, "uMVP");
            _uniformBaseColor = GL.GetUniformLocation(_program, "uBaseColor");

            GL.UseProgram(_program);

            _quad = new Quad();
        }

        public void Dispose()
        {
            if (_program >= 0)
            {
                GL.DeleteProgram(_program);
                GL.DeleteShader(_vertexProgram);
                GL.DeleteShader(_fragmentProgram);
                _program = -1;
            }
        }

        protected int LoadShader(Stream pFileStream, ShaderType pType, int pProgram)
        {
            int address = GL.CreateShader(pType);
            using (StreamReader sr = new StreamReader(pFileStream))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(pProgram, address);
            return address;
        }

        public void Draw(ref Matrix4 viewProjection, float width, float height)
        {
            if (_fft != null)
            {
                GL.BindVertexArray(_quad.GetId());
                int numberOfBins = _fft.Length / 2 - 2;
                float j = -100;
                float step = (width * 2) / numberOfBins;
                for (int i = 2; i < numberOfBins; i++)
                {
                    float h = ((float)_fft[i * 2 + 1]) * 10f;
                    Matrix4 model = Matrix4.CreateScale(0.5f, h, 1f);
                    model[3, 0] = j;
                    model[3, 1] = -height / 2;
                    Matrix4 mvp = model * viewProjection;
                    GL.UniformMatrix4(_uniformMVP, false, ref mvp);

                    GL.Uniform3(_uniformBaseColor, 1f, 1f, 1f);

                    GL.DrawArrays(PrimitiveType.Quads, 0, 4);

                    j += step;
                }

                GL.BindVertexArray(0);
            }
        }
    }
}