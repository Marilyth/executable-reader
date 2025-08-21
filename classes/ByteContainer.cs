using System;

public class ByteContainer
{
    protected byte[] _data;

    public ByteContainer(byte[] data)
    {
        _data = data;
    }

    protected ReadOnlySpan<byte> Data => _data;
}