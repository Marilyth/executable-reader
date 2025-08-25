string fileToRead = args.Length > 0 ? args[0] : "C:\\Users\\m.baer\\Desktop\\Programme\\ReverseEngineering\\HelloWorlds\\MachineCode\\main.exe";

PEReader reader = new(File.ReadAllBytes(fileToRead));
reader.ReadPEHeader();
reader.ReadSections();

// Output the information to text files.
reader.OutputInformation();
