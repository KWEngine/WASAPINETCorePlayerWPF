using NAudio.Wave;
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

namespace WASAPINETCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WASAPIPlayer _player = null;
        private DispatcherTimer _timer;
        private WaveBuffer _waveBuffer;
        private byte[] _currentWaveData = null;


        public MainWindow()
        {
            InitializeComponent();
            _player = new WASAPIPlayer();
            _player.PlaybackStopped += _player_PlaybackStopped;

            _timer = new DispatcherTimer(DispatcherPriority.Normal);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            _timer.Tick += _ticker_Tick;


            
        }

        private void _ticker_Tick(object sender, EventArgs e)
        {
            if (_player.IsPlaying)
            {
                if (BufferingResults.Data != null)
                {
                    int c = BufferingResults.Count;
                    byte[] data = BufferingResults.Data;
                    WaveFormat wf = BufferingResults.Format;


                    

                   
                    


                    lblBytes.Content = "" + c;
                }
            }
            else
            {
                lblBytes.Content = "";
            }
        }

        private void _player_PlaybackStopped(object sender, EventArgs e)
        {
            btnPlay.IsEnabled = true;
            _timer.Stop();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            bool result = _player.OpenWaveFile(@".\Samples\sinewave_8_mono.wav");
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

        
    }
}
