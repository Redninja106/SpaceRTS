namespace SpaceGame.Networking;

class RingBuffer : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length
    {
        get
        {
            if (Full)
            {
                return Capacity;
            }
            else if (Front <= Back)
            {
                return Back - Front;
            }
            else
            {
                return (data.Length - Front) + Back;
            }
        }
    }

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    private byte[] data;

    public int Front { get; set; }
    public int Back { get; set; }

    public bool Full { get; private set; }

    public int Capacity => data.Length;

    public RingBuffer(int capacity)
    {
        data = new byte[capacity];
    }

    public int PeakInt32()
    {
        return BitConverter.ToInt32([
            data[(Front + 0) % data.Length],
            data[(Front + 1) % data.Length],
            data[(Front + 2) % data.Length],
            data[(Front + 3) % data.Length],
        ]);
    }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Read(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        if (buffer.Length > Length)
        {
            throw new Exception("read too much data from ring buffer!");
        }

        int firstPart = Math.Min(buffer.Length, Capacity - Front);
        data.AsSpan(Front, firstPart).CopyTo(buffer);
        data.AsSpan(0, buffer.Length - firstPart).CopyTo(buffer.Slice(firstPart));
        Front = (Front + buffer.Length) % Capacity;
        Full = false;

        return buffer.Length;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        Write(buffer.AsSpan(offset, count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length > Capacity - Length)
        {
            throw new Exception("ring buffer capacity exceeded");
        }

        int firstPart = Math.Min(buffer.Length, Capacity - Back);
        buffer[..firstPart].CopyTo(data.AsSpan(Back));
        buffer[firstPart..].CopyTo(data.AsSpan(0));
        Back = (Back + buffer.Length) % Capacity;
        Full = Front == Back;
    }
}