using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace WASAPINETCore.Audio
{
    static class AudioConverter
    {
        private static byte[] tmpArray = new byte[2];

        public static double[] ConvertToMonoDoubleFFTArray(byte[] data, WaveFormat sourceFormat)
        {
            double[] dData = null;
            if (sourceFormat.BitsPerSample == 8)
            {
                if (sourceFormat.Channels == 1)
                {
                    ulong size = upper_power_of_two((ulong)data.Length * 2);
                    dData = new double[size];

                    for (int i = 0, j = 0; i < data.Length; i++, j+=2)
                    {
                        dData[j] = HammingWindow((data[i] - 127) / 128f, i, data.Length);
                        dData[j + 1] = 0.0;
                    }
                }
                else if (sourceFormat.Channels == 2)
                {
                    ulong size = upper_power_of_two((ulong)data.Length);
                    dData = new double[size];

                    for (int i = 0, j = 0, h = 0; i < data.Length; i += 2, j+=2, h++)
                    {
                        dData[j] = HammingWindow(((data[i] - 127) + (data[i + 1] - 127)) / 2f / 128f, h, data.Length / 2);
                        dData[j + 1] = 0.0;
                    }
                }
            }
            else if (sourceFormat.BitsPerSample == 16)
            {
                if (sourceFormat.Channels == 1)
                {
                    ulong size = upper_power_of_two((ulong)data.Length);
                    dData = new double[size];
                    for (int i = 0, j = 0, h = 0; i < data.Length; i += 2, j += 2, h++)
                    {
                        tmpArray[0] = data[i];
                        tmpArray[1] = data[i + 1];
                        dData[j] = HammingWindow(BitConverter.ToInt16(tmpArray, 0) / 32767.0, h, data.Length / 2);
                        dData[j + 1] = 0.0;
                    }
                }
                else if (sourceFormat.Channels == 2)
                {
                    ulong size = upper_power_of_two((ulong)data.Length / 2);
                    dData = new double[size];
                    for (int i = 0, j = 0, h = 0; i < data.Length; i += 4, j += 2, h++)
                    {
                        tmpArray[0] = data[i];
                        tmpArray[1] = data[i+1];
                        double left = BitConverter.ToInt16(tmpArray, 0) / 32767.0;

                        tmpArray[0] = data[i + 2];
                        tmpArray[1] = data[i + 3];
                        double right = BitConverter.ToInt16(tmpArray, 0) / 32767.0;

                        
                        dData[j] = HammingWindow((left + right) / 2f, h, data.Length / 4);
                        dData[j + 1] = 0.0;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid source format for wave file.");
            }
            return dData;
        }

        private static double HammingWindow(double v, int n, int windowsize)
        {
            return Math.Clamp(v * (0.52 - 0.48 * Math.Cos((2 * Math.PI * n) / (windowsize - 1))), -1.0, 1.0);
        }

        private static ulong upper_power_of_two(ulong v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }
    }
}
