using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MPSSELight.Ftdi;
using MPSSELight.Mpsse;

namespace MPSSELight.Protocol
{
    public class I2cBus
    {
        private readonly MpsseDevice _mpsse;

        private static byte START_DURATION_1 = 10;
        private static byte START_DURATION_2 = 20;

        private static byte STOP_DURATION_1 = 10;
        private static byte STOP_DURATION_2 = 10;
        private static byte STOP_DURATION_3 = 10;

        public I2cBus(MpsseDevice mpsse)
        {
            _mpsse = mpsse;

            _mpsse.Enqueue(MpsseCommand.EnableClkDivideBy5());
            _mpsse.Enqueue(MpsseCommand.TurnOffAdaptiveClocking());
            _mpsse.Enqueue(MpsseCommand.Enable3PhaseDataClocking());
            _mpsse.Enqueue(MpsseCommand.SetClkDivisor(mpsse.ClkDivisor));
            _mpsse.Enqueue(MpsseCommand.DisconnectTdiTdoLoopback());

            /*_mpsse.AdBusDirection.SetBit(0).SetBit(1);
            _mpsse.AdBusValue.SetBit(0).SetBit(1);
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));*/

            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x13, (FtdiPin)0x13));

            _mpsse.ExecuteBuffer();
        }

        public byte[] Scan()
        {
            IList<byte> result = new List<byte>();

            _mpsse.ClearInput();
            _mpsse.ClearOutput();

            for (byte addr = 1; addr < 127; addr++)
            {
                EnqueueStartCommands();
                EnqueueSendByteCommands((byte)(addr << 1), true);
                EnqueueStopCommands();
                _mpsse.ExecuteBuffer();
                var ack = _mpsse.read(1);
                if ((ack[0] & 0x01) == 0)
                {
                    result.Add(addr);
                }
            }
            return result.ToArray();
        }

        public bool SendByteAndCheckACK(byte DataByteToSend, bool immediate = false, byte len = 1)
        {
            EnqueueSendByteCommands(DataByteToSend, immediate);

            // Execute
            _mpsse.ExecuteBuffer();

            // Result
            var ack = _mpsse.read(len);
            return (ack[0] & 0x01) == 0;
        }

        public bool SendDeviceAddrAndCheckACK(byte address, bool read = false, bool immediate = false, byte len = 1)
        {
            // Address
            address <<= 1;
            if (read)
            {
                address |= 0x01;
            }
            return SendByteAndCheckACK(address, immediate, len);
        }

        public byte ReceiveByte(bool nAck = true, bool immediate = false)
        {
            EnqueueGetByteCommands(nAck, immediate);

            // Execute
            _mpsse.ExecuteBuffer();

            // Result
            var result = _mpsse.read(1);
            return result[0];
        }

        public void FastWriteData(byte address, byte[] data, bool checkAck = true)
        {
            _mpsse.ClearInput();

            EnqueueStartCommands();
            EnqueueSendByteCommands((byte)(address << 1));
            for (var i = 0; i < data.Length; i++)
            {
                EnqueueSendByteCommands(data[i]);
            }
            EnqueueStopCommands();

            _mpsse.ExecuteBuffer();

            if (checkAck)
            {
                // Result
                var ack = _mpsse.read((uint)data.Length + 1);
                if (!ack.All(x => (x & 0x01) == 0))
                {
                    throw new Exception("Slave device doesn't acknowledge all written bytes during fast write operation");
                }
            }
        }

        public void WriteData(byte address, byte[] data)
        {
            _mpsse.ClearInput();

            Start();
            if (!SendDeviceAddrAndCheckACK(address))
            {
                throw new Exception("");
            }
            for (var i = 0; i < data.Length; i++)
            {
                if (!SendByteAndCheckACK(data[i], true))
                {
                    throw new Exception("");
                }
            }
            Stop();
        }

        public void ClearInput()
        {
            _mpsse.ClearInput();
        }

        public byte[] ReadData(byte address, byte register, uint dataSize)
        {
            _mpsse.ClearInput();

            Start();
            if (!SendDeviceAddrAndCheckACK(address) || !SendByteAndCheckACK(register))
            {
                Stop();
                throw new Exception("Unable to write device header while reading");
            }
            Start();
            if (!SendDeviceAddrAndCheckACK(address, true))
            {
                Stop();
                throw new Exception("Unable to write address whle reading");
            }
            var data = new byte[dataSize];
            for (int i = 0; i < dataSize; i++)
            {
                bool nAck = i == (dataSize - 1);
                data[i] = ReceiveByte(nAck);
            }
            Stop();
            return data;
        }


        public byte[] FastReadData(byte address, byte register, uint dataSize)
        {
            _mpsse.ClearInput();
            EnqueueStartCommands();
            address <<= 1;
            EnqueueSendByteCommands(address);
            EnqueueSendByteCommands(register);
            EnqueueStartCommands();
            EnqueueSendByteCommands((byte)(address | 0x01));
            for (int i = 0; i < dataSize; i++)
            {
                bool nAck = /*dataSize > 1 &&*/ i == (dataSize - 1);
                EnqueueGetByteCommands(nAck);
            }
            EnqueueStopCommands();
            _mpsse.ExecuteBuffer();

            var result = _mpsse.read(dataSize + 3);
            if ((result[0] & 0x01) != 0)
            {
                throw new Exception("Slave device doesn't acknowledge all service bytes during fast read operation");
            }
            var arr = new byte[dataSize];
            Array.Copy(result, 3, arr, 0, dataSize);
            return arr;
        }

        private void EnqueueGetByteCommands(bool nAck = false, bool sendImmediate = false)
        {
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x00, (FtdiPin)0x11));
            _mpsse.Enqueue(MpsseCommand.BitsInOnPlusEdgeWithMsbFirst(8));
            FtdiPin pin = nAck ? (FtdiPin)0x11 : (FtdiPin)0x13;
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x00, pin));
            byte val = nAck ? (byte)0x80 : (byte)0x00;
            _mpsse.Enqueue(MpsseCommand.BitsOutOnMinusEdgeWithMsbFirst(val, 1));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x00, (FtdiPin)0x11));

            if (sendImmediate)
            {
                _mpsse.Enqueue(MpsseCommand.SendImmediate());
            }
        }


        private void EnqueueSendByteCommands(byte value, bool sendImmediate = false)
        {
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x00, (FtdiPin)0x13));
            _mpsse.Enqueue(MpsseCommand.BitsOutOnMinusEdgeWithMsbFirst(value, 8));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x00, (FtdiPin)0x11));

            // Put line back to idle (data released, clock pulled low)
            /*_mpsse.AdBusValue.SetBit(1).UnsetBit(0);
            _mpsse.AdBusDirection.SetBit(1).SetBit(1);
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));*/

            // CLOCK IN ACK
            _mpsse.Enqueue(MpsseCommand.BitsInOnPlusEdgeWithMsbFirst(1));

            // Send off the commands
            if (sendImmediate)
            {
                _mpsse.Enqueue(MpsseCommand.SendImmediate());
            }
        }

        private void EnqueueStartCommands()
        {
            /*_mpsse.AdBusDirection.SetBit(0).SetBit(1);
            _mpsse.AdBusValue.SetBit(0).SetBit(1);
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));

            _mpsse.AdBusValue.UnsetBit(1);
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));

            _mpsse.AdBusValue.UnsetBit(0);
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));

            _mpsse.AdBusValue.SetBit(1).UnsetBit(0);
            _mpsse.AdBusDirection.SetBit(1).SetBit(1);
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));*/

            for (var i = 0; i < START_DURATION_1; i++)
            {
                _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x03, (FtdiPin)0x11));
            }

            for (var i = 0; i < START_DURATION_2; i++)
            {
                _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x01, (FtdiPin)0x13));
            }

            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x00, (FtdiPin)0x13));

        }

        public void Start()
        {
            EnqueueStartCommands();
            _mpsse.ExecuteBuffer();
        }

        private void EnqueueStopCommands()
        {
            /*_mpsse.AdBusDirection.SetBit(1).SetBit(0);
            _mpsse.AdBusValue.UnsetBit(1).UnsetBit(0);
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));

            _mpsse.AdBusValue.SetBit(0);
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));

            _mpsse.AdBusValue.SetBit(1);
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte(_mpsse.AdBusValue, _mpsse.AdBusDirection));*/

            for (var i = 0; i < STOP_DURATION_1; i++)
            {
                _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x00, (FtdiPin)0x13));
            }

            for (var i = 0; i < STOP_DURATION_2; i++)
            {
                _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x01, (FtdiPin)0x13));
            }

            for (var i = 0; i < STOP_DURATION_3; i++)
            {
                _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x03, (FtdiPin)0x11));
            }
            _mpsse.Enqueue(MpsseCommand.SetDataBitsLowByte((FtdiPin)0x03, (FtdiPin)0x10));
        }

        public void Stop()
        {
            EnqueueStopCommands();
            // Execute
            _mpsse.ExecuteBuffer();
        }
    }
}