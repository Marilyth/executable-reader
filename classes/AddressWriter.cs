using System.Text;

public class AddressAnnotation
{
    public List<string> Entries { get; set; } = new();
    public string Category { get; set; }
    public int Priority { get; set; }
}

public class AddressWriter
{
    private Dictionary<AddressPointer, string> Labels = new();
    private Dictionary<AddressPointer, Dictionary<string, AddressAnnotation>> Items = new();

    public void SetLabel(AddressPointer pointer, string label)
    {
        Labels[pointer] = label;
    }

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
            string label = Labels.ContainsKey(annotation.Key) ? Labels[annotation.Key] : string.Empty;
            string prefix = $"0x{annotation.Key.Address:X}: ";
            sb.AppendLine(prefix + label);

            for (int annotationIndex = 0; annotationIndex < orderedAnnotations.Count; annotationIndex++)
            {
                AddressAnnotation annotations = orderedAnnotations[annotationIndex];
                string categoryPrefix = new string(' ', prefix.Length) + annotations.Category;

                if (annotations.Category != string.Empty)
                {
                    categoryPrefix += $"[{annotations.Entries.Count}]";
                    categoryPrefix += ": ";

                    sb.AppendLine(categoryPrefix);
                }

                for (int entryIndex = 0; entryIndex < annotations.Entries.Count; entryIndex++)
                {
                    string entry = new string(' ', categoryPrefix.Length) + annotations.Entries[entryIndex];
                    sb.AppendLine(entry);
                }
            }
        }

        return sb.ToString();
    }
}