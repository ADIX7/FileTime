namespace GeneralInputKey;

public record struct SpecialKeysStatus(bool IsAltPressed, bool IsShiftPressed, bool IsCtrlPressed)
{
    public static SpecialKeysStatus Default { get; } = new(false, false, false); 
}