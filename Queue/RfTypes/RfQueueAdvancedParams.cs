using System;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace RuSharpX.Queue.RfTypes;

public class RfQueueAdvancedParams
{
    /// <summary>
    /// Whether this transfer will make use of the global skip-list.
    /// </summary>
    public bool UseGlobalSkipList = true;

    /// <summary>
    /// Whether this transfer is in "synchronization mode", which would make the two
    /// sites equivalent to each-other.
    /// </summary>
    public bool EnableSynchronization = false;

    /// <summary>
    /// Whether this transfer is recursive.
    /// </summary>
    public bool IncludeSubfolders = true;

    /// <summary>
    /// Whether paths in the advanced string options contain regular expressions.
    /// </summary>
    public bool UseRegularExpressions = false;

    /// <summary>
    /// Only applicable when <see cref="EnableSynchronization"/> is true.
    /// <br/>Whether the synchronization will ignore new or deleted files.
    /// </summary>
    public bool SyncExistingFilesOnly = false;

    /// <summary>
    /// Used alongside <see cref="SizeParam"/> to limit what files get transferred between sites.
    /// </summary>
    public RfQueueAdvancedFileSizeMode FileSizeMode = RfQueueAdvancedFileSizeMode.Disabled;

    /// <summary>
    /// Whether <see cref="DateParam1"/>, <see cref="DateParam2"/>, and <see cref="FileNotOlderThanMode"/>
    /// have an effect on directories or just files.
    /// </summary>
    public bool ApplyDateConditionToFolders = false;

    /// <summary>
    /// Only applicable when <see cref="EnableSynchronization"/> is true.
    /// <br/>Whether the synchronization will delete files in the destination if they
    /// do not exist on the source.
    /// </summary>
    public bool SyncDeleteNonExistentFiles = false;

    /// <summary>
    /// Only applicable when <see cref="EnableSynchronization"/> is true.
    /// <br/>Whether the synchronization will compare using file date-times.
    /// </summary>
    public bool SyncCompareFileDateTime = false;

    /// <summary>
    /// Only applicable when <see cref="EnableSynchronization"/> is true.
    /// <br/>Whether the synchronization will compare using file sizes.
    /// </summary>
    public bool SyncCompareFileSize = false;

    /// <summary>
    /// This affects the behavior of <see cref="DateParam1"/> and <see cref="DateParam2"/>.
    /// <br/>If this is false, it is set to "Date Between" mode.
    /// </summary>
    public bool FileNotOlderThanMode = false;

    public bool SyncUseBinaryModeForAscii = false;

    public bool SyncBothSides = false;

    /// <summary>
    /// Whether the client should disconnect after this transfer is completed.
    /// </summary>
    public bool DisconnectAfterComplete = false;

    public bool Unknown15 = false;

    /// <summary>
    /// When <see cref="FileSizeMode"/> is set to something other than <c>FileSizeMode.Disabled</c>,
    /// this option determines the size for that option in bytes.
    /// </summary>
    public long SizeParam;

    /// <summary>
    /// - If in "Date Between" mode:<br/>
    ///   In "Date Between" mode, Date Param 1 is set to the TDateTime value of the earliest allowed date.<br/>
    /// - If in "File not older than" mode:<br/>
    ///   In "File not older than" mode, Date Param 1 is set to the numerical value of timeframes allowed.<br/> 
    ///
    /// <br/> If both this, and <see cref="DateParam2"/> are set to 0, date checks are disabled entirely.
    /// </summary>
    public int DateParam1;

    /// <summary>
    /// - If in "Date Between" mode:<br/>
    ///   In "Date Between" mode, Date Param 2 is set to the TDateTime value of the latest allowed date.<br/>
    /// - If in "File not older than" mode:<br/>
    ///   In "File not older than" mode, Date Param 2 is set to the enum value of the timeframe;<br/>
    ///    - 1: Day(s)<br/>
    ///    - 2: Week(s)<br/>
    ///    - 3: Month(s)<br/>
    ///    - 4: Year(s)<br/>
    /// 
    /// <br/> If both this, and <see cref="DateParam1"/> are set to 0, date checks are disabled entirely.
    /// </summary>
    public int DateParam2;

    /// <summary>
    /// Creates a <see cref="RfQueueAdvancedParams"/> with default values.
    /// </summary>
    public RfQueueAdvancedParams()
    {
    }

    /// <summary>
    /// Creates a <see cref="RfQueueAdvancedParams"/> from an Advanced Parameters string
    /// </summary>
    /// <param name="advancedParams">The serialized Advanced Parameters string</param>
    /// <exception cref="ArgumentException">Raised when the provided <paramref name="advancedParams"/> do not match the spec requirements</exception>
    public RfQueueAdvancedParams(string advancedParams)
    {
        string[] aParams = advancedParams.Split(",");
        if (aParams.Length != 4)
            throw new ArgumentException("There must only be 4 advanced parameters.", nameof(advancedParams));

        if (aParams[0].Length != 15)
            throw new ArgumentException("Options IntMap did not contain 15 integers.", nameof(advancedParams));

        this.UseGlobalSkipList = Convert.ToBoolean(Int32.Parse(aParams[0][0].ToString()));
        this.EnableSynchronization = Convert.ToBoolean(Int32.Parse(aParams[0][1].ToString()));
        this.IncludeSubfolders = Convert.ToBoolean(Int32.Parse(aParams[0][2].ToString()));
        this.UseRegularExpressions = Convert.ToBoolean(Int32.Parse(aParams[0][3].ToString()));
        this.SyncExistingFilesOnly = Convert.ToBoolean(Int32.Parse(aParams[0][4].ToString()));
        this.FileSizeMode = (RfQueueAdvancedFileSizeMode)Int32.Parse(aParams[0][5].ToString());
        this.ApplyDateConditionToFolders =
            Convert.ToBoolean(Int32.Parse(aParams[0][6].ToString()));
        this.SyncDeleteNonExistentFiles =
            Convert.ToBoolean(Int32.Parse(aParams[0][7].ToString()));
        this.SyncCompareFileDateTime =
            Convert.ToBoolean(Int32.Parse(aParams[0][8].ToString()));
        this.SyncCompareFileSize = Convert.ToBoolean(Int32.Parse(aParams[0][9].ToString()));
        this.FileNotOlderThanMode = Convert.ToBoolean(Int32.Parse(aParams[0][10].ToString()));
        this.SyncUseBinaryModeForAscii =
            Convert.ToBoolean(Int32.Parse(aParams[0][11].ToString()));
        this.SyncBothSides = Convert.ToBoolean(Int32.Parse(aParams[0][12].ToString()));
        this.DisconnectAfterComplete =
            Convert.ToBoolean(Int32.Parse(aParams[0][13].ToString()));
        this.Unknown15 = Convert.ToBoolean(Int32.Parse(aParams[0][14].ToString()));

        this.SizeParam = long.Parse(aParams[1]);
        this.DateParam1 = Int32.Parse(aParams[2]);
        this.DateParam2 = Int32.Parse(aParams[3]);
    }

    internal string Encode()
    {
        int useGlobalSkipList = Convert.ToInt32(this.UseGlobalSkipList);
        int enableSynchronization = Convert.ToInt32(this.EnableSynchronization);
        int includeSubfolders = Convert.ToInt32(this.IncludeSubfolders);
        int useRegularExpressions = Convert.ToInt32(this.UseRegularExpressions);
        int syncExistingFilesOnly = Convert.ToInt32(this.SyncExistingFilesOnly);
        int fileSizeMode = Convert.ToInt32(this.FileSizeMode);
        int syncApplyDateConditionToFolders = Convert.ToInt32(this.ApplyDateConditionToFolders);
        int syncDeleteNonExistentFiles = Convert.ToInt32(this.SyncDeleteNonExistentFiles);
        int syncCompareFileDateTime = Convert.ToInt32(this.SyncCompareFileDateTime);
        int syncCompareFileSize = Convert.ToInt32(this.SyncCompareFileSize);
        int fileNotOlderThanMode = Convert.ToInt32(this.FileNotOlderThanMode);
        int syncUseBinaryModeForAscii = Convert.ToInt32(this.SyncUseBinaryModeForAscii);
        int syncBothSides = Convert.ToInt32(this.SyncBothSides);
        int disconnectAfterComplete = Convert.ToInt32(this.DisconnectAfterComplete);
        int unknown15 = Convert.ToInt32(this.Unknown15);
        long sizeParam = Convert.ToInt64(this.SizeParam);
        int dateParam1 = Convert.ToInt32(this.DateParam1);
        int dateParam2 = Convert.ToInt32(this.DateParam2);

        return $"{useGlobalSkipList}{enableSynchronization}{includeSubfolders}{useRegularExpressions}" +
               $"{syncExistingFilesOnly}{fileSizeMode}{syncApplyDateConditionToFolders}{syncDeleteNonExistentFiles}" +
               $"{syncCompareFileDateTime}{syncCompareFileSize}{fileNotOlderThanMode}{syncUseBinaryModeForAscii}" +
               $"{syncBothSides}{disconnectAfterComplete}{unknown15},{sizeParam},{dateParam1},{dateParam2}";
    }

    public override string ToString() => Encode();
}