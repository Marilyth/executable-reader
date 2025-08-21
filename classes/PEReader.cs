using System;
using System.Runtime.InteropServices;
using System.Text.Json;

public class PEReader : ByteContainer
{
    private JsonSerializerOptions _options = new JsonSerializerOptions
    {
        WriteIndented = true,
        IncludeFields = true
    };
    private int _peHeaderOffset;

    public PEReader(byte[] data) : base(data)
    {
        ReadOnlySpan<byte> searchPattern = new byte[] { 0x50, 0x45, 0x00, 0x00 };
        _peHeaderOffset = Data.IndexOf(searchPattern);
    }

    public PEHeader Header { get; set; }
    public List<Section> Sections { get; set; } = new();

    public void ReadPEHeader()
    {
        ReadOnlySpan<byte> peHeaderSpan = Data.Slice(_peHeaderOffset, Marshal.SizeOf<PEHeader>());
        Header = MemoryMarshal.Read<PEHeader>(peHeaderSpan);
    }

    public void ReadSections()
    {
        int sectionsOffset = _peHeaderOffset + Marshal.SizeOf<PEHeader>();

        for (int i = 0; i < Header.CoffHeader.NumberOfSections; i++)
        {
            int sectionOffset = sectionsOffset + i * Marshal.SizeOf<SectionDeclaration>();
            ReadOnlySpan<byte> sectionSpan = Data.Slice(sectionOffset, Marshal.SizeOf<SectionDeclaration>());
            GCHandle handle = GCHandle.Alloc(sectionSpan.ToArray(), GCHandleType.Pinned);

            SectionDeclaration section = Marshal.PtrToStructure<SectionDeclaration>(handle.AddrOfPinnedObject())!;
            handle.Free();

            Sections.Add(new Section(section, _data));
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, _options);
    }
}