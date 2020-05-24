using NAudio.Wave;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WASAPINETCore.Audio;
using WASAPINETCore.OpenGL;

namespace WASAPINETCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Audio fields:
        private WASAPIPlayer _player = null;
        private DispatcherTimer _timer;
        private LomontFFT _fft;
        private long _lastUpdate = 0;

        // OpenGL fields:
        private Matrix4 _pMatrix = Matrix4.Identity;
        private Matrix4 _vMatrix = Matrix4.LookAt(0, 0, 1, 0, 0, 0, 0, 1, 0);
        private Matrix4 _vpMatrix = Matrix4.Identity;
        private Renderer _renderer = null;
        private float _glWidth = 100f;
        private float _glHeight = 100f;
        


        public MainWindow()
        {
            InitializeComponent();

            Global.Watch.Start();

            _player = new WASAPIPlayer();
            _player.PlaybackStopped += _player_PlaybackStopped;

            _timer = new DispatcherTimer(DispatcherPriority.Normal);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            _timer.Tick += _ticker_Tick;

            _fft = new LomontFFT();
            _fft.A = 0;
            _fft.B = 1;
        }

        
        private void _ticker_Tick(object sender, EventArgs e)
        {
            
            if (_player.IsPlaying)
            {
                if (BufferingResults.Data != null)
                {
                    if(_lastUpdate <= BufferingResults.Timestamp)
                    {
                        int c = BufferingResults.Count;
                        byte[] data = BufferingResults.Data;
                        WaveFormat wf = BufferingResults.Format;

                        double[] dData = AudioConverter.ConvertToMonoDoubleFFTArray(data, wf);
                        _fft.FFT(dData, true);

                        double[] fftResult = new double[dData.Length / 2];

                        for (int i = 0, j = 0; i < dData.Length / 2; i += 2, j += 2)
                        {
                            fftResult[j] = i * wf.SampleRate / 2.0 / (dData.Length / 2);
                            fftResult[j + 1] = Math.Sqrt(dData[i] * dData[i] + dData[i + 1] * dData[i + 1]);
                        }
                        _renderer.SetFFTData(fftResult);
                    }
                    else
                    {
                        _renderer.ReduceCurrentFFTData();
                    }

                    glControl.Invalidate();
                }
            }
            else
            {
                //lblBytes.Content = "";
            }
            _lastUpdate = Global.Watch.ElapsedMilliseconds;
        }

        private void _player_PlaybackStopped(object sender, EventArgs e)
        {
            btnPlay.IsEnabled = true;
            _timer.Stop();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            bool result = _player.OpenWaveFile(@".\Samples\superstring_8bit_44100hz_stereo.wav");
            if (result)
            {
                btnPlay.IsEnabled = false;
                _timer.Start();
                _player.Play();
            }
            else
            {
                MessageBox.Show("Cannot open file.", "Error during playback", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (_player.IsPlaying)
            {
                _timer.Stop();
                _player.Pause();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_player.IsPlaying)
            {
                _timer.Stop();
                _player.Stop();
            }
        }

        private void glControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
            _renderer = new Renderer();
        }

        private void glControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _renderer.Draw(ref _vpMatrix, _glWidth, _glHeight);

            glControl.SwapBuffers();
        }

        private void glControlHost_Initialized(object sender, EventArgs e)
        {
            glControl.MakeCurrent();
        }

        private void glControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);

            _glWidth = 100f;
            _glHeight = 100f * (glControl.Height / (float)glControl.Width);

            _pMatrix = Matrix4.CreateOrthographic(_glWidth, _glHeight, 0, 100);
            _vpMatrix = _vMatrix * _pMatrix;
        }
    }
}
