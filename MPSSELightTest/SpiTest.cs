using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPSSELight;
using System.Linq;
using MPSSELight.Mpsse;
using MPSSELight.Devices;
using MPSSELight.Protocol;

namespace MPSSELightTest
{
    [TestClass]
    public class SpiTest
    {
        [TestMethod]
        public void OpenCloseTest()
        {
            using (MpsseDevice mpsse = new FT2232D("A"))
            {
                SpiDevice spi = new SpiDevice(mpsse);
            }
        }

        [TestMethod]
        public void LoopbackTest()
        {
            using (MpsseDevice mpsse = new FT2232D("A"))
            {
                SpiDevice spi = new SpiDevice(mpsse);
                mpsse.Loopback = true;

                byte[] tData = { 0x0A, 0x01, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0xFF };
                byte[] rData = spi.readWrite(tData);

                Assert.IsTrue(tData.SequenceEqual(rData));
            }
        }

        [TestMethod]
        public void TransmitTest()
        {
            using (MpsseDevice mpsse = new FT2232D("A"))
            {
                SpiDevice spi = new SpiDevice(mpsse);

                byte[] tData = { 0x0D, 0x01, 0x0F };
                spi.write(tData);
            }
        }

        [TestMethod]
        public void LoopbackBigTest()
        {
            Random r = new Random();

            const uint size = 60000;
            using (MpsseDevice mpsse = new FT2232D("A"))
            {
                SpiDevice spi = new SpiDevice(mpsse);
                mpsse.Loopback = true;

                byte[] tData = new byte[size];
                r.NextBytes(tData);

                byte[] rData = spi.readWrite(tData);

                Assert.IsTrue(tData.SequenceEqual(rData));
            }
        }
    }
}
