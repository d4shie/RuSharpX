using System.Text;
using RuSharpX.Queue.RfTypes;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

// ReSharper disable RedundantNameQualifier

namespace RuSharpX.Queue;

public class QueueItem
{
    private static readonly byte[] BomMagic = new byte[2] { 0xFF, 0xFE };

    private static readonly byte[] ParameterSeparator = new byte[2] { 0x02, 0x00 };
    private static readonly byte[] ParameterTerminator = new byte[4] { 0x0D, 0x00, 0x0A, 0x00 };

    /// <summary>
    /// The <see cref="RfQueueFileType"/> of the current <see cref="QueueItem"/>.
    /// <br/>NOTE: This is a string for the sake of serialization, but the underlying type is of <see cref="RfQueueFileType"/>.
    /// </summary>
    public string FileType;

    /// <summary>
    /// The <see cref="RfQueueTransferType"/> of the current <see cref="QueueItem"/>.
    /// <br/>NOTE: This is a string for the sake of serialization, but the underlying type is of <see cref="RfQueueTransferType"/>.
    /// </summary>
    public string TransferType;

    /// <summary>
    /// The source site unique id (UID). This can be a 32-character hexadecimal string, or <c>Local</c>.
    /// </summary>
    public string SrcSiteUid;

    /// <summary>
    /// The source site directory path. This is the parent directory to the item being transferred.
    /// <br/>NOTE: If the item being transferred is a directory, this is the parent directory,
    /// instead relying on <see cref="SrcSiteFname"/> for the actual directory to transfer.
    /// </summary>
    public string SrcSitePath;

    /// <summary>
    /// The source site file name. Does not include the path (see <see cref="SrcSitePath"/>)
    /// <br/>NOTE: If the item being transferred is a directory, this is the directory name itself,
    /// as the path to its parent is provided in <see cref="SrcSitePath"/>.
    /// </summary>
    public string SrcSiteFname;

    /// <summary>
    /// The destination site unique id (UID). This can be a 32-character hexadecimal string, or <c>Local</c>.
    /// </summary>
    public string DstSiteUid;

    /// <summary>
    /// The destination site directory path. This is the parent directory to the item being transferred.
    /// <br/>NOTE: If the item being transferred is a directory, this is the parent directory,
    /// instead relying on <see cref="DstSiteFname"/> for the actual directory to transfer.
    /// </summary>
    public string DstSitePath;

    /// <summary>
    /// The destination site file name. Does not include the path (see <see cref="DstSitePath"/>)
    /// <br/>NOTE: If the item being transferred is a directory, this is the directory name itself,
    /// as the path to its parent is provided in <see cref="DstSitePath"/>.
    /// </summary>
    public string DstSiteFname;

    /// <summary>
    /// The size of the item in bytes.
    /// <br/>NOTE: This is a string for the sake of serialization, but the underlying type is of <see cref="int"/>.
    /// </summary>
    public string ItemSize;

    /// <summary>
    /// This is an unknown value, that has only been observed to be set as "1".
    /// <br/>NOTE: This is a string for the sake of serialization, but the underlying type is of <see cref="int"/>.
    /// </summary>
    public string UnknownIndex4;

    public RfQueueAdvancedParams? AdvancedParams;

    /// <summary>
    /// The remark on the item. This can be a string of any length.
    /// </summary>
    public string Remark;

    public string AdvancedFolderInclude = string.Empty;
    public string AdvancedFolderExclude = string.Empty;
    public string AdvancedFileInclude = string.Empty;
    public string AdvancedFileExclude = string.Empty;

    /// <summary>
    /// Creates a new <see cref="QueueItem"/> from the given parameters.
    /// </summary>
    /// <param name="fileType"><inheritdoc cref="FileType"/></param>
    /// <param name="transferType"><inheritdoc cref="TransferType"/></param>
    /// <param name="srcSiteUid"><inheritdoc cref="SrcSiteUid"/></param>
    /// <param name="srcSitePath"><inheritdoc cref="SrcSitePath"/></param>
    /// <param name="srcSiteFname"><inheritdoc cref="SrcSiteFname"/></param>
    /// <param name="dstSiteUid"><inheritdoc cref="DstSiteUid"/></param>
    /// <param name="dstSitePath"><inheritdoc cref="DstSitePath"/></param>
    /// <param name="dstSiteFname"><inheritdoc cref="DstSiteFname"/></param>
    /// <param name="itemSizeBytes"><inheritdoc cref="ItemSize"/></param>
    /// <param name="idx4"></param>
    /// <param name="advancedParams"><inheritdoc cref="AdvancedParams"/></param>
    /// <param name="remark"><inheritdoc cref="Remark"/></param>
    public QueueItem(RfQueueFileType fileType, RfQueueTransferType transferType, string srcSiteUid,
        string srcSitePath,
        string srcSiteFname,
        string dstSiteUid, string dstSitePath, string dstSiteFname, int itemSizeBytes, int idx4,
        RfQueueAdvancedParams advancedParams,
        string remark)
    {
        this.FileType = ((int)fileType).ToString();
        this.TransferType = ((int)transferType).ToString();

        this.SrcSiteUid = srcSiteUid;
        this.SrcSitePath = srcSitePath;
        this.SrcSiteFname = srcSiteFname;

        this.DstSiteUid = dstSiteUid;
        this.DstSitePath = dstSitePath;
        this.DstSiteFname = dstSiteFname;

        this.ItemSize = itemSizeBytes.ToString();
        this.UnknownIndex4 = idx4.ToString();

        this.AdvancedParams = advancedParams;

        this.Remark = remark;
    }

    /// <summary>
    /// Creates a <see cref="QueueItem"/> from an entry's bytes.
    /// </summary>
    /// <param name="entry"></param>
    public QueueItem(byte[] entry)
    {
        int idx = 0;
        int nextOff = 0;
        if (entry[0..2] == BomMagic) idx += 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.FileType = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.TransferType = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;


        // Source site info
        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.SrcSiteUid = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.SrcSitePath = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.SrcSiteFname = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;


        // Destination site info
        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.DstSiteUid = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.DstSitePath = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.DstSiteFname = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        // Item Size
        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.ItemSize = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.UnknownIndex4 = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.AdvancedParams = new RfQueueAdvancedParams(Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]));
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.Remark = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.AdvancedFolderInclude = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.AdvancedFolderExclude = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.AdvancedFileInclude = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;

        nextOff = SearchForBytePattern(entry[idx..], ParameterSeparator);
        this.AdvancedFileExclude = Encoding.Unicode.GetString(entry[idx..(idx + nextOff)]);
        idx += nextOff + 2;
    }

    /// <summary>
    /// Encodes a <see cref="QueueItem"/> into a <see cref="T:byte[]"/>.
    /// </summary>
    /// <returns>A <see cref="T:byte[]"></see> containing the encoded version of this <see cref="QueueItem"/>.</returns>
    public byte[] Encode()
    {
        byte[] fileType = Encoding.Unicode.GetBytes(this.FileType);
        byte[] transferType = Encoding.Unicode.GetBytes(this.TransferType);

        byte[] srcSiteUid = Encoding.Unicode.GetBytes(this.SrcSiteUid);
        byte[] srcSitePath = Encoding.Unicode.GetBytes(this.SrcSitePath);
        byte[] srcSiteFname = Encoding.Unicode.GetBytes(this.SrcSiteFname);

        byte[] dstSiteUid = Encoding.Unicode.GetBytes(this.DstSiteUid);
        byte[] dstSitePath = Encoding.Unicode.GetBytes(this.DstSitePath);
        byte[] dstSiteFname = Encoding.Unicode.GetBytes(this.DstSiteFname);

        byte[] itemSize = Encoding.Unicode.GetBytes(this.ItemSize);
        byte[] unknownIdx4 = Encoding.Unicode.GetBytes(this.UnknownIndex4);

        byte[] advancedParams = new byte[] { };
        if (this.AdvancedParams != null)
            advancedParams = Encoding.Unicode.GetBytes(this.AdvancedParams.ToString());

        byte[] remark = Encoding.Unicode.GetBytes(this.Remark);

        byte[] advancedFolderInclude = Encoding.Unicode.GetBytes(this.AdvancedFolderInclude);
        byte[] advancedFolderExclude = Encoding.Unicode.GetBytes(this.AdvancedFolderExclude);
        byte[] advancedFileInclude = Encoding.Unicode.GetBytes(this.AdvancedFileInclude);
        byte[] advancedFileExclude = Encoding.Unicode.GetBytes(this.AdvancedFileExclude);

        int arrayLength = 0;
        arrayLength += fileType.Length;
        arrayLength += ParameterSeparator.Length;
        arrayLength += transferType.Length;
        arrayLength += ParameterSeparator.Length;

        arrayLength += srcSiteUid.Length;
        arrayLength += ParameterSeparator.Length;
        arrayLength += srcSitePath.Length;
        arrayLength += ParameterSeparator.Length;
        arrayLength += srcSiteFname.Length;
        arrayLength += ParameterSeparator.Length;

        arrayLength += dstSiteUid.Length;
        arrayLength += ParameterSeparator.Length;
        arrayLength += dstSitePath.Length;
        arrayLength += ParameterSeparator.Length;
        arrayLength += dstSiteFname.Length;
        arrayLength += ParameterSeparator.Length;

        arrayLength += itemSize.Length;
        arrayLength += ParameterSeparator.Length;
        arrayLength += unknownIdx4.Length;
        arrayLength += ParameterSeparator.Length;

        arrayLength += advancedParams.Length;
        arrayLength += ParameterSeparator.Length;

        arrayLength += remark.Length;
        arrayLength += ParameterSeparator.Length;


        arrayLength += advancedFolderInclude.Length;
        arrayLength += ParameterSeparator.Length;
        arrayLength += advancedFolderExclude.Length;
        arrayLength += ParameterSeparator.Length;
        arrayLength += advancedFileInclude.Length;
        arrayLength += ParameterSeparator.Length;
        arrayLength += advancedFileExclude.Length;
        arrayLength += ParameterSeparator.Length;
        arrayLength += ParameterTerminator.Length;

        int idx = 0;
        byte[] line = new byte[arrayLength];
        System.Buffer.BlockCopy(fileType, 0, line, idx, fileType.Length);
        idx += fileType.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;

        System.Buffer.BlockCopy(transferType, 0, line, idx, transferType.Length);
        idx += transferType.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;

        System.Buffer.BlockCopy(srcSiteUid, 0, line, idx, srcSiteUid.Length);
        idx += srcSiteUid.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;
        System.Buffer.BlockCopy(srcSitePath, 0, line, idx, srcSitePath.Length);
        idx += srcSitePath.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;
        System.Buffer.BlockCopy(srcSiteFname, 0, line, idx, srcSiteFname.Length);
        idx += srcSiteFname.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;

        System.Buffer.BlockCopy(dstSiteUid, 0, line, idx, dstSiteUid.Length);
        idx += dstSiteUid.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;
        System.Buffer.BlockCopy(dstSitePath, 0, line, idx, dstSitePath.Length);
        idx += dstSitePath.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;
        System.Buffer.BlockCopy(dstSiteFname, 0, line, idx, dstSiteFname.Length);
        idx += dstSiteFname.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;

        System.Buffer.BlockCopy(itemSize, 0, line, idx, itemSize.Length);
        idx += itemSize.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;
        System.Buffer.BlockCopy(unknownIdx4, 0, line, idx, unknownIdx4.Length);
        idx += unknownIdx4.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;

        System.Buffer.BlockCopy(advancedParams, 0, line, idx, advancedParams.Length);
        idx += advancedParams.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;

        System.Buffer.BlockCopy(remark, 0, line, idx, remark.Length);
        idx += remark.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;


        System.Buffer.BlockCopy(advancedFolderInclude, 0, line, idx, advancedFolderInclude.Length);
        idx += advancedFolderInclude.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;
        System.Buffer.BlockCopy(advancedFolderExclude, 0, line, idx, advancedFolderExclude.Length);
        idx += advancedFolderExclude.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;
        System.Buffer.BlockCopy(advancedFileInclude, 0, line, idx, advancedFileInclude.Length);
        idx += advancedFileInclude.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;
        System.Buffer.BlockCopy(advancedFileExclude, 0, line, idx, advancedFileExclude.Length);
        idx += advancedFileExclude.Length;
        System.Buffer.BlockCopy(ParameterSeparator, 0, line, idx, ParameterSeparator.Length);
        idx += ParameterSeparator.Length;
        System.Buffer.BlockCopy(ParameterTerminator, 0, line, idx, ParameterTerminator.Length);
        idx += ParameterTerminator.Length;

        return line;
    }

    /// <summary>
    /// Horrible byte-scanner used to identify the next parameter separator for <see cref="QueueItem"/>'s deserializer 
    /// </summary>
    /// <param name="src"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    private static int SearchForBytePattern(byte[] src, byte[] pattern)
    {
        int maxFirstCharSlot = src.Length - pattern.Length + 1;
        for (int i = 0; i < maxFirstCharSlot; i++)
        {
            if (src[i] != pattern[0]) // compare only first byte
                continue;

            // found a match on first byte, now try to match rest of the pattern
            for (int j = pattern.Length - 1; j >= 1; j--)
            {
                if (src[i + j] != pattern[j]) break;
                if (j == 1) return i;
            }
        }

        return -1;
    }
}