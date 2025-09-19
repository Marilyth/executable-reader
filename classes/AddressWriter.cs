using System.Text;

public class CategoryEntry
{
    public string Text { get; set; }

    public string ToString(int indentationLevel)
    {
        return new string(' ', indentationLevel) + Text;
    }
}

public class Category
{
    public List<CategoryEntry> Entries { get; set; } = new();
    public string CategoryName { get; set; }
    public int Priority { get; set; }

    public void BuildString(StringBuilder sb, int indentationLevel)
    {
        string prefix = new string(' ', indentationLevel) + CategoryName;

        if (CategoryName != string.Empty)
        {
            prefix += $"[{Entries.Count}]";
            prefix += ": ";
        }

        sb.Append(prefix);

        for (int entryIndex = 0; entryIndex < Entries.Count; entryIndex++)
        {
            sb.AppendLine(Entries[entryIndex].ToString(entryIndex == 0 ? 0 : prefix.Length));
        }
    }
}

public class AddressAnnotations
{
    private string _label;

    public string Label
    {
        get
        {
            string label = _label;

            // We have a target and a label, so this is a thunk / stub function.
            if (Target != null && IsFunctionStart())
                label = $"THUNK_{Target.Label}";

            return label;
        }
        set
        {
            _label = value;
        }
    }
    public Dictionary<string, Category> Categories { get; set; } = new();
    public AddressPointer AddressPointer { get; set; }
    public AddressAnnotations? Target { get; set; }

    public bool IsFunctionEnd()
    {
        return !Categories.ContainsKey(string.Empty) || (Categories.ContainsKey(string.Empty) &&
            (Categories[string.Empty].Entries.Any(e => e.Text == "ret") ||
             Categories[string.Empty].Entries.Any(e => e.Text.StartsWith("jmp"))));
    }

    public bool IsFunctionStart()
    {
        return _label != null && !_label.StartsWith("LAB_");
    }

    public void AddAnnotation(string category, string text, int priority)
    {
        if (!Categories.ContainsKey(category))
        {
            Categories[category] = new Category
            {
                CategoryName = category,
                Entries = new List<CategoryEntry>()
            };
        }

        Categories[category].Entries.Add(new CategoryEntry { Text = text });
        Categories[category].Priority = priority;
    }

    public void BuildString(StringBuilder sb)
    {
        List<Category> orderedCategories = Categories.Select(c => c.Value).OrderByDescending(c => c.Priority).ToList();

        string prefix = $"0x{AddressPointer.Address:X}: ";
        sb.AppendLine(prefix + Label);

        if (Target != null)
            Categories[string.Empty].Entries.First().Text += $" // -> {Target.Label}";

        foreach (var category in orderedCategories)
                category.BuildString(sb, prefix.Length);
    }
}

public class AddressWriter
{
    private int _currentIndentationLevel;
    private Dictionary<AddressPointer, AddressAnnotations> Items = new();

    public void SetLabel(AddressPointer pointer, string label)
    {
        GetAddressAnnotations(pointer).Label = label;
    }

    public void SetTarget(AddressPointer pointer, AddressPointer target)
    {
        GetAddressAnnotations(pointer).Target = GetAddressAnnotations(target);
    }

    public void AddAnnotation(AddressPointer pointer, string text, string category = "", int priority = 0)
    {
        GetAddressAnnotations(pointer).AddAnnotation(category, text, priority);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var annotation in Items.OrderBy(kvp => kvp.Key.Address).Select(kvp => kvp.Value))
        {
            if (annotation.IsFunctionStart())
            {
                _currentIndentationLevel++;
            }
                
            if (_currentIndentationLevel == 0)
                    annotation.BuildString(sb);
                else
                {
                    StringBuilder indentedAnnotations = new StringBuilder();
                    annotation.BuildString(indentedAnnotations);
                    IEnumerable<string> lines = indentedAnnotations.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(l => new string('-', _currentIndentationLevel * 4) + l);
                    sb.AppendLine(string.Join(Environment.NewLine, lines));
                }

            if (_currentIndentationLevel > 0 && annotation.IsFunctionEnd())
            {
                _currentIndentationLevel--;
            }
        }

        return sb.ToString();
    }

    private AddressAnnotations GetAddressAnnotations(AddressPointer pointer)
    {
        if (!Items.ContainsKey(pointer))
        {
            Items[pointer] = new AddressAnnotations
            {
                AddressPointer = pointer,
            };
        }

        return Items[pointer];
    }
}