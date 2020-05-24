using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NAudio;
using NAudio.FileFormats;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;

namespace WASAPINETCore.Audio
{
    class WASAPIPlayer 
    {
        private WasapiOut _device = null;
        public event EventHandler PlaybackStopped;
        private WASAPIBufferedWaveProvider _waveProvider;
        private long _positionOnPause = 0;

        private void OnPlaybackStopped()
        {
            EventHandler handler = PlaybackStopped;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void _device_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            OnPlaybackStopped();
            _waveProvider.CloseStream();
        }

        public bool IsPlaying
        {
            get
            {
                return _device != null && (_device.PlaybackState == PlaybackState.Playing || _device.PlaybackState == PlaybackState.Paused);
            }
        }

        public void Stop()
        {
            if (IsPlaying)
            {
                _device.Stop();
            }
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                _positionOnPause = _device.GetPosition();
                _device.Stop();
            }
        }

        public bool OpenWaveFile(string file)
        {
            if(_waveProvider != null)
            {
                _waveProvider.Dispose();
            }
            _waveProvider = new WASAPIBufferedWaveProvider(this, file);

            if (_waveProvider != null)
            {
                if (_device != null)
                {
                    _device.Dispose();
                    _device = null;
                }
                _device = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 50);
                _device.PlaybackStopped += _device_PlaybackStopped;

                _device.Init(_waveProvider);
                
                return true;
            }

            return false;
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                _device.Play();
                _positionOnPause = 0;
            }
        }
    }
}
