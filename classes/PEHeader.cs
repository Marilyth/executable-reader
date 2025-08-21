
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PEHeader
{
    public COFFHeader CoffHeader;
    public StandardCOFFFields StandardFields;
    public WindowsSpecificFields WindowsFields;
    public DataDirectories DataDirectories;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct COFFHeader
{
    public uint Signature;
    public ushort Machine;
    public ushort NumberOfSections;
    public uint TimeDateStamp;
    public uint PointerToSymbolTable;
    public uint NumberOfSymbols;
    public ushort SizeOfOptionalHeader;
    public ushort Characteristics;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StandardCOFFFields
{

    public ushort Magic;
    public byte MajorLinkerVersion;
    public byte MinorLinkerVersion;
    public uint SizeOfCode;
    public uint SizeOfInitializedData;
    public uint SizeOfUninitializedData;
    public uint AddressOfEntryPoint;
    public uint BaseOfCode;
    public uint BaseOfData;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WindowsSpecificFields
{

    public uint ImageBase;
    public uint SectionAlignment;
    public uint FileAlignment;
    public ushort MajorOperatingSystemVersion;
    public ushort MinorOperatingSystemVersion;
    public ushort MajorImageVersion;
    public ushort MinorImageVersion;
    public ushort MajorSubsystemVersion;
    public ushort MinorSubsystemVersion;
    public uint Win32VersionValue;
    public uint SizeOfImage;
    public uint SizeOfHeaders;
    public uint CheckSum;
    public ushort Subsystem;
    public ushort DllCharacteristics;
    public uint SizeOfStackReserve;
    public uint SizeOfStackCommit;
    public uint SizeOfHeapReserve;
    public uint SizeOfHeapCommit;
    public uint LoaderFlags;
    public uint NumberOfRvaAndSizes;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DataDirectories
{
    public uint ExportTable;
    public uint SizeOfExportTable;
    public uint ImportTable;
    public uint SizeOfImportTable;
    public uint ResourceTable;
    public uint SizeOfResourceTable;
    public uint ExceptionTable;
    public uint SizeOfExceptionTable;
    public uint CertificateTable;
    public uint SizeOfCertificateTable;
    public uint BaseRelocationTable;
    public uint SizeOfBaseRelocationTable;
    public uint DebugData;
    public uint SizeOfDebugData;
    public uint ArchitectureData;
    public uint SizeOfArchitectureData;
    public uint GlobalPtr;
    public uint Buffer;
    public uint TLSTable;
    public uint SizeOfTLSTable;
    public uint LoadConfigTable;
    public uint SizeOfLoadConfigTable;
    public uint BoundImport;
    public uint SizeOfBoundImport;
    public uint ImportAddressTable;
    public uint SizeOfImportAddressTable;
    public uint DelayImportDescriptor;
    public uint SizeOfDelayImportDescriptor;
    public uint CLRRuntimeHeader;
    public uint SizeOfCLRRuntimeHeader;
    public ulong Padding;
}