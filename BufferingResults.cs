﻿using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace WASAPINETCore
{
    static class BufferingResults
    {
        public static volatile double[] Data;
        public static volatile int Count;
        public static volatile WaveFormat Format;
        public static volatile uint Timestamp;
    }
}
