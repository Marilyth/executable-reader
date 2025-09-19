using System.Runtime.InteropServices;
using System.Text;

public class ImportTable
{
    public ImportTable(uint tableStart, uint tableSize)
    {
        TableSegment = new ByteSegment()
        {
            Pointer = new AddressPointer()
            {
                AddressType = AddressType.Virtual,
                Address = tableStart
            },
            Size = tableSize
        };

        DescriptorSegments = new ArraySegment<ImageImportDescriptor>()
        {
            Pointer = new AddressPointer()
            {
                AddressType = AddressType.Virtual,
                Address = tableStart
            }
        };
    }

    public List<ImageImport> Imports { get; set; } = new();
    public ByteSegment TableSegment { get; set; }
    public ArraySegment<ImageImportDescriptor> DescriptorSegments { get; set; }

    public void Parse(PEReader reader)
    {
        ReadOnlySpan<byte> data = reader.GetDataForSegment(DescriptorSegments);
        ImageImportDescriptor[] descriptors = MemoryMarshal.Cast<byte, ImageImportDescriptor>(data).ToArray();

        foreach (ImageImportDescriptor descriptor in descriptors)
        {
            ImageImport import = new(descriptor);
            import.Parse(reader);
            Imports.Add(import);
        }
    }

    public override string ToString()
    {
        return $"Import Table ({Imports.Count} dlls imported)\n\n" +
            string.Join("\n\n", Imports.Select(i => i.ToString()));
    }
}

public class ImageImport
{
    public ImageImport(ImageImportDescriptor descriptor)
    {
        ImageNameAddress = new ArraySegment<char>()
        {
            Pointer = new AddressPointer()
            {
                AddressType = AddressType.Virtual,
                Address = descriptor.Name
            }
        };

        FunctionNameBaseAddresses = new ArraySegment<uint>()
        {
            Pointer = new AddressPointer()
            {
                AddressType = AddressType.Virtual,
                Address = descriptor.OriginalFirstThunk
            }
        };

        FunctionBaseAddresses = new ArraySegment<uint>()
        {
            Pointer = new AddressPointer()
            {
                AddressType = AddressType.Virtual,
                Address = descriptor.FirstThunk
            }
        };
    }

    public ArraySegment<char> ImageNameAddress { get; set; }
    public ArraySegment<uint> FunctionNameBaseAddresses { get; set; }
    public ArraySegment<uint> FunctionBaseAddresses { get; set; }
    public List<FunctionImport> Functions { get; set; } = new();
    public string ImageName { get; set; }

    public void Parse(PEReader reader)
    {
        ReadOnlySpan<byte> imageNameData = reader.GetDataForSegment(ImageNameAddress);
        ImageName = Encoding.ASCII.GetString(imageNameData);

        ReadOnlySpan<byte> functionNameBaseData = reader.GetDataForSegment(FunctionNameBaseAddresses);
        uint[] functionNameBaseAddresses = MemoryMarshal.Cast<byte, uint>(functionNameBaseData).ToArray();

        for (int i = 0; i < functionNameBaseAddresses.Length; i++)
        {
            FunctionImport import = new(
                new AddressPointer()
                {
                    AddressType = AddressType.Virtual,
                    Address = functionNameBaseAddresses[i]
                },
                new AddressPointer()
                {
                    AddressType = AddressType.Virtual,
                    Address = FunctionBaseAddresses.Pointer.Address + (uint)(i * 4)
                });

            import.Parse(reader);
            Functions.Add(import);
        }
    }

    public override string ToString()
    {
        return $"{ImageName} ({Functions.Count} functions imported):\n" +
            string.Join("\n", Functions.Select(f => $"\t{f}"));
    }
}

public class FunctionImport
{
    public FunctionImport(AddressPointer nameAddress, AddressPointer functionAddress)
    {
        HintSegment = new ByteSegment()
        {
            Pointer = nameAddress,
            Size = 2
        };

        NameSegment = new ArraySegment<char>()
        {
            Pointer = nameAddress + HintSegment.Size
        };

        FunctionAddress = new ByteSegment()
        {
            Pointer = functionAddress,
            Size = 4
        };
    }

    public ushort Hint { get; set; }
    public string Name { get; set; }
    public ByteSegment HintSegment { get; set; }
    public ArraySegment<char> NameSegment { get; set; }
    public ByteSegment FunctionAddress { get; set; }

    public void Parse(PEReader reader)
    {
        Hint = MemoryMarshal.Read<ushort>(reader.GetDataForSegment(HintSegment));
        ReadOnlySpan<byte> nameData = reader.GetDataForSegment(NameSegment);
        Name = Encoding.ASCII.GetString(nameData);
    }

    public override string ToString()
    {
        return $"{Name} ({FunctionAddress.Pointer}, Export offset 0x{Hint})";
    }
}