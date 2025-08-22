using System;
using System.Text;

public class Section : ByteContainer
{
    public Section(SectionDeclaration declaration, byte[] data) : base(data)
    {
        SectionDeclaration = declaration;
    }

    protected ReadOnlySpan<byte> SectionData
        => Data.Slice((int)SectionDeclaration.PointerToRawData, (int)SectionDeclaration.SizeOfRawData);

    public SectionDeclaration SectionDeclaration { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new($"{SectionDeclaration.Name}:\n");
        sb.AppendLine($"\tSpans raw [0x{SectionDeclaration.PointerToRawData:X}, 0x{SectionDeclaration.PointerToRawData + SectionDeclaration.SizeOfRawData:X}]," +
                      $" virtual [0x{SectionDeclaration.VirtualAddress:X}, 0x{SectionDeclaration.VirtualAddress + SectionDeclaration.VirtualSize:X}]");
        sb.AppendLine($"\tCharacteristics: {SectionDeclaration.Characteristics:F}");
        sb.AppendLine($"\tData: {BitConverter.ToString(SectionData.ToArray().Take(50).ToArray()).Replace("-", " ")}...");

        return sb.ToString();
    }
}