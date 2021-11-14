using System;
using System.IO;
using System.Threading;
using MPSSELight.Ftdi;
using MPSSELight.BitMainipulation;
using MPSSELight.Mpsse;

namespace MPSSELight.Protocol
{
    public class SpiDeviceDc
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

        public enum SpiBitOrder
        {
            MsbFirst,
            LsbFirst
        }

        protected SpiDeviceParams _param;
        protected MpsseDevice _mpsse;

        private delegate byte[] WriteCommandDelegate(byte[] data, int index = 0, int length = 0);
        private readonly WriteCommandDelegate _WriteCommand;

        public SpiDeviceDc(MpsseDevice mpsse) : this(mpsse, new SpiDeviceParams())
        {
        }

        public SpiDeviceDc(MpsseDevice mpsse, SpiDeviceParams param)
        {
            _mpsse = mpsse;
            _param = param;

            if (param.BitOrder == SpiBitOrder.MsbFirst)
            {
                _WriteCommand = MpsseCommand.BytesOutOnMinusEdgeWithMsbFirst;
            }
            else
            {
                _WriteCommand = MpsseCommand.BytesOutOnMinusEdgeWithLsbFirst;
            }

            var csPin = param.ChipSelectPolicy == CsPolicy.CsActiveLow ? _param.ChipSelect : FtdiPin.None;
            _mpsse.write(MpsseCommand.SetDataBitsLowByte(csPin, lowByteDirection));
        }

        private FtdiPin lowByteDirection => _param.ChipSelect | FtdiPin.DO | FtdiPin.SK;

        private Bit cs;
        private FtdiPin csPinValue;
        public Bit CS
        {
            get => cs;
            set
            {
                cs = value;
                csPinValue = cs == Bit.One ? _param.ChipSelect : FtdiPin.None;
            }
        }

        protected byte[] EnableLine()
        {
            if (_param.ChipSelectPolicy == CsPolicy.CsActiveHigh)
                CS = Bit.One;
            if (_param.ChipSelectPolicy == CsPolicy.CsActiveLow)
                CS = Bit.Zero;

            return MpsseCommand.SetDataBitsLowByte(csPinValue, lowByteDirection);
        }

        protected byte[] DisableLine()
        {
            if (_param.ChipSelectPolicy == CsPolicy.CsActiveHigh)
                CS = Bit.Zero;
            if (_param.ChipSelectPolicy == CsPolicy.CsActiveLow)
                CS = Bit.One;

            return MpsseCommand.SetDataBitsLowByte(csPinValue, lowByteDirection);
        }

        protected virtual byte[] SetDataBitsHighByte(bool command)
        {
            return null;
        }

        public void Write(byte[] data, bool command)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Append(EnableLine());
                var buf = SetDataBitsHighByte(command);
                if (buf != null && buf.Length > 0)
                {
                    ms.Append(buf);
                }
                var len = data.Length;
                if (len > 65535)
                {
                    var i = 0;
                    while (i < len)
                    {
                        var chunkSize = len - i >= 65535 ? 65535 : len - i;
                        ms.Append(_WriteCommand(data, i, chunkSize));
                        i += 65535;
                    }
                }
                else 
                {
                    ms.Append(_WriteCommand(data));
                }
                ms.Append(DisableLine());

                var b = ms.ToArray();
                //Console.WriteLine("0x"+BitConverter.ToString(b).Replace("-", ", 0x"));
                _mpsse.write(b);
            }
        }

        public void WriteBatch(byte[] batch)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Append(EnableLine());
                int i = 0;
                while (i < batch.Length)
                {
                    var buf = SetDataBitsHighByte(true);
                    if (buf != null && buf.Length > 0)
                    {
                        ms.Append(buf);
                    }
                    ms.Append(_WriteCommand(batch, i++, 1));
                    byte numArgs = batch[i++];
                    numArgs &= 0x7F;
                    if (numArgs == 0)
                    {
                        continue;
                    }
                    buf = SetDataBitsHighByte(false);
                    if (buf != null && buf.Length > 0)
                    {
                        ms.Append(buf);
                    }
                    ms.Append(_WriteCommand(batch, i, numArgs));
                    i += numArgs;
                }
                ms.Append(DisableLine());

                _mpsse.write(ms.ToArray());
            }
        }

        public class SpiDeviceParams
        {
            public FtdiPin ChipSelect = FtdiPin.CS;
            public CsPolicy ChipSelectPolicy = CsPolicy.CsActiveLow;
            public SpiMode Mode = SpiMode.Mode0;
            public SpiBitOrder BitOrder = SpiBitOrder.MsbFirst;
        }
    }
}