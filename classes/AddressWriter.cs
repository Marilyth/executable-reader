using System.Text;

public class AddressAnnotation
{
    public AddressPointer Pointer { get; set; }
    public string Text { get; set; }
    public int Priority { get; set; }
}

public class AddressWriter
{
    private Dictionary<AddressPointer, List<AddressAnnotation>> Items = new();

    public void AddAnnotation(AddressPointer pointer, string text, int priority = 0)
    {
        if (!Items.ContainsKey(pointer))
            Items[pointer] = new List<AddressAnnotation>();

        Items[pointer].Add(new AddressAnnotation
        {
            Pointer = pointer,
            Text = text,
            Priority = priority
        });
    }

    public List<AddressAnnotation> GetAnnotations(AddressPointer pointer)
    {
        if (Items.ContainsKey(pointer))
            return Items[pointer].OrderByDescending(a => a.Priority).ToList();

        return new List<AddressAnnotation>();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        int highestAddressLength = $"{Items.Max(kvp => kvp.Key.Address):X}".Length;

        foreach (var annotation in Items.OrderBy(kvp => kvp.Key.Address))
        {
            List<AddressAnnotation> orderedAnnotations = GetAnnotations(annotation.Key);
            sb.AppendLine($"0x{annotation.Key.Address.ToString($"X{highestAddressLength}")}: {orderedAnnotations.First().Text}");

            foreach (var item in orderedAnnotations.Skip(1))
            {
                sb.AppendLine(new string(' ', highestAddressLength + 4) + item.Text);
            }
        }

        return sb.ToString();
    }
}