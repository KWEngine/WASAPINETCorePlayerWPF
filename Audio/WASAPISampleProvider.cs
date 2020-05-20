using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WASAPINETCore.Audio
{
    class WASAPISampleProvider : NAudio.Wave.ISampleProvider
    {
        private WaveFormat _waveFormat;
        private AudioFileReader _reader;

        public WaveFormat WaveFormat
        {
            get
            {
                return _waveFormat;
            }
        }

        public WASAPISampleProvider(string file)
        {
            try
            {
                _reader = new AudioFileReader(file);
            }
            catch(Exception ex)
            {
                throw new IOException("Cannot open file '" + file + "'.", ex);
            }

            _waveFormat = _reader.WaveFormat;
            
           
        }

        public int Read(float[] buffer, int offset, int count)
        {
            return _reader.Read(buffer, offset, count);
        }
    }
}
