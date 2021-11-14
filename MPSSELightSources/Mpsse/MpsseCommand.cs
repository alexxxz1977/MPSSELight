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

using MPSSELight.Ftdi;
using System;

namespace MPSSELight.Mpsse
{
    internal static class MpsseCommand
    {
        #region Set clk divisor

        public static byte[] SetClkDivisor(ushort divisor)
        {
            return new[] { (byte)CommandCode.SetClkDivisor, (byte)(divisor & 0xFF), (byte)(divisor >> 8) };
        }

        #endregion Set clk divisor

        #region FT232H only

        public static byte[] SetIoToOnlyDriveOn0andTristateOn1(FtdiPin low, FtdiPin high)
        {
            return transfer(CommandCode.SetIoToOnlyDriveOn0andTristateOn1, low, high);
        }

        #endregion FT232H only

        #region MSB First

        public static byte[] BytesOutOnPlusEdgeWithMsbFirst(byte[] data)
        {
            return transfer(CommandCode.BytesOutOnPlusEdgeWithMsbFirst, data);
        }

        public static byte[] BytesOutOnMinusEdgeWithMsbFirst(byte[] data, int index = 0, int length = 0)
        {
            return transfer(CommandCode.BytesOutOnMinusEdgeWithMsbFirst, data, index, length);
        }

        public static byte[] BytesOutOnMinusEdgeWithMsbFirst(byte data)
        {
            return transfer(CommandCode.BytesOutOnMinusEdgeWithMsbFirst, data);
        }

        public static byte[] BitsOutOnPlusEdgeWithMsbFirst(byte data, byte len)
        {
            return transfer(CommandCode.BitsOutOnPlusEdgeWithMsbFirst, data, len);
        }

        public static byte[] BitsOutOnMinusEdgeWithMsbFirst(byte data, byte len)
        {
            return transfer(CommandCode.BitsOutOnMinusEdgeWithMsbFirst, data, len);
        }

        public static byte[] BytesInOnPlusEdgeWithMsbFirst(uint len)
        {
            return transfer(CommandCode.BytesInOnPlusEdgeWithMsbFirst, len);
        }

        public static byte[] BytesInOnMinusEdgeWithMsbFirst(uint len)
        {
            return transfer(CommandCode.BytesInOnMinusEdgeWithMsbFirst, len);
        }

        public static byte[] BitsInOnPlusEdgeWithMsbFirst(byte len)
        {
            return transfer(CommandCode.BitsInOnPlusEdgeWithMsbFirst, len);
        }

        public static byte[] BitsInOnMinusEdgeWithMsbFirst(byte len)
        {
            return transfer(CommandCode.BitsInOnMinusEdgeWithMsbFirst, len);
        }

        public static byte[] BytesInOnPlusOutOnMinusWithMsbFirst(byte[] data)
        {
            return transfer(CommandCode.BytesInOnPlusOutOnMinusWithMsbFirst, data);
        }

        public static byte[] BytesInOnMinusOutOnPlusWithMsbFirst(byte[] data)
        {
            return transfer(CommandCode.BytesInOnMinusOutOnPlusWithMsbFirst, data);
        }

        public static byte[] BitsInOnPlusOutOnMinusWithMsbFirst(byte data, byte len)
        {
            return transfer(CommandCode.BitsInOnPlusOutOnMinusWithMsbFirst, data, len);
        }

        public static byte[] BitsInOnMinusOutOnPlusWithMsbFirst(byte data, byte len)
        {
            return transfer(CommandCode.BitsInOnMinusOutOnPlusWithMsbFirst, data, len);
        }

        #endregion MSB First

        #region LSB First

        public static byte[] BytesOutOnPlusEdgeWithLsbFirst(byte[] data)
        {
            return transfer(CommandCode.BytesOutOnPlusEdgeWithLsbFirst, data);
        }

        public static byte[] BytesOutOnMinusEdgeWithLsbFirst(byte[] data, int index = 0, int length = 0)
        {
            return transfer(CommandCode.BytesOutOnMinusEdgeWithLsbFirst, data, index, length);
        }

        public static byte[] BitsOutOnPlusEdgeWithLsbFirst(byte data, byte len)
        {
            return transfer(CommandCode.BitsOutOnPlusEdgeWithLsbFirst, data, len);
        }

        public static byte[] BitsOutOnMinusEdgeWithLsbFirst(byte data, byte len)
        {
            return transfer(CommandCode.BitsOutOnMinusEdgeWithLsbFirst, data, len);
        }

        public static byte[] BytesInOnPlusEdgeWithLsbFirst(uint len)
        {
            return transfer(CommandCode.BytesInOnPlusEdgeWithLsbFirst, len);
        }

        public static byte[] BytesInOnMinusEdgeWithLsbFirst(uint len)
        {
            return transfer(CommandCode.BytesInOnMinusEdgeWithLsbFirst, len);
        }

        public static byte[] BitsInOnPlusEdgeWithLsbFirst(byte len)
        {
            return transfer(CommandCode.BitsInOnPlusEdgeWithLsbFirst, len);
        }

        public static byte[] BitsInOnMinusEdgeWithLsbFirst(byte len)
        {
            return transfer(CommandCode.BitsInOnMinusEdgeWithLsbFirst, len);
        }

        public static byte[] BytesInOnPlusOutOnMinusWithLsbFirst(byte[] data)
        {
            return transfer(CommandCode.BytesInOnPlusOutOnMinusWithLsbFirst, data);
        }

        public static byte[] BytesInOnMinusOutOnPlusWithLsbFirst(byte[] data)
        {
            return transfer(CommandCode.BytesInOnMinusOutOnPlusWithLsbFirst, data);
        }

        public static byte[] BitsInOnPlusOutOnMinusWithLsbFirst(byte data, byte len)
        {
            return transfer(CommandCode.BitsInOnPlusOutOnMinusWithLsbFirst, data, len);
        }

        public static byte[] BitsInOnMinusOutOnPlusWithLsbFirst(byte data, byte len)
        {
            return transfer(CommandCode.BitsInOnMinusOutOnPlusWithLsbFirst, data, len);
        }

        #endregion LSB First

        #region TMS Commands

        public static byte[] TmsOutOnPlusEdge(byte data, byte len)
        {
            return transfer(CommandCode.TmsOutOnPlusEdge, data, len);
        }

        public static byte[] TmsOutOnMinusEdge(byte data, byte len)
        {
            return transfer(CommandCode.TmsOutOnMinusEdge, data, len);
        }

        public static byte[] TmsInOutOnPlusEdge(byte data, byte len)
        {
            return transfer(CommandCode.TmsInOutOnPlusEdge, data, len);
        }

        public static byte[] TmsInOnMinusOutOnPlusEdge(byte data, byte len)
        {
            return transfer(CommandCode.TmsInOnMinusOutOnPlusEdge, data, len);
        }

        public static byte[] TmsInOnPlusOutOnMinusEdge(byte data, byte len)
        {
            return transfer(CommandCode.TmsInOnPlusOutOnMinusEdge, data, len);
        }

        public static byte[] TmsInOutOnMinusEdge(byte data, byte len)
        {
            return transfer(CommandCode.TmsInOutOnMinusEdge, data, len);
        }

        #endregion TMS Commands

        #region Set / Read Data Bits High / Low Bytes

        public static byte[] SetDataBitsLowByte(FtdiPin value, FtdiPin direction)
        {
            return transfer(CommandCode.SetDataBitsLowByte, value, direction);
        }

        public static byte[] SetDataBitsHighByte(FtdiPin value, FtdiPin direction)
        {
            return transfer(CommandCode.SetDataBitsHighByte, value, direction);
        }

        public static byte[] ReadDataBitsLowByte()
        {
            return transfer(CommandCode.ReadDataBitsLowByte);
        }

        public static byte[] ReadDataBitsHighByte()
        {
            return transfer(CommandCode.ReadDataBitsHighByte);
        }

        #endregion Set / Read Data Bits High / Low Bytes

        #region Loopback

        public static byte[] ConnectTdiTdoLoopback()
        {
            return transfer(CommandCode.ConnectTdiTdoLoopback);
        }

        public static byte[] DisconnectTdiTdoLoopback()
        {
            return transfer(CommandCode.DisconnectTdiTdoLoopback);
        }

        #endregion Loopback

        #region Instructions for CPU mode

        public static byte[] ReadShortAddress(byte addrLow)
        {
            return new[] { (byte)CommandCode.ReadShortAddress, addrLow };
        }

        public static byte[] ReadExtendedAddress(ushort addr)
        {
            return new[] { (byte)CommandCode.ReadExtendedAddress, (byte)(addr >> 8), (byte)(addr & 0xFF) };
        }

        public static byte[] WriteShortAddress(byte addrLow, byte data)
        {
            return new[] { (byte)CommandCode.WriteShortAddress, addrLow, data };
        }

        public static byte[] WriteExtendedAddress(ushort addr, byte data)
        {
            return new[] { (byte)CommandCode.WriteExtendedAddress, (byte)(addr >> 8), (byte)(addr & 0xFF), data };
        }

        #endregion Instructions for CPU mode

        #region Instructions for use in both MPSSE and MCU Host Emulation Modes

        public static byte[] SendImmediate()
        {
            return transfer(CommandCode.SendImmediate);
        }

        public static byte[] WaitOnIoHigh()
        {
            return transfer(CommandCode.WaitOnIoHigh);
        }

        public static byte[] WaitOnIoLow()
        {
            return transfer(CommandCode.WaitOnIoLow);
        }

        #endregion Instructions for use in both MPSSE and MCU Host Emulation Modes

        #region FT232H, FT2232H & FT4232H only

        public static byte[] DisableClkDivideBy5()
        {
            return transfer(CommandCode.DisableClkDivideBy5);
        }

        public static byte[] EnableClkDivideBy5()
        {
            return transfer(CommandCode.EnableClkDivideBy5);
        }

        public static byte[] Enable3PhaseDataClocking()
        {
            return transfer(CommandCode.Enable3PhaseDataClocking);
        }

        public static byte[] Disable3PhaseDataClocking()
        {
            return transfer(CommandCode.Enable3PhaseDataClocking);
        }

        public static byte[] ClockForNbitswithNoDataTransfer(byte len)
        {
            return transfer(CommandCode.ClockForNbitswithNoDataTransfer, len);
        }

        public static byte[] ClockForNx8bitswithNoDataTransfer(uint len)
        {
            return transfer(CommandCode.ClockForNx8bitswithNoDataTransfer, len);
        }

        public static byte[] ClkContinuouslyAndWaitOnIoHigh()
        {
            return transfer(CommandCode.ClkContinuouslyAndWaitOnIoHigh);
        }

        public static byte[] ClkContinuouslyAndWaitOnIoLow()
        {
            return transfer(CommandCode.ClkContinuouslyAndWaitOnIoLow);
        }

        public static byte[] TurnOnAdaptiveClocking()
        {
            return transfer(CommandCode.TurnOnAdaptiveClocking);
        }

        public static byte[] TurnOffAdaptiveClocking()
        {
            return transfer(CommandCode.TurnOffAdaptiveClocking);
        }

        public static byte[] ClockForNx8BitsWithNoDataTransferOrUntilGPIOL1isHigh(uint len)
        {
            return transfer(CommandCode.ClockForNx8BitsWithNoDataTransferOrUntilGPIOL1isHigh, len);
        }

        public static byte[] ClockForNx8BitsWithNoDataTransferOrUntilGPIOL1isLow(uint len)
        {
            return transfer(CommandCode.ClockForNx8BitsWithNoDataTransferOrUntilGPIOL1isLow, len);
        }

        #endregion FT232H, FT2232H & FT4232H only

        #region common private functions

        private static byte[] transfer(CommandCode cmd)
        {
            return new[] { (byte)cmd };
        }

        private static byte[] transfer(CommandCode cmd, FtdiPin value, FtdiPin direction)
        {
            return new[] { (byte)cmd, (byte)value, (byte)direction };
        }

        private static byte[] transfer(CommandCode cmd, byte data, byte len)
        {
            if (len == 0 || len > 8)
                throw new ArgumentException("Bit data len should be from 1 to 8");

            return new[] { (byte)cmd, (byte)(len - 1), data };
        }

        private static byte[] transfer(CommandCode cmd, byte len)
        {
            if (len == 0 || len > 8)
                throw new ArgumentException("Bit data len should be from 1 to 8");

            return new[] { (byte)cmd, (byte)(len - 1) };
        }

        private static byte[] transfer(CommandCode cmd, byte[] data, int index = 0, int length = 0)
        {
            var len = length == 0 ? (uint)data.Length : (uint)length;

            if (len == 0 || len > 0x10000)
                throw new ArgumentException("Data len should be from 1 to 65536");

            var result = new byte[len + 3];

            len--;
            result[0] = (byte)cmd;
            result[1] = (byte)(len & 0xFF);
            result[2] = (byte)(len >> 8);
            len++;
            Array.Copy(data, index, result, 3, len);
            return result;
        }

        private static byte[] transfer(CommandCode cmd, uint len)
        {
            if (len == 0 || len > 0x10000)
                throw new ArgumentException("Data len should be from 1 to 65536");

            len--;
            return new[] { (byte)cmd, (byte)(len & 0xFF), (byte)(len >> 8) };
        }
        #endregion common private functions
    }
}