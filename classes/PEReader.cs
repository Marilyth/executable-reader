using System.Reflection;
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

    public AddressWriter Annotations { get; set; } = new();
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

            Sections.Add(new Section(section));
        }
    }

    public ReadOnlySpan<byte> GetDataForSegment(ByteSegment segment)
    {
        AddressPointer rawPointer = VirtualToRawPointer(segment.Pointer);

        return Data.Slice((int)rawPointer.Address, (int)segment.Size);
    }

    public void OutputInformation()
    {
        List<(string, string)> outputFiles = new();
        outputFiles.Add((Header.ToString(), "Header"));

        uint entryPointAddress = Header.StandardFields.AddressOfEntryPoint;
        AddressPointer entryPointPointer = new()
        {
            AddressType = AddressType.Virtual,
            Address = entryPointAddress,
        };

        outputFiles.Add(($"Entrypoint at section {GetSectionForPointer(entryPointPointer).SectionDeclaration.Name}\n" +
                         $"virtual address 0x{entryPointAddress:X}\n" +
                         $"raw address 0x{VirtualToRawPointer(entryPointPointer).Address:X}", "EntryPoint"));

        outputFiles.Add((string.Join("\n\n", Sections.Select(s => s.ToString())), "Sections"));

        // Disassemble executable sections.
        foreach (var section in Sections.Where(s => s.SectionDeclaration.Characteristics.HasFlag(SectionCharacteristics.IMAGE_SCN_MEM_EXECUTE)))
        {
            FillAddressWriter(section);
        }

        Annotations.SetLabel(entryPointPointer, "entry_point");
        outputFiles.Add((Annotations.ToString(), $"Disassembly"));

        Directory.CreateDirectory("output");

        foreach (var file in outputFiles)
        {
            File.WriteAllText($"output/{file.Item2}.txt", file.Item1);
        }
    }

    private Section GetSectionForPointer(AddressPointer pointer)
    {
        Section? section = Sections.FirstOrDefault(s => s.ContainsPointer(pointer));

        if (section == null)
            throw new ArgumentException("Invalid pointer");

        return section;
    }

    private AddressPointer VirtualToRawPointer(AddressPointer pointer)
    {
        if (pointer.AddressType == AddressType.Raw)
            return pointer;

        Section section = GetSectionForPointer(pointer);

        // Calculate the virtual offset
        uint offset = pointer.Address - section.SectionDeclaration.VirtualAddress;

        return new AddressPointer
        {
            AddressType = AddressType.Raw,
            Address = section.SectionDeclaration.PointerToRawData + offset
        };
    }

    private void FillAddressWriter(Section codeSection)
    {
        ReadOnlySpan<byte> sectionData = GetDataForSegment(codeSection.SectionSegment);
        Annotations.AddAnnotation(codeSection.SectionSegment.Pointer, codeSection.SectionDeclaration.Name, "Section", int.MaxValue);

        Decoder decoder = Decoder.Create(32, new ByteArrayCodeReader(sectionData.ToArray()));

        IntelFormatter formatter = new IntelFormatter();
        StringOutput output = new();

        while (decoder.IP < codeSection.SectionSegment.Size)
        {
            Instruction instruction = decoder.Decode();
            formatter.Format(instruction, output);
            AddressPointer rva = new() { AddressType = AddressType.Virtual, Address = (uint)instruction.IP + codeSection.SectionDeclaration.VirtualAddress };

            string instructionMachineCode = BitConverter.ToString(sectionData.Slice((int)instruction.IP, instruction.Length).ToArray()).Replace("-", " ");

            Annotations.AddAnnotation(rva, output.ToStringAndReset(), string.Empty);
            Annotations.AddAnnotation(rva, instructionMachineCode, string.Empty);

            if (instruction.NearBranchTarget != 0)
            {
                AddressPointer targetAddress = new() { AddressType = AddressType.Virtual, Address = (uint)instruction.NearBranchTarget + codeSection.SectionDeclaration.VirtualAddress };
                Annotations.AddAnnotation(targetAddress, $"0x{rva.Address:X} ({instruction.Code})", "XREF", -1);

                if (instruction.Code.ToString().StartsWith("Call"))
                    Annotations.SetLabel(targetAddress, $"FUN_{targetAddress.Address:X}");
                else
                    Annotations.SetLabel(targetAddress, $"LAB_{targetAddress.Address:X}");
            }
        }
    }
}