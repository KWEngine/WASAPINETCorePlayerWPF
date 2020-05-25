using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace WASAPINETCore.Audio
{
    class WASAPIBufferedWaveProvider : IWaveProvider, IDisposable
    {
        private WaveFormat _waveFormat = null;
        private WaveStream _waveStream = null;

        public WASAPIBufferedWaveProvider(WaveStream stream)
        {
            _waveStream = stream;
            if (_waveStream != null)
            {
                _waveFormat = _waveStream.WaveFormat;
                if (_waveFormat.BitsPerSample != 8 && _waveFormat.BitsPerSample != 16)
                {
                    throw new Exception("Invalid bit depth. File must be @ 8 or 16 bits per sample.");
                }
            }
            else
            {
                _waveFormat = null;
            }
        }

        public void ResetStream()
        {
            try
            {
                _waveStream.Position = 0;
            }
            catch (Exception)
            {

            }
            
        }

        public void CloseStream()
        {
            if(_waveStream != null)
            {
                _waveStream.Close();
            }
        }

        public WaveFormat WaveFormat
        {
            get
            {
                return _waveFormat;
            }
        }

        public void Dispose()
        {
            CloseStream();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (_waveStream != null)
            {
                int read = _waveStream.Read(buffer,offset,count);
                uint ms = (uint)Global.Watch.ElapsedMilliseconds;
                if (read > 0)
                {
                    BufferingResults.Count = read;
                    BufferingResults.Data = buffer;
                    BufferingResults.Format = _waveStream.WaveFormat;
                    BufferingResults.Timestamp = ms;
                }
                return read;
            }
            return 0;
        }
    }
}
