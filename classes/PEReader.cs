using System.Runtime.InteropServices;
using System.Text;
using Iced.Intel;
using Decoder = Iced.Intel.Decoder;

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

    public string Disassemble(Section codeSection)
    {
        StringBuilder sb = new();
        Decoder decoder = Decoder.Create(32, new ByteArrayCodeReader(codeSection.SectionData.ToArray()));

        IntelFormatter formatter = new IntelFormatter();
        StringOutput output = new();

        while (decoder.IP < (ulong)codeSection.SectionData.Length)
        {
            Instruction instruction = decoder.Decode();
            formatter.Format(instruction, output);
            ulong rva = instruction.IP + codeSection.SectionDeclaration.VirtualAddress;
            ulong rawAddress = instruction.IP + codeSection.SectionDeclaration.PointerToRawData;
            string instructionMachineCode = BitConverter.ToString(codeSection.SectionData.Slice((int)instruction.IP, instruction.Length).ToArray()).Replace("-", " ");

            sb.AppendLine($"RAW 0x{rawAddress:X}, RVA 0x{rva:X}:\t\t{output.ToStringAndReset()} ({instructionMachineCode})");
        }

        return sb.ToString();
    }

    public void OutputInformation()
    {
        List<(string, string)> segments = new();
        segments.Add((Header.ToString(), "Header"));

        uint entryPointAddress = Header.StandardFields.AddressOfEntryPoint;
        segments.Add(($"Entrypoint at section {GetSectionForRVA(entryPointAddress).SectionDeclaration.Name}\n" +
                     $"virtual address 0x{entryPointAddress:X}\n" +
                     $"raw address 0x{RVAToRawAddress(entryPointAddress):X}", "EntryPoint"));

        segments.Add((string.Join("\n\n", Sections.Select(s => s.ToString())), "Sections"));

        // Disassemble executable sections.
        foreach (var section in Sections.Where(s => s.SectionDeclaration.Characteristics.HasFlag(SectionCharacteristics.IMAGE_SCN_MEM_EXECUTE)))
        {
            segments.Add((Disassemble(section), $"Disassembly_{section.SectionDeclaration.Name}"));
        }

        foreach (var segment in segments) {
            File.WriteAllText($"{segment.Item2}.txt", segment.Item1);
        }
    }
}