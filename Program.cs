string fileToRead = args.Length > 0 ? args[0] : "main.exe";

PEReader reader = new(File.ReadAllBytes(fileToRead));
reader.ReadPEHeader();
reader.ReadSections();

Console.WriteLine(reader.ToString());
