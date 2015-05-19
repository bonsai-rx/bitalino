using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Bitalino
{
    public class BitalinoDataFrame
    {
        public BitalinoDataFrame(global::Bitalino.Frame[] frames)
        {
            if (frames == null)
            {
                throw new ArgumentNullException("frames");
            }

            if (frames.Length == 0)
            {
                throw new ArgumentException("The array of BITalino frames cannot be empty.");
            }

            Analog = GetAnalogData(frames);
            Digital = GetDigitalData(frames);
        }

        static Mat GetAnalogData(global::Bitalino.Frame[] frames)
        {
            var data = new short[frames.Length * frames[0].analog.Length];
            for (int j = 0; j < frames.Length; j++)
            {
                for (int i = 0; i < frames[j].analog.Length; i++)
                {
                    data[i * frames.Length + j] = frames[j].analog[i];
                }
            }

            return Mat.FromArray(data, frames[0].analog.Length, frames.Length, Depth.S16, 1);
        }

        static Mat GetDigitalData(global::Bitalino.Frame[] frames)
        {
            var data = new byte[frames.Length * frames[0].digital.Length];
            for (int j = 0; j < frames.Length; j++)
            {
                for (int i = 0; i < frames[j].digital.Length; i++)
                {
                    data[i * frames.Length + j] = frames[j].digital[i] ? (byte)1 : (byte)0;
                }
            }

            return Mat.FromArray(data, frames[0].analog.Length, frames.Length, Depth.U8, 1);
        }

        public Mat Analog { get; private set; }

        public Mat Digital { get; private set; }
    }
}
