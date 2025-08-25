using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class PEReader : ByteContainer
{
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
        int sectionsOffset = _peHeaderOffset + Marshal.SizeOf<COFFHeader>() + Header.CoffHeader.SizeOfOptionalHeader;

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

    public Section GetSectionForRVA(uint rva)
    {
        Section? section = Sections.FirstOrDefault(s =>
            s.SectionDeclaration.VirtualAddress <= rva &&
            (s.SectionDeclaration.VirtualAddress + s.SectionDeclaration.VirtualSize) > rva);

        if (section == null)
            throw new ArgumentException("Invalid RVA");

        return section;
    }

    public uint RVAToRawAddress(uint rva)
    {
        Section section = GetSectionForRVA(rva);

        // Calculate the virtual offset
        uint offset = rva - section.SectionDeclaration.VirtualAddress;

        return section.SectionDeclaration.PointerToRawData + offset;
    }

    public override string ToString()
    {
        List<string> segments = new();
        segments.Add($"PE Header:\n{Header}");

        uint entryPointAddress = Header.StandardFields.AddressOfEntryPoint;
        segments.Add($"Entrypoint at section {GetSectionForRVA(entryPointAddress).SectionDeclaration.Name}\n" +
                     $"virtual address 0x{entryPointAddress:X}\n" +
                     $"raw address 0x{RVAToRawAddress(entryPointAddress):X}");

        segments.Add("Sections:");

        segments.AddRange(Sections.Select(s => s.ToString()));

        return string.Join("\n\n", segments);
    }
}