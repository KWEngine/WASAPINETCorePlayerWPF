using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WASAPINETCore.Audio
{
    class WASAPIBufferedWaveProvider : IWaveProvider, IDisposable
    {
        private WaveFormat _waveFormat = null;
        private WaveFileReader _reader = null;
        private FileStream _stream = null;
        private WASAPIPlayer _player = null;

        public WASAPIBufferedWaveProvider(WASAPIPlayer player, string file)
        {
            _player = player;
            _stream = new FileStream(file, FileMode.Open);
            _reader = new WaveFileReader(_stream);
            if (_reader != null)
            {
                _waveFormat = _reader.WaveFormat;
            }
            else
            {
                _waveFormat = null;
            }
        }

        public void CloseStream()
        {
            if(_stream != null)
            {
                _stream.Close();
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
            if (_reader != null)
            {
                _reader.Close();
                CloseStream();
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (_reader != null) {
                int read = _reader.Read(buffer,offset,count);
                _player.OnBuffering(buffer, read, _reader.WaveFormat);
                return read;
            }
            return 0;
        }
    }
}
