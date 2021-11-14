/* The MIT License (MIT)

Copyright(c) 2016 Stanislav Zhelnio

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using MPSSELight.BitMainipulation;
using MPSSELight.Ftdi;
using MPSSELight.Mpsse;
using System.Diagnostics;

namespace MPSSELight.Protocol
{
    public class SpiDevice
    {
        public enum CsPolicy
        {
            //managed wout user actions
            CsActiveLow,

            CsActiveHigh,

            //manualy managed default value
            CsDefaultLow,

            CsDefaultHigh
        }

        public enum SpiMode
        {
            Mode0, //CPOL=0, CPHA=0
            Mode2 //CPOL=1, CPHA=0
        }

        private Bit cs;
        protected readonly MpsseDevice mpsse;
        protected readonly SpiParams param;

        private readonly ReadWriteCommandDelegate readWriteCommand;

        protected readonly WriteCommandDelegate writeCommand;

        public SpiDevice(MpsseDevice mpsse) : this(mpsse, new SpiParams())
        {
        }

        public SpiDevice(MpsseDevice mpsse, SpiParams param)
        {
            this.mpsse = mpsse;
            this.param = param;

            switch (param.Mode)
            {
                default:
                case SpiMode.Mode0:
                    writeCommand = mpsse.BytesOutOnMinusEdgeWithMsbFirst;
                    readWriteCommand = mpsse.BytesInOnPlusOutOnMinusWithMsbFirst;
                    break;

                case SpiMode.Mode2:
                    writeCommand = mpsse.BytesOutOnPlusEdgeWithMsbFirst;
                    readWriteCommand = mpsse.BytesInOnMinusOutOnPlusWithMsbFirst;
                    break;
            }

            //pin init values
            switch (param.ChipSelectPolicy)
            {
                default:
                case CsPolicy.CsActiveLow:
                case CsPolicy.CsDefaultHigh:
                    CS = Bit.One;
                    break;

                case CsPolicy.CsActiveHigh:
                case CsPolicy.CsDefaultLow:
                    CS = Bit.Zero;
                    break;
            }

            Debug.WriteLine("SPI initial successful : " + mpsse.ClockFrequency);
        }

        public Bit CS
        {
            get => cs;
            set
            {
                cs = value;
                var pinValue = cs == Bit.One ? param.ChipSelect : FtdiPin.None;
                mpsse.SetDataBitsLowByte(pinValue, param.ChipSelect | FtdiPin.DO | FtdiPin.SK);
            }
        }

        public bool LoopbackEnabled
        {
            get => mpsse.Loopback;
            set => mpsse.Loopback = value;
        }

        protected void EnableLine()
        {
            if (param.ChipSelectPolicy == CsPolicy.CsActiveHigh)
                CS = Bit.One;
            if (param.ChipSelectPolicy == CsPolicy.CsActiveLow)
                CS = Bit.Zero;
        }

        protected void DisableLine()
        {
            if (param.ChipSelectPolicy == CsPolicy.CsActiveHigh)
                CS = Bit.Zero;
            if (param.ChipSelectPolicy == CsPolicy.CsActiveLow)
                CS = Bit.One;
        }

        public virtual void write(byte[] data)
        {
            EnableLine();
            writeCommand(data);
            DisableLine();
        }

        public byte[] readWrite(byte[] data)
        {
            EnableLine();
            var result = readWriteCommand(data);
            DisableLine();
            return result;
        }

        public class SpiParams
        {
            public FtdiPin ChipSelect = FtdiPin.CS;
            public CsPolicy ChipSelectPolicy = CsPolicy.CsActiveLow;
            public SpiMode Mode = SpiMode.Mode0;
        }

        protected delegate void WriteCommandDelegate(byte[] data);
        protected delegate byte[] ReadWriteCommandDelegate(byte[] data);
    }
}