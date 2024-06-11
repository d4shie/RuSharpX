namespace RuSharpX.Queue.RfTypes;

public enum RfQueueFileType
{
    Unknown,
    Directory,
    File
}

public enum RfQueueTransferType
{
    Upload,
    Download,
    Fxp,
    Unknown3,
    Unknown4,
    Unknown5, // Maybe Delete
    Unknown6
}

public enum RfQueueAdvancedFileSizeMode
{
    Disabled,
    Equals,
    AtMost,
    AtLeast
}

public enum RfQueueAdvancedNotOlderThanTimeframe
{
    Disabled,
    Day,
    Week,
    Month,
    Year
}