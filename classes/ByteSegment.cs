using System.Runtime.InteropServices;

public struct AddressPointer
{
    public uint Address { get; set; }
    public AddressType AddressType { get; set; }

    public static AddressPointer operator +(AddressPointer pointer, uint offset)
    {
        return new AddressPointer
        {
            Address = pointer.Address + offset,
            AddressType = pointer.AddressType
        };
    }

    public static AddressPointer operator -(AddressPointer pointer, uint offset)
    {
        return new AddressPointer
        {
            Address = pointer.Address - offset,
            AddressType = pointer.AddressType
        };
    }

    public override string ToString()
    {
        return $"{AddressType} 0x{Address:X}";
    }
}

public struct ByteSegment
{
    public AddressPointer Pointer { get; set; }
    public uint Size { get; set; }

    public bool Contains(AddressPointer pointer)
    {
        return pointer.AddressType == Pointer.AddressType &&
               pointer.Address >= Pointer.Address &&
               pointer.Address < Pointer.Address + Size;
    }

    public override string ToString()
    {
        return $"{Pointer.AddressType} [0x{Pointer.Address:X}, 0x{Pointer.Address + Size:X}]";
    }
}

public struct ArraySegment<T>
{
    public AddressPointer Pointer { get; set; }
    public uint StructSize => (uint)Marshal.SizeOf<T>();
    public uint? ByteLength { get; set; }
    public uint? ElementCount => ByteLength is not null ? ByteLength / StructSize : null;

    public override string ToString()
    {
        string endAddress = $"0x{Pointer.Address:X} + 0x{StructSize:X} * n";

        if (ByteLength is not null)
            endAddress = $"0x{Pointer.Address + ByteLength:X} ({ElementCount} elements)";

        return $"{Pointer.AddressType} [0x{Pointer.Address:X}, {endAddress}]";
    }
}

public enum AddressType
{
    Virtual,
    Raw
}
