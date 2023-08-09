namespace TerminalUI.Controls;


public enum GridUnitType
{
    Auto,
    Pixel,
    Star
}
    
public record struct RowDefinition(GridUnitType Type, int Value)
{
    public static RowDefinition Auto => new(GridUnitType.Auto, 0);
    public static RowDefinition Pixel(int value) => new(GridUnitType.Pixel, value);
    public static RowDefinition Star(int value) => new(GridUnitType.Star, value);
}
    
public record struct ColumnDefinition(GridUnitType Type, int Value)
{
    public static ColumnDefinition Auto => new(GridUnitType.Auto, 0);
    public static ColumnDefinition Pixel(int value) => new(GridUnitType.Pixel, value);
    public static ColumnDefinition Star(int value) => new(GridUnitType.Star, value);
}