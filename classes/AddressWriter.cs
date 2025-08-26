using System.Text;

public class AddressAnnotation
{
    public List<string> Entries { get; set; } = new();
    public string Category { get; set; }
    public int Priority { get; set; }
}

public class AddressWriter
{
    private Dictionary<AddressPointer, Dictionary<string, AddressAnnotation>> Items = new();

    public void AddAnnotation(AddressPointer pointer, string text, string category = "", int priority = 0)
    {
        if (!Items.ContainsKey(pointer))
            Items[pointer] = new Dictionary<string, AddressAnnotation>();

        if (!Items[pointer].ContainsKey(category))
        {
            Items[pointer][category] = new AddressAnnotation
            {
                Category = category,
                Priority = priority
            };
        }

        Items[pointer][category].Entries.Add(text);
    }

    public List<AddressAnnotation> GetAnnotations(AddressPointer pointer)
    {
        if (Items.ContainsKey(pointer))
            return Items[pointer].OrderByDescending(a => a.Value.Priority).Select(a => a.Value).ToList();

        return new List<AddressAnnotation>();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var annotation in Items.OrderBy(kvp => kvp.Key.Address))
        {
            List<AddressAnnotation> orderedAnnotations = GetAnnotations(annotation.Key);
            string prefix = $"0x{annotation.Key.Address:X}: ";
            sb.Append(prefix);

            for (int annotationIndex = 0; annotationIndex < orderedAnnotations.Count; annotationIndex++)
            {
                AddressAnnotation annotations = orderedAnnotations[annotationIndex];
                string categoryPrefix = annotations.Category;

                if (annotations.Category != string.Empty)
                {
                    if (annotations.Entries.Count > 1)
                        categoryPrefix += $"[{annotations.Entries.Count}]";

                    categoryPrefix += ": ";
                }

                for (int entryIndex = 0; entryIndex < annotations.Entries.Count; entryIndex++)
                {
                    string entry = annotations.Entries[entryIndex];

                    if (entryIndex == 0)
                        entry = categoryPrefix + entry;
                    else if (entryIndex > 0)
                        entry = new string(' ', categoryPrefix.Length) + entry;

                    if (entryIndex > 0 || annotationIndex > 0)
                        entry = new string(' ', prefix.Length) + entry;

                    sb.AppendLine(entry);
                }
            }
        }

        return sb.ToString();
    }
}