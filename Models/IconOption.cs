namespace WhiteboardProjectBuilder.Models;

public class IconOption<T> where T : struct, Enum
{
    public T EnumValue { get; set; }
    public string IconPath { get; set; } = string.Empty;

    public static List<IconOption<T>> Create(Func<T, string> toIconFunc)
    {
        return Enum.GetValues<T>()
            .Select(value => new IconOption<T>
            {
                EnumValue = value,
                IconPath = toIconFunc(value)
            })
            .ToList();
    }
}
