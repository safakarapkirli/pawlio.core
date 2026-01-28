using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

public static class Extensions
{
    public static string? Digit(this int @this, int digit, char val)
    {
        var v = @this.ToString();
        while (v.Length < digit) v = val + v;
        return v;
    }

    public static string? ToMoney(this object @this)
    {
        return @this?.ToString()?.Replace(",", ".");
    }

    public static string? Left(this string? @this, int count)
    {
        if (@this == null) return null;
        if (@this.Length <= count) return @this;
        return @this.Substring(0, count);
    }

    public static int ToInt(this object @this)
    {
        try { return Convert.ToInt32(@this); }
        catch { return 0; }
    }

    public static decimal ToDecimal(this object @this)
    {
        try { return Convert.ToDecimal(@this); }
        catch { return 0; }
    }

    public static decimal ToDecimal(this object @this, NumberFormatInfo format)
    {
        try
        {
            if (format == null)
                format = new NumberFormatInfo { CurrencyDecimalDigits = 2, CurrencyDecimalSeparator = "." };

            return Convert.ToDecimal(@this, format);
        }
        catch { return 0; }
    }

    public static T? JsonTo<T>(this string @this)
    {
        return JsonConvert.DeserializeObject<T>(@this);
    }

    static DefaultContractResolver snakeCaseContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    };

    public static string ToJson(this object @this, bool formatting = false, bool nullValues = false)
    {
        return JsonConvert.SerializeObject(@this,
            formatting ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = nullValues ? NullValueHandling.Include : NullValueHandling.Ignore,
                    ContractResolver = snakeCaseContractResolver,
                }
            );
    }

    public static dynamic? ToDynamic(this string @this)
    {
        return JsonConvert.DeserializeObject<dynamic>(@this);
    }

    public static dynamic? ToDynamic(this object @this)
    {
        return JsonConvert.DeserializeObject<dynamic>(@this.ToString()!);
    }

    public static string ToXml(this object @this)
    {
        var stringwriter = new System.IO.StringWriter();
        var serializer = new XmlSerializer(@this.GetType());
        serializer.Serialize(stringwriter, @this);
        return stringwriter.ToString();
    }

    public static T? XmlTo<T>(this string @this)
    {
        var stringReader = new System.IO.StringReader(@this);
        var serializer = new XmlSerializer(typeof(T));
        return (T?)(serializer.Deserialize(stringReader));
    }

    public static string ToMd5(this string @this)
    {
        if (@this == null) return "";
        using (var md5 = MD5.Create())
        {
            var result = md5.ComputeHash(Encoding.ASCII.GetBytes(@this));
            return Encoding.ASCII.GetString(result);
        }
    }

    public static string ByteArrayToHexString(byte[] inputArray)
    {
        if (inputArray == null) return "";
        var o = new StringBuilder("");
        for (var i = 0; i < inputArray.Length; i++)
            o.Append(inputArray[i].ToString("X2"));
        return o.ToString();
    }
}
