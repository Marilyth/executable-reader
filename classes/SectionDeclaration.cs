
using System.Runtime.InteropServices;
using System.Text;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SectionDeclaration
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] NameBytes;
    public string Name => Encoding.ASCII.GetString(NameBytes).TrimEnd('\0');
    public uint VirtualSize;
    public uint VirtualAddress;
    public uint SizeOfRawData;
    public uint PointerToRawData;
    public uint PointerToRelocations;
    public uint PointerToLinenumbers;
    public ushort NumberOfRelocations;
    public ushort NumberOfLinenumbers;
    public uint Characteristics;
}