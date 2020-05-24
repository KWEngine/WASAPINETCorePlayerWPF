using NAudio.Wave;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Documents;

namespace WASAPINETCore.OpenGL
{
    class Renderer
    {
        private int _program = -1;
        private int _vertexProgram = -1;
        private int _fragmentProgram = -1;

        private int _uniformVP = -1;
        private int _uniformBaseColor = -1;
        private int _uniformHeight = -1;
        private int _uniformWindowHeight = -1;
        private int _uniformBinOffset = -1;
        private int _uniformBinWidth = -1;
        private int _uniformStep = -1;

        private Quad _quad;
        private double[] _fft;

        public void SetFFTData(double[] data)
        {
            _fft = data;
        }

        public void ReduceCurrentFFTData()
        {
            if(_fft != null)
            {
                for (int i = 0; i < _fft.Length; i += 2)
                {
                    _fft[i + 1] /= 2.0;
                }
            }
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


            //_uniformMVP = GL.GetUniformLocation(_program, "uMVP");
            _uniformVP = GL.GetUniformLocation(_program, "uVP");
            _uniformBaseColor = GL.GetUniformLocation(_program, "uBaseColor");
            _uniformHeight = GL.GetUniformLocation(_program, "uHeight");
            _uniformWindowHeight = GL.GetUniformLocation(_program, "uWindowHeight");
            _uniformBinOffset = GL.GetUniformLocation(_program, "uBinOffset");
            _uniformBinWidth = GL.GetUniformLocation(_program, "uBinWidth");
            _uniformStep = GL.GetUniformLocation(_program, "uStep");

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

        private void PrepareDataForDrawing(float width, out float step, out int numberOfBins, out float[] heights, out float beginning )
        {
            step = 0;
            heights = new float[20];
            numberOfBins = 0;
            beginning = 0;

            if (_fft != null)
            {
                numberOfBins = _fft.Length / 2 - 2;
                int bins2 = numberOfBins * 2;
                step = width / (20 - 0);
                beginning = (-width / 2f) + (step / 2f);
                int binsPerSuperBin = numberOfBins / 20;
                if (binsPerSuperBin < 1)
                {
                    return;
                }
                    

                float currentSum = 0;
                for (int i = 2, j = 0, b = 0; i < bins2; i += 2, j++)
                {
                    float h = Math.Clamp(100 + (20f * (float)Math.Log10(_fft[i + 1] / (numberOfBins * 0.5f))), 0f, 100f);
                    currentSum += h;
                    if (j > 0 && j % binsPerSuperBin == 0)
                    {
                        float avg = currentSum / binsPerSuperBin;
                        heights[b] = avg;
                        b++;
                        currentSum = 0;
                        if (b >= 20)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void Draw(ref Matrix4 viewProjection, float width, float height)
        {
            PrepareDataForDrawing(width, out float step, out int numberOfBins, out float[] heights, out float beginning);

            GL.Uniform1(_uniformBinWidth, 100f / (20f + 1f));
            GL.Uniform1(_uniformStep, step);
            GL.Uniform1(_uniformWindowHeight, -height / 2f);
            GL.Uniform1(_uniformBinOffset, beginning);
            GL.UniformMatrix4(_uniformVP, false, ref viewProjection);
            GL.Uniform1(_uniformHeight, 20, heights);

            GL.BindVertexArray(_quad.GetId());
            GL.DrawArraysInstanced(PrimitiveType.Quads, 0, 4, numberOfBins);
            GL.BindVertexArray(0);
            
        }
    }
}