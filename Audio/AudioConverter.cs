using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace WASAPINETCore.Audio
{
    static class AudioConverter
    {
        public static float[] ConvertToMonoFloatArray(byte[] data, WaveFormat sourceFormat)
        {
            float[] fData = null;
            if (sourceFormat.BitsPerSample == 8)
            {
                if(sourceFormat.Channels == 1)
                {
                    fData = new float[data.Length];
                    for (int i = 0; i < data.Length; i++)
                    {
                        fData[i] = (data[i] - 128) / 127f;
                    }
                }
                else if(sourceFormat.Channels == 2)
                {
                    fData = new float[data.Length / 2];
                    for (int i = 0, j = 0; i < data.Length; i+=2, j++)
                    {
                        fData[j] = ((data[i] - 128) + (data[i+1] - 128)) / 2f / 127f;
                    }
                }
            }
            else if(sourceFormat.BitsPerSample == 16)
            {
                if (sourceFormat.Channels == 1)
                {
                    fData = new float[data.Length / 2];
                    for (int i = 0; i < data.Length; i+=2)
                    {
                        fData[i] = ((data[i + 1] << 8) | data[i]) / (float)short.MaxValue;
                    }
                }
                else if (sourceFormat.Channels == 2)
                {
                    fData = new float[data.Length / 4];
                    for (int i = 0, j = 0; i < data.Length; i += 4, j++)
                    {
                        float left = ((data[i + 1] << 8) | data[i]) / (float)short.MaxValue;
                        float right = ((data[i + 3] << 8) | data[i + 2]) / (float)short.MaxValue;
                        fData[j] = (left + right) / 2f;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid source format for wave file.");
            }
            return fData;
        }

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
                        dData[j] = (data[i] - 128) / 128f;
                        dData[j + 1] = 0.0;
                    }
                }
                else if (sourceFormat.Channels == 2)
                {
                    ulong size = upper_power_of_two((ulong)data.Length);
                    dData = new double[size];

                    for (int i = 0, j = 0; i < data.Length; i += 2, j+=2)
                    {
                        dData[j] = ((data[i] - 128) + (data[i + 1] - 128)) / 2f / 128f;
                        dData[j + 2] = 0.0;
                    }
                }
            }
            else if (sourceFormat.BitsPerSample == 16)
            {
                if (sourceFormat.Channels == 1)
                {
                    ulong size = upper_power_of_two((ulong)data.Length);
                    dData = new double[size];
                    for (int i = 0, j = 0; i < data.Length; i += 2, j += 2)
                    {
                        dData[j] = ((data[i + 1] << 8) | data[i]) / (float)short.MaxValue;
                        dData[j + 1] = 0.0;
                    }
                }
                else if (sourceFormat.Channels == 2)
                {
                    ulong size = upper_power_of_two((ulong)data.Length / 2);
                    dData = new double[size];
                    for (int i = 0, j = 0; i < data.Length; i += 4, j += 2)
                    {
                        float left = ((data[i + 1] << 8) | data[i]) / (float)short.MaxValue;
                        float right = ((data[i + 3] << 8) | data[i + 2]) / (float)short.MaxValue;
                        dData[j] = (left + right) / 2f;
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
