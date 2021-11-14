using MPSSELight.BitMainipulation;
using MPSSELight.Ftdi;
using MPSSELight.Mpsse;
using System.IO;
using System.Threading;

namespace MPSSELight.Protocol
{
    public class SpiDisplayDevice  : SpiDeviceDc
    {
        public SpiDisplayDevice(MpsseDevice mpsse) : this(mpsse, new SpiDisplayParams())
        {
        }

        public SpiDisplayDevice(MpsseDevice mpsse, SpiDeviceParams param) : base (mpsse, param)
        {
            DC = Bit.Zero;
        }

        protected SpiDisplayParams Param
        {
            get { return (SpiDisplayParams)_param; }
        }


        private FtdiPin highByteDirection => Param.DataCommandPin | Param.ResetPin;

        private Bit dc;
        private FtdiPin dcPinValue;
        public Bit DC
        {
            get => dc;
            set
            {
                dc = value;
                dcPinValue = dc == Bit.One ? highByteDirection : Param.ResetPin;
            }
        }

        public void Reset()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                for (var i = 0; i < 5; i++)
                {
                    ms.Append(MpsseCommand.SetDataBitsHighByte(FtdiPin.None, Param.DataCommandPin | Param.ResetPin));
                }
                ms.Append(MpsseCommand.SetDataBitsHighByte(Param.ResetPin, Param.DataCommandPin | Param.ResetPin));
                _mpsse.write(ms.ToArray());
            }
        }

        public void Reset(int timeout)
        {
            _mpsse.write(MpsseCommand.SetDataBitsHighByte(Param.DataCommandPin, highByteDirection));
            Thread.Sleep(timeout);
            _mpsse.write(MpsseCommand.SetDataBitsHighByte(highByteDirection, highByteDirection));
            Thread.Sleep(timeout);
        }

        protected override byte[] SetDataBitsHighByte(bool command)
        {
            DC = command ? Bit.Zero : Bit.One;
            return MpsseCommand.SetDataBitsHighByte(dcPinValue, highByteDirection);
        }

        public class SpiDisplayParams : SpiDeviceParams
        {
            public FtdiPin DataCommandPin = FtdiPin.GPIOH1;
            public FtdiPin ResetPin = FtdiPin.GPIOH3;
        }
    }
}