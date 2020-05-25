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

        private void OnPlaybackStopped()
        {
            EventHandler handler = PlaybackStopped;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void _device_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            OnPlaybackStopped();
            _waveProvider.ResetStream();
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
                _device.Dispose();
                _device = null;
            }

            if (_waveProvider != null)
            {
                _waveProvider.Dispose();
            }
            try
            {
                WaveStream wStream = null;
                if (file.ToLower().EndsWith("mp3"))
                {
                    wStream = new Mp3FileReader(file);
                }
                else if (file.ToLower().EndsWith("wav"))
                {
                    wStream = new WaveFileReader(file);
                }

                
                _waveProvider = new WASAPIBufferedWaveProvider(wStream);

                if (_waveProvider != null)
                {


                    _device = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 50);
                    _device.PlaybackStopped += _device_PlaybackStopped;
                    _device.Init(_waveProvider);

                    FileDetails fd = ReadTagsFromFile(file);
                    fd.StreamOK = true;
                    return fd;
                }
            }
            catch(Exception)
            {

            }

            return null;
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
                _device.Play();
            }
        }
    }
}
