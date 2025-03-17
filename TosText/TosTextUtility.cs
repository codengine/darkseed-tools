using System.Text;

namespace DarkSeedTools;

public static class TosTextUtility
{
    // Reads a 16-bit unsigned integer in little-endian format
    public static ushort ReadUInt16LE(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(2);
        return (ushort)(bytes[0] | (bytes[1] << 8));
    }

    // Writes a 16-bit unsigned integer in little-endian format
    public static void WriteUInt16LE(BinaryWriter writer, ushort value)
    {
        writer.Write((byte)(value & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
    }

    // Reads a string from the binary file, handling multi-byte characters if specified
    public static string ReadString(BinaryReader reader, int length, bool useMultibyte)
    {
        var str = new StringBuilder();
        var initialPosition = reader.BaseStream.Position;
        var bytesAvailable = reader.BaseStream.Length - initialPosition;
            
        if (length > bytesAvailable)
        {
            Console.WriteLine($"Warning: String length ({length}) exceeds available bytes ({bytesAvailable})");
            length = (int)bytesAvailable;
        }
            
        if (useMultibyte)
        {
            for (var i = 0; i < length; i++)
            {
                if (reader.BaseStream.Position >= reader.BaseStream.Length)
                    break;
                        
                var b = reader.ReadByte();
                if ((b & 0x80) != 0)
                {
                    if (i < length - 1 && reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        var b2 = reader.ReadByte();
                        var c = (char)((b << 8) | b2);
                        str.Append(c);
                        i++;
                    }
                    else
                    {
                        str.Append((char)b);
                    }
                }
                else
                {
                    str.Append((char)b);
                }
            }
        }
        else
        {
            try
            {
                for (var i = 0; i < length; i++)
                {
                    if (reader.BaseStream.Position >= reader.BaseStream.Length)
                        break;
                            
                    str.Append((char)reader.ReadByte());
                }
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("Warning: Reached end of file while reading string");
            }
        }
            
        return str.ToString();
    }

    public static byte[] EncodeString(string text, bool useMultibyte)
    {
        var bytes = new List<byte>();
            
        if (useMultibyte)
        {
            foreach (var c in text)
            {
                if (c > 127)
                {
                    bytes.Add((byte)(((c >> 8) & 0xFF) | 0x80)); // High byte with high bit set
                    bytes.Add((byte)(c & 0xFF)); // Low byte
                }
                else
                {
                    bytes.Add((byte)c);
                }
            }
        }
        else
        {
            bytes.AddRange(text.Select(c => (byte)c));
        }
            
        return bytes.ToArray();
    }
}