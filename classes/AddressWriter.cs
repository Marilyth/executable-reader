using System.Text;

public class CategoryEntry
{
    public string Text { get; set; }

    public override string ToString()
    {
        return Text;
    }
}

public class Category
{
    public List<CategoryEntry> Entries { get; set; } = new();
    public string CategoryName { get; set; }
    public int Priority { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        string prefix = CategoryName;

        if (CategoryName != string.Empty)
        {
            prefix += $"[{Entries.Count}]";
            prefix += ": ";
        }

        sb.Append(prefix);

        for (int entryIndex = 0; entryIndex < Entries.Count; entryIndex++)
        {
            string entryString = Entries[entryIndex].ToString();

            if (entryIndex != 0)
                entryString = new string(' ', prefix.Length) + entryString;

            sb.AppendLine(entryString);
        }

        return sb.ToString().Trim();
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
            if (Target != null && IsFunctionStart() && IsFunctionEnd())
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

    public override string ToString()
    {
        List<Category> orderedCategories = Categories.Select(c => c.Value).OrderByDescending(c => c.Priority).ToList();

        StringBuilder sb = new StringBuilder();
        string prefix = $"0x{AddressPointer.Address:X}: ";

        if (Label is not null)
            sb.AppendLine(prefix + Label);
        else
            sb.Append(prefix);

        if (Target != null)
            Categories[string.Empty].Entries.First().Text += $"\t\t\t// -> {Target.Label}";

        for (int i = 0; i < orderedCategories.Count; i++)
        {
            string[] categoryExpressions = orderedCategories[i].ToString().Split(Environment.NewLine);
            for (int j = 0; j < categoryExpressions.Length; j++)
            {
                string categoryLine = categoryExpressions[j];

                if (i > 0 || Label is not null)
                    categoryLine = new string(' ', prefix.Length) + categoryLine;

                sb.AppendLine(categoryLine);
            }
        }

        return sb.ToString().Trim();
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
                sb.AppendLine();
                _currentIndentationLevel++;
            }
                
            if (_currentIndentationLevel == 0)
                sb.AppendLine(annotation.ToString());
            else
            {
                string annotationExpression = annotation.ToString();
                IEnumerable<string> lines = annotationExpression.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(l => new string('-', _currentIndentationLevel * 4) + l);
                sb.AppendLine(string.Join(Environment.NewLine, lines));
            }

            if (_currentIndentationLevel > 0 && annotation.IsFunctionEnd())
            {
                _currentIndentationLevel--;
                sb.AppendLine();
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