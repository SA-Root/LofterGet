using System.Text;
using System.Text.RegularExpressions;

namespace LofterGet;

internal partial class ChromiumVersionDetector
{
    public static string DetectVersion(string filePath)
    {
        var asciiString = "Chrome/";
        byte[] pattern = Encoding.ASCII.GetBytes(asciiString);
        int patternLength = pattern.Length;

        const int chunkSize = 1024 * 1024; // 1 MB
        byte[] buffer = new byte[chunkSize + 1024]; // extra space for overlap

        long fileOffset = 0;
        int bytesRead;

        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
        byte[] overlap = new byte[patternLength - 1];

        while ((bytesRead = fs.Read(buffer, 0, chunkSize)) > 0)
        {
            // Copy overlap from previous chunk
            if (fileOffset > 0)
                Buffer.BlockCopy(overlap, 0, buffer, 0, overlap.Length);

            int searchStart = (fileOffset == 0) ? 0 : overlap.Length;
            int totalBytes = searchStart + bytesRead;

            // Search inside buffer
            for (int i = searchStart; i <= totalBytes - patternLength; i++)
            {
                if (IsMatch(buffer, i, pattern))
                {
                    long matchOffset = fileOffset + (i - searchStart);
                    var ver = Encoding.ASCII.GetString(buffer.AsSpan(i + patternLength, 14)).Trim();
                    if (RegexChromiumVersion().IsMatch(ver))
                    {
                        return ver;
                    }
                }
            }

            // Save new overlap
            Buffer.BlockCopy(buffer, totalBytes - overlap.Length, overlap, 0, overlap.Length);

            fileOffset += bytesRead;
        }

        return string.Empty;
    }

    private static bool IsMatch(byte[] buffer, int position, byte[] pattern)
    {
        for (int i = 0; i < pattern.Length; i++)
        {
            if (buffer[position + i] != pattern[i])
                return false;
        }
        return true;
    }

    [GeneratedRegex(@"\d{1,3}\.\d\.\d{1,4}\.\d{1,3}")]
    private static partial Regex RegexChromiumVersion();
}
