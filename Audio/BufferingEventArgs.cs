using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace WASAPINETCore.Audio
{
    class BufferingEventArgs : EventArgs
    {
        public byte[] Data;
        public int Count;
        public WaveFormat Format;
    }
}
