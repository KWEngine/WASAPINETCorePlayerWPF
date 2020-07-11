using Microsoft.Win32;
using NAudio.Dsp;
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
            _player.FftCalculated += _player_FftCalculated;
            _player.MaximumCalculated += _player_MaximumCalculated;

            _timer = new DispatcherTimer(DispatcherPriority.Normal);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            _timer.Tick += _ticker_Tick;

        }

        private void _player_MaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            
        }

        private void _player_FftCalculated(object sender, FftEventArgs e)
        {
            BufferingResults.Data = e.Result;
            BufferingResults.Count = e.Result.Length;
            BufferingResults.Format = _player.WaveFormat;
            BufferingResults.Timestamp = (uint)Global.Watch.ElapsedMilliseconds;
            //Console.WriteLine("!");
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
                        double[] data = BufferingResults.Data;
                        double[] fftResult = new double[BufferingResults.Data.Length];

                        for (int i = 0, j = 0; i < data.Length; i += 2, j += 2)
                        {
                            fftResult[j] = i * BufferingResults.Format.SampleRate / 2.0 / (data.Length);
                            fftResult[j + 1] = Math.Sqrt(data[i] * data[i] + data[i + 1] * data[i + 1]);
                        }
                        _renderer.SetFFTData(fftResult);
                        //Console.WriteLine("max: " + fftResult[fftResult.Length - 2]);
                        //Console.WriteLine("min: " + fftResult[2]);
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
                bool result = _renderer.ReduceCurrentFFTData();
                glControl.Invalidate();
                if (!result)
                {
                    _timer.Stop();
                }
            }
            _lastUpdate = Global.Watch.ElapsedMilliseconds;
        }

        private void _player_PlaybackStopped(object sender, EventArgs e)
        {
            btnPlay.IsEnabled = true;
            btnPause.IsEnabled = false;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void Play()
        {
            btnPlay.IsEnabled = false;
            btnPause.IsEnabled = true;
            _timer.Start();
            _player.Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_player.IsPlaying || _player.IsPaused)
            {
                _player.Stop();
                _player.Dispose();
                _timer.Stop();
              
            }
        }

        private void Pause()
        {
            if (_player.IsPlaying)
            {
                _timer.Stop();
                _player.Pause();
                btnPause.IsEnabled = false;
                btnPlay.IsEnabled = true;
            }
        }

        private void glControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
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

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            bool? result = fd.ShowDialog(this);
            if(result != null && result == true)
            {
                tbFilename.Text = fd.FileName;
                _player.Stop();
                FileDetails fileLoaded = _player.OpenWaveFile(tbFilename.Text.Trim());
                if (fileLoaded != null)
                {
                    lblArtistFill.Content = fileLoaded.Artist;
                    lblTitleFill.Content = fileLoaded.Title;
                    lblAlbumFill.Content = fileLoaded.Album;

                    if (fileLoaded.StreamOK)
                    {
                        btnPlay.IsEnabled = true;
                        btnPause.IsEnabled = false;
                        return;
                    }
                }
                else
                {
                    tbFilename.Text = "";
                    btnPlay.IsEnabled = false;
                    btnPause.IsEnabled = false;
                }
                MessageBox.Show("Cannot open file.", "Error during playback", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
    }
}
