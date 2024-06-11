using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RuSharpX.Queue;

public class QueueFile
{
    private static readonly byte[] BomMagic = new byte[2] { 0xFF, 0xFE };

    /// <summary>
    /// Contains a <see cref="List{T}"/>. Add <see cref="QueueItem">QueueItems</see> you want in this
    /// <see cref="QueueFile"/> to this list.
    /// </summary>
    public List<QueueItem> Items = new List<QueueItem>();

    /// <summary>
    /// Creates a new <see cref="QueueFile"/> from defaults.
    /// </summary>
    QueueFile()
    {
    }

    /// <summary>
    /// Creates a <see cref="QueueFile"/> from an existing file.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    QueueFile(string filePath)
    {
        StreamReader sr = new StreamReader(File.OpenRead(filePath));
        Decode(Encoding.Unicode.GetBytes(sr.ReadToEnd()));
        sr.Close();
    }

    /// <summary>
    /// Creates a <see cref="QueueFile"/> from an existing file.
    /// </summary>
    /// <param name="fileBytes">The file's byte-content.</param>
    QueueFile(byte[] fileBytes)
    {
        Decode(fileBytes);
    }

    private void Decode(byte[] fileBytes)
    {
        // This is horrible, realistically we'd want to just process the entire thing above,
        // but that's effort.
        
        string fileContent = System.Text.Encoding.UTF8.GetString(fileBytes);
        string[] lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        foreach (string line in lines)
        {
            byte[] lineBytes = Encoding.Unicode.GetBytes(line);
            Items.Add(new QueueItem(lineBytes));
        }
    }
    
    /// <summary>
    /// Encodes the Items into a byte array ready to be saved to a file.
    /// </summary>
    /// <returns>A byte array of the encoded <see cref="QueueFile"/></returns>
    public byte[] Encode()
    {
        MemoryStream ms = new MemoryStream();
        ms.Write(BomMagic, 0, BomMagic.Length);

        foreach (QueueItem item in this.Items)
        {
            byte[] encodedBytes = item.Encode();
            ms.Write(encodedBytes, 0, encodedBytes.Length);
        }


        return ms.ToArray();
    }

    /// <summary>
    /// Encodes and saves the Items into a file.
    /// </summary>
    /// <param name="filePath">The path of the file to save to.</param>
    public void EncodeToFile(string filePath)
    {
        File.WriteAllBytes(filePath, this.Encode());
    }
}