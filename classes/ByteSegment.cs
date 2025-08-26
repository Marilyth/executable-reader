public struct AddressPointer
{
    public uint Address { get; set; }
    public AddressType AddressType { get; set; }

    public override string ToString()
    {
        return $"{AddressType} 0x{Address:X}";
    }
}

public struct ByteSegment
{
    public AddressPointer Pointer { get; set; }
    public uint Size { get; set; }
    
    public override string ToString()
    {
        return $"{Pointer.AddressType} [0x{Pointer.Address:X}, 0x{Pointer.Address + Size:X}]";
    }
}

public enum AddressType
{
    Virtual,
    Raw
}