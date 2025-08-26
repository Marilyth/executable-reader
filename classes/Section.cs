using System;
using System.Text;

public class Section
{
    public Section(SectionDeclaration declaration)
    {
        SectionDeclaration = declaration;
        SectionSegment = new()
        {
            Pointer = new AddressPointer
            {
                AddressType = AddressType.Virtual,
                Address = declaration.VirtualAddress
            },
            Size = declaration.SizeOfRawData
        };
    }

    public ByteSegment SectionSegment { get; set; } 
    public SectionDeclaration SectionDeclaration { get; set; }

    /// <summary>
    /// Returns whether the specified pointer is contained within the section.
    /// </summary>
    /// <param name="pointer">The pointer to check.</param>
    public bool ContainsPointer(AddressPointer pointer)
    {
        switch (pointer.AddressType)
        {
            case AddressType.Virtual:
                return pointer.Address >= SectionDeclaration.VirtualAddress &&
                       pointer.Address <= SectionDeclaration.VirtualAddress + SectionDeclaration.VirtualSize;
            case AddressType.Raw:
                return pointer.Address >= SectionDeclaration.PointerToRawData &&
                       pointer.Address <= SectionDeclaration.PointerToRawData + SectionDeclaration.SizeOfRawData;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new($"{SectionDeclaration.Name}:\n");
        sb.AppendLine($"\tSpans raw [0x{SectionDeclaration.PointerToRawData:X}, 0x{SectionDeclaration.PointerToRawData + SectionDeclaration.SizeOfRawData:X}]," +
                      $" virtual [0x{SectionDeclaration.VirtualAddress:X}, 0x{SectionDeclaration.VirtualAddress + SectionDeclaration.VirtualSize:X}]");
        sb.AppendLine($"\tCharacteristics: {SectionDeclaration.Characteristics:F}");

        return sb.ToString();
    }
}