using System;
using System.Linq;
using System.Text.RegularExpressions;

public static class UUIDv9
{
    private static readonly Regex UuidRegex = new Regex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", RegexOptions.Compiled);
    private static readonly Regex Base16Regex = new Regex("^[0-9a-fA-F]+$", RegexOptions.Compiled);

    private static string CalcChecksum(string hexString) // CRC-8
    {
        var data = Enumerable.Range(0, hexString.Length / 2)
            .Select(i => Convert.ToByte(hexString.Substring(i * 2, 2), 16))
            .ToArray();

        const byte polynomial = 0x07;
        byte crc = 0x00;

        foreach (var byteValue in data)
        {
            crc ^= byteValue;
            for (int i = 0; i < 8; i++)
            {
                crc = (crc & 0x80) != 0 ? (byte)((crc << 1) ^ polynomial) : (byte)(crc << 1);
            }
        }

        return crc.ToString("x2");
    }

    public static bool VerifyChecksum(string uuid)
    {
        var base16String = uuid.Replace("-", "").Substring(0, 30);
        var crc = CalcChecksum(base16String);
        return crc == uuid.Substring(34, 2);
    }

    public static bool CheckVersion(string uuid, int? version = null)
    {
        var versionDigit = uuid[14].ToString();
        var variantDigit = uuid[19].ToString();
        return (!version.HasValue || versionDigit == version.ToString()) &&
               (versionDigit == "9" || ("14".Contains(versionDigit) && "89abAB".Contains(variantDigit)));
    }

    public static bool IsUUID(string uuid) => !string.IsNullOrEmpty(uuid) && UuidRegex.IsMatch(uuid);

    public class ValidateUUIDv9Options
    {
        public bool? Checksum { get; set; }
        public bool? Version { get; set; }
    }

    public static bool IsValidUUIDv9(string uuid, ValidateUUIDv9Options options)
    {
        return IsUUID(uuid) &&
               (!options?.Checksum == true || VerifyChecksum(uuid)) &&
               (!options?.Version == true || CheckVersion(uuid));
    }

    private static string RandomBytes(int count)
    {
        var random = new Random();
        return string.Concat(Enumerable.Range(0, count).Select(_ => random.Next(16).ToString("x")));
    }

    private static char RandomChar(string chars)
    {
        var random = new Random();
        return chars[random.Next(chars.Length)];
    }

    private static bool IsBase16(string str) => Base16Regex.IsMatch(str);

    private static void ValidatePrefix(string prefix)
    {
        if (prefix == null) throw new ArgumentException("Prefix must be a string");
        if (prefix.Length > 8) throw new ArgumentException("Prefix must be no more than 8 characters");
        if (!IsBase16(prefix)) throw new ArgumentException("Prefix must be only hexadecimal characters");
    }

    private static string AddDashes(string str)
    {
        return $"{str.Substring(0, 8)}-{str.Substring(8, 4)}-{str.Substring(12, 4)}-{str.Substring(16, 4)}-{str.Substring(20)}";
    }

    public class UUIDv9Options
    {
        public string Prefix { get; set; } = string.Empty;
        public object Timestamp { get; set; } = true; // can be bool, number, string, or DateTime
        public bool Checksum { get; set; }
        public bool Version { get; set; }
        public bool Legacy { get; set; }
    }

    private static T OptionOrDefault<T>(string name, UUIDv9Options options)
    {
        return options != null && options.GetType().GetProperty(name)?.GetValue(options) != null
            ? (T)options.GetType().GetProperty(name).GetValue(options)
            : (T)typeof(UUIDv9Options).GetProperty(name).GetValue(new UUIDv9Options());
    }

    public static string Generate(UUIDv9Options options = null)
    {
        options ??= new UUIDv9Options();
        var prefix = OptionOrDefault<string>("Prefix", options).ToLower();
        var timestamp = OptionOrDefault<object>("Timestamp", options);
        var checksum = OptionOrDefault<bool>("Checksum", options);
        var version = OptionOrDefault<bool>("Version", options);
        var legacy = OptionOrDefault<bool>("Legacy", options);

        if (!string.IsNullOrEmpty(prefix))
        {
            ValidatePrefix(prefix);
        }

        string center = timestamp switch
        {
            DateTime dt => dt.Ticks.ToString("x"),
            long num => num.ToString("x"),
            string str => new DateTime(Convert.ToInt64(str)).Ticks.ToString("x"),
            _ => DateTime.Now.Ticks.ToString("x")
        };

        string suffix = RandomBytes(32 - prefix.Length - center.Length - (checksum ? 2 : 0) - (legacy ? 2 : version ? 1 : 0));
        string joined = prefix + center + suffix;

        if (legacy)
        {
            joined = joined.Substring(0, 12) + (timestamp != null ? '1' : '4') + joined.Substring(12, 3) + RandomChar("89ab") + joined.Substring(15);
        }
        else if (version)
        {
            joined = joined.Substring(0, 12) + '9' + joined.Substring(12);
        }

        if (checksum)
        {
            joined += CalcChecksum(joined);
        }

        return AddDashes(joined);
    }
}
