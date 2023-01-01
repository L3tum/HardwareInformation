#region using

using System;

#endregion

[AttributeUsage(
    AttributeTargets.Class |
    AttributeTargets.Delegate |
    AttributeTargets.Enum |
    AttributeTargets.Field |
    AttributeTargets.Struct)]
internal class MapAttribute : Attribute
{
    public MapAttribute()
    {
    }

    public MapAttribute(string nativeType)
    {
        NativeType = nativeType;
    }

    public string NativeType { get; }

    public string SuppressFlags { get; set; }
}