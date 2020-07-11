using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NAudio;
using NAudio.FileFormats;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;
using NAudio.Dsp;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;

namespace WASAPINETCore.Audio
{
    class WASAPIPlayer 
    {
        private AudioFileReader _reader = null;
        private WASAPISampleAggregator _aggregator = null;
        private WasapiOut _device = null;
        public event EventHandler PlaybackStopped;
        public WaveFormat WaveFormat = null;

        public event EventHandler<FftEventArgs> FftCalculated;
        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;

        private void OnPlaybackStopped()
        {
            EventHandler handler = PlaybackStopped;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void _device_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            OnPlaybackStopped();
            _aggregator.Reset();
        }

        public void Dispose()
        {
            _reader.Dispose();
            _device.Dispose();
        }

        public bool IsPlaying
        {
            get
            {
                return _device != null && _device.PlaybackState == PlaybackState.Playing;
            }
        }

        public bool IsPaused
        {
            get
            {
                return _device != null && _device.PlaybackState == PlaybackState.Paused;
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
                _device.Pause();
            }
        }

        public FileDetails OpenWaveFile(string file)
        {
            if (_device != null)
            {
                _aggregator.Reset();
                _reader.Dispose();
                _device.Dispose();
                WaveFormat = null;
            }

            try
            {
                _reader = new AudioFileReader(file);
                
                WaveFormat = _reader.WaveFormat;
                _aggregator = new WASAPISampleAggregator(_reader, 1024);
                _aggregator.NotificationCount = _reader.WaveFormat.SampleRate / 100;
                _aggregator.PerformFFT = true;
                _aggregator.FftCalculated += (s, a) => FftCalculated?.Invoke(this, a);
                _aggregator.MaximumCalculated += (s, a) => MaximumCalculated?.Invoke(this, a);

                _device = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 200);
                _device.PlaybackStopped += _device_PlaybackStopped;
                _device.Init(_aggregator);

                FileDetails fd = ReadTagsFromFile(file);
                fd.StreamOK = true;
                return fd;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private FileDetails ReadTagsFromFile(string file)
        {
            try
            {
                TagLib.File fileData = TagLib.File.Create(file);
                FileDetails fd = new FileDetails(fileData.Tag.FirstPerformer, fileData.Tag.Title, fileData.Tag.Album);
                return fd;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                if(IsPaused)
                    _device.Play();
                else
                {
                    if (_reader != null)
                    {
                        _reader.Position = 0;
                        _device.Play();
                    }
                }
            }
        }
    }
}
