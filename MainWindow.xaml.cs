using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WASAPINETCore.Audio;

namespace WASAPINETCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WASAPIPlayer _player = null;

        public MainWindow()
        {
            InitializeComponent();
            _player = new WASAPIPlayer();
            _player.PlaybackStopped += _player_PlaybackStopped;
            _player.Buffering += _player_Buffering;
        }

        private void _player_PlaybackStopped(object sender, EventArgs e)
        {
            btnPlay.IsEnabled = true;
        }

        private void _player_Buffering(object sender, BufferingEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    lblBytes.Content = e.Count;
                });
            }
            catch (Exception)
            {

            }
            
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            bool result = _player.OpenWaveFile(@".\Samples\gameover.wav");
            if (result)
            {
                btnPlay.IsEnabled = false;
                _player.Play();
            }
            else
            {
                MessageBox.Show("Cannot open file.", "Error during playback", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_player.IsPlaying)
            {
                _player.Stop();
                
            }
        }
    }
}
