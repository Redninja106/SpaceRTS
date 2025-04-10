using SpaceGame.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Tests.Networking;

[TestClass]
public class RingBufferTests
{
    [TestMethod]
    public void WriteAndRead_SimpleData_Success()
    {
        var rb = new RingBuffer(16);

        byte[] data = { 1, 2, 3, 4, 5 };
        rb.Write(data.AsSpan());

        Span<byte> readBuf = stackalloc byte[5];
        rb.Read(readBuf);
        CollectionAssert.AreEqual(data, readBuf.ToArray());

        Assert.AreEqual(0, rb.Length);
    }

    [TestMethod]
    public void Write_PartialFillAndWrap_Success()
    {
        var rb = new RingBuffer(8);

        rb.Write([1, 2, 3, 4, 5]);

        Span<byte> tmp = stackalloc byte[3];
        rb.Read(tmp);

        rb.Write([6, 7, 8]);

        Span<byte> readBuf = stackalloc byte[5];
        rb.Read(readBuf);
        CollectionAssert.AreEqual((byte[])[ 4, 5, 6, 7, 8 ], readBuf.ToArray());
    }

    [TestMethod]
    public void Write_OverCapacity_Fails()
    {
        var rb = new RingBuffer(4);
        rb.Write([1, 2, 3, 4]);
        Assert.Throws<Exception>(() =>
        {
            rb.Write([5]);
        });
    }

    [TestMethod]
    public void Read_BeforeWrite_Fails()
    {
        var rb = new RingBuffer(4);
        Assert.Throws<Exception>(() => 
        {
            Span<byte> readBuf = stackalloc byte[2];
            rb.Read(readBuf);
        });
    }

}
