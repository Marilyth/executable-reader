using System;

public class Section : ByteContainer
{
    public Section(SectionDeclaration declaration, byte[] data) : base(data)
    {
        SectionDeclaration = declaration;
    }

    public SectionDeclaration SectionDeclaration { get; set; }
}