namespace AlexVirlan.Entities
{
    public enum PathType
    {
        File = 0,
        Directory = 1,
        Unknown = 2
    }

    public enum PropertiesType
    {
        Layers = 0,
        Polygons = 1
    }

    public enum StringRepeatType
    {
        Replace = 0,
        Concat = 1,
        SBInsert = 2,
        SBAppendJoin = 3
    }
}