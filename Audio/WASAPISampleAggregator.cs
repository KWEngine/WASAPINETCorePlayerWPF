using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NAudio.Dsp;
using NAudio.Wave;

namespace WASAPINETCore.Audio
{
    public class WASAPISampleAggregator : ISampleProvider
    {
        // volume
        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;
        private float maxValue;
        private float minValue;
        public int NotificationCount { get; set; }
        int count;

        // FFT
        private LomontFFT _fft = new LomontFFT();
        public event EventHandler<FftEventArgs> FftCalculated;
        private double[] audioData;
        private double[] audioDataFinal;
        public bool PerformFFT { get; set; }
        //private readonly Complex[] fftBuffer;
        private readonly FftEventArgs fftArgs;
        private int fftPos;
        private readonly int fftLength;
        private readonly int m;
        private readonly ISampleProvider source;

        private readonly int channels;

        public WASAPISampleAggregator(ISampleProvider source, int fftLength = 1024)
        {
            channels = source.WaveFormat.Channels;
            if (!IsPowerOfTwo(fftLength))
            {
                throw new ArgumentException("FFT Length must be a power of two");
            }
            m = (int)Math.Log(fftLength, 2.0);
            this.fftLength = fftLength;
            audioData = new double[fftLength * 2];
            audioDataFinal = new double[fftLength];
            fftArgs = new FftEventArgs(audioDataFinal);
            this.source = source;
        }

        static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }


        public void Reset()
        {
            count = 0;
            maxValue = minValue = 0;
        }

        private void Add(float value)
        {
            //Console.WriteLine(value);
            if (PerformFFT && FftCalculated != null)
            {
                
                audioData[fftPos] = (float)(value * FastFourierTransform.BlackmannHarrisWindow(fftPos / 2, fftLength));
                audioData[fftPos + 1] = 0;
                fftPos+=2;
                if (fftPos >= audioData.Length)
                {
                    fftPos = 0;
                    // 1024 = 2^10
                    _fft.FFT(audioData, true);
                    for(int i = 0; i < audioData.Length / 2; i++)
                    {
                        audioDataFinal[i] = audioData[i];
                    }
                    //FastFourierTransform.FFT(true, m, fftBuffer);
                    FftCalculated(this, fftArgs);
                }
            }

            maxValue = Math.Max(maxValue, value);
            minValue = Math.Min(minValue, value);
            count++;
            if (count >= NotificationCount && NotificationCount > 0)
            {
                MaximumCalculated?.Invoke(this, new MaxSampleEventArgs(minValue, maxValue));
                Reset();
            }
        }

        public WaveFormat WaveFormat => source.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            
            var samplesRead = source.Read(buffer, offset, count);

            for (int n = 0; n < samplesRead; n += channels)
            {
                Add(buffer[n + offset]);
            }
            return samplesRead;
        }
    }

    public class MaxSampleEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public MaxSampleEventArgs(float minValue, float maxValue)
        {
            MaxSample = maxValue;
            MinSample = minValue;
        }
        public float MaxSample { get; private set; }
        public float MinSample { get; private set; }
    }

    public class FftEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public FftEventArgs(double[] result)
        {
            Result = result;
        }
        public double[] Result { get; private set; }
    }
}
