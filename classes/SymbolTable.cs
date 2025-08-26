using System;
using System.Text;

public class SymbolTable
{
    public SymbolTable(Section section, uint pointerToSymbolTable, uint numberOfSymbols)
    {
        Section = section;
        PointerToSymbolTable = pointerToSymbolTable;
        NumberOfSymbols = numberOfSymbols;
    }

    public Section Section { get; set; }
    public uint PointerToSymbolTable { get; set; }
    public uint NumberOfSymbols { get; set; }
    
    public override string ToString()
    {
        return "";
    }
}