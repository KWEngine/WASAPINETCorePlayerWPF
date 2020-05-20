using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NAudio;
using NAudio.FileFormats;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;

namespace WASAPINETCore.Audio
{
    class WASAPIPlayer 
    {
        private WasapiOut _device = null;
        public delegate void BufferingEventHandler(object sender, BufferingEventArgs e);
        public event BufferingEventHandler Buffering;
        public event EventHandler PlaybackStopped;
        private WASAPIBufferedWaveProvider _waveProvider;

        private void OnPlaybackStopped()
        {
            EventHandler handler = PlaybackStopped;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void OnBuffering(byte[] bufferData, int count, WaveFormat format)
        {
            BufferingEventHandler handler = Buffering;
            BufferingEventArgs args = new BufferingEventArgs();
            args.Data = bufferData;
            args.Count = count;
            args.Format = format;

            if (handler != null) handler(this, args);
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
                return _device != null && _device.PlaybackState == PlaybackState.Playing;
            }
        }

        public void Stop()
        {
            if (IsPlaying)
            {
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
                _device = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 100);
                _device.PlaybackStopped += _device_PlaybackStopped;

                _device.Init(_waveProvider);
                
                return true;
            }

            return false;
        }

        public void Play()
        {
            _device.Play();
        }
    }
}
