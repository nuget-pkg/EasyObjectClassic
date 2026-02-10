// ReSharper disable once CheckNamespace
namespace Global;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public enum EasyObjectClassicType
{
    // ReSharper disable once InconsistentNaming
    @string,
    // ReSharper disable once InconsistentNaming
    @number,
    // ReSharper disable once InconsistentNaming
    @boolean,
    // ReSharper disable once InconsistentNaming
    @object,
    // ReSharper disable once InconsistentNaming
    @array,
    // ReSharper disable once InconsistentNaming
    @null
}

internal class EasyObjectClassicConverter : IConvertParsedResult
{
    public object? ConvertParsedResult(object? x, string origTypeName)
    {
        if (x is Dictionary<string, object>)
        {
            var dict = x as Dictionary<string, object>;
            var keys = dict!.Keys;
            var result = new Dictionary<string, EasyObjectClassic>();
            foreach (var key in keys)
            {
                var eo = new EasyObjectClassic();
                eo.RealData = dict[key];
                result[key] = eo;
            }
            return result;
        }
        else if (x is List<object>)
        {
            var list = x as List<object>;
            var result = new List<EasyObjectClassic>();
            foreach (var e in list!)
            {
                var eo = new EasyObjectClassic();
                eo.RealData = e;
                result.Add(eo);
            }
            return result;
        }
        return x;
    }
}

public class EasyObjectClassic :
    DynamicObject,
    IExposeInternalObject,
    IExportToPlainObject,
    IImportFromPlainObject,
    IExportToCommonJson,
    IImportFromCommonJson
{
    public object? RealData /*= null*/;
    //public static readonly bool IsConsoleApplication = HasConsole();

    // ReSharper disable once MemberCanBePrivate.Global
    public static readonly IParseJson DefaultJsonParser = new CSharpEasyLanguageHandler(numberAsDecimal: true);
    public static IParseJson? JsonParser /*= null*/;
    // ReSharper disable once MemberCanBePrivate.Global
    public static bool DebugOutput /*= false*/;
    public static bool ShowDetail /*= false*/;
    // ReSharper disable once MemberCanBePrivate.Global
    public static bool ForceAscii /*= false*/;

    public static void ClearSettings()
    {
        EasyObjectClassic.JsonParser = DefaultJsonParser;
        EasyObjectClassic.DebugOutput = false;
        EasyObjectClassic.ShowDetail = false;
        EasyObjectClassic.ForceAscii = false;
    }

    static EasyObjectClassic()
    {
        EasyObjectClassic.ClearSettings();
    }

    public EasyObjectClassic()
    {
        this.RealData = null;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public EasyObjectClassic(object? x)
    {
        this.RealData = new PlainObjectConverter(jsonParser: JsonParser, forceAscii: false, iConvertParsedResult: new EasyObjectClassicConverter()).Parse(x, numberAsDecimal: true);
    }

    public dynamic Dynamic { get { return this; } }

    public override string ToString()
    {
        return this.ToPrintable();
    }

    //public object? ToPlainObject()
    //{
    //    return this.ToObject();
    //}

    public string ToPrintable(bool noIndent = false)
    {
        return EasyObjectClassic.ToPrintable(this, noIndent: noIndent);
    }

    public static EasyObjectClassic Null { get { return new EasyObjectClassic(); } }
    public static EasyObjectClassic EmptyArray { get { return new EasyObjectClassic(new List<EasyObjectClassic>()); } }
    public static EasyObjectClassic EmptyObject { get { return new EasyObjectClassic(new Dictionary<string, EasyObjectClassic>()); } }

    public static EasyObjectClassic NewArray(params object[] args)
    {
        EasyObjectClassic result = EmptyArray;
        for (int i = 0; i < args.Length; i++)
        {
            result.Add(FromObject(args[i]));
        }
        return result;
    }
    public static EasyObjectClassic NewObject(params object[] args)
    {
        if ((args.Length % 2) != 0) throw new ArgumentException("EasyObjectClassic.NewObject() requires even number arguments");
        EasyObjectClassic result = EmptyObject;
        for (int i = 0; i < args.Length; i += 2)
        {
            result.Add(args[i].ToString()!, FromObject(args[i + 1]));
        }
        return result;
    }

    // ReSharper disable once InconsistentNaming
    public static EasyObjectClassicType @string { get { return EasyObjectClassicType.@string; } }
    // ReSharper disable once InconsistentNaming
    public static EasyObjectClassicType @boolean { get { return EasyObjectClassicType.@boolean; } }
    // ReSharper disable once InconsistentNaming
    public static EasyObjectClassicType @object { get { return EasyObjectClassicType.@object; } }
    // ReSharper disable once InconsistentNaming
    public static EasyObjectClassicType @array { get { return EasyObjectClassicType.@array; } }
    // ReSharper disable once InconsistentNaming
    public static EasyObjectClassicType @null { get { return EasyObjectClassicType.@null; } }

    public bool IsString { get { return this.TypeValue == EasyObjectClassicType.@string; } }
    public bool IsNumber { get { return this.TypeValue == EasyObjectClassicType.@number; } }
    public bool IsBoolean { get { return this.TypeValue == EasyObjectClassicType.@boolean; } }
    public bool IsObject { get { return this.TypeValue == EasyObjectClassicType.@object; } }
    public bool IsArray { get { return this.TypeValue == EasyObjectClassicType.@array; } }
    public bool IsNull { get { return this.TypeValue == EasyObjectClassicType.@null; } }

    private static object? ExposeInternalObjectHelper(object? x)
    {
        while (x is EasyObjectClassic)
        {
            x = ((EasyObjectClassic)x).RealData;
        }
        return x;
    }

    private static EasyObjectClassic WrapInternal(object? x)
    {
        if (x is EasyObjectClassic) return (x as EasyObjectClassic)!;
        return new EasyObjectClassic(x);
    }

    public object? ExposeInternalObject()
    {
        return EasyObjectClassic.ExposeInternalObjectHelper(this);
    }

    public EasyObjectClassicType TypeValue
    {
        get
        {
            object? obj = ExposeInternalObjectHelper(this);
            if (obj == null) return EasyObjectClassicType.@null;
            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Boolean:
                    return EasyObjectClassicType.@boolean;
                case TypeCode.String:
                case TypeCode.Char:
                case TypeCode.DateTime:
                    return EasyObjectClassicType.@string;
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    return EasyObjectClassicType.@number;
                case TypeCode.Object:
                    return (obj is List<EasyObjectClassic>) ? EasyObjectClassicType.@array : EasyObjectClassicType.@object;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                default:
                    if (obj is TimeSpan || obj is Guid) return EasyObjectClassic.@string;
                    return EasyObjectClassicType.@null;
            }
        }
    }

    public string TypeName
    {
        get
        {
            return this.TypeValue.ToString();
        }
    }

    // ReSharper disable once InconsistentNaming
    internal List<EasyObjectClassic>? list
    {
        get { return RealData as List<EasyObjectClassic>; }
    }

    // ReSharper disable once InconsistentNaming
    internal Dictionary<string, EasyObjectClassic>? dictionary
    {
        get { return RealData as Dictionary<string, EasyObjectClassic>; }
    }

    public int Count
    {
        get
        {
            if (list != null) return list.Count;
            if (dictionary != null) return dictionary.Count;
            return 0;
        }
    }

    public List<string> Keys
    {
        get
        {
            var keys = new List<string>();
            if (dictionary == null) return keys;
            foreach (var key in dictionary.Keys) keys.Add(key);
            return keys;
        }
    }

    public bool ContainsKey(string name)
    {
        if (dictionary == null) return false;
        return dictionary.ContainsKey(name);
    }

    public EasyObjectClassic Add(object x)
    {
        if (list == null) RealData = new List<EasyObjectClassic>();
        EasyObjectClassic eo = x is EasyObjectClassic ? (x as EasyObjectClassic)! : new EasyObjectClassic(x);
        list!.Add(eo);
        return this;
    }

    public EasyObjectClassic Add(string key, object? x)
    {
        if (dictionary == null) RealData = new Dictionary<string, EasyObjectClassic>();
        EasyObjectClassic eo = x is EasyObjectClassic ? (x as EasyObjectClassic)! : new EasyObjectClassic(x);
        dictionary!.Add(key, eo);
        return this;
    }

    public override bool TryGetMember(
        GetMemberBinder binder, out object result)
    {
        result = Null;
        string name = binder.Name;
        if (list != null)
        {
            var assoc = TryAssoc(name);
            result = assoc;
        }
        if (dictionary == null) return true;
        EasyObjectClassic? eo;
        dictionary.TryGetValue(name, out eo);
        if (eo == null) eo = Null;
        result = eo;
        return true;
    }

    public override bool TrySetMember(
        SetMemberBinder binder, object? value)
    {
        value = ExposeInternalObjectHelper(value);
        if (dictionary == null)
        {
            RealData = new Dictionary<string, EasyObjectClassic>();
        }
        string name = binder.Name;
        dictionary![name] = WrapInternal(value);
        return true;
    }
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        result = Null;
        var idx = indexes[0];
        if (idx is int)
        {
            int pos = (int)indexes[0];
            if (list == null)
            {
                result = WrapInternal(null);
                return true;
            }
            if (list.Count < (pos + 1))
            {
                result = WrapInternal(null);
                return true;
            }
            result = WrapInternal(list[pos]);
            return true;
        }
        if (list != null)
        {
            var assoc = TryAssoc((string)idx);
            result = assoc;
        }
        if (dictionary == null)
        {
            result = Null;
            return true;
        }
        EasyObjectClassic? eo /*= Null*/;
        dictionary.TryGetValue((string)idx, out eo);
        if (eo == null) eo = Null;
        result = eo;
        return true;
    }

    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value)
    {
        if (value is EasyObjectClassic) value = ((EasyObjectClassic)value).RealData;
        var idx = indexes[0];
        if (idx is int)
        {
            int pos = (int)indexes[0];
            if (pos < 0) throw new ArgumentException("index is below 0");
            if (list == null)
            {
                RealData = new List<EasyObjectClassic>();
            }
            while (list!.Count < (pos + 1))
            {
                list.Add(Null);
            }
            list[pos] = WrapInternal(value);
            return true;
        }
        if (dictionary == null)
        {
            RealData = new Dictionary<string, EasyObjectClassic>();
        }
        string name = (string)indexes[0];
        dictionary![name] = WrapInternal(value);
        return true;
    }

    public override bool TryConvert(ConvertBinder binder, out object? result)
    {
        if (binder.Type == typeof(IEnumerable))
        {
            if (list != null)
            {
                var ie1 = list.Select(x => x);
                result = ie1;
                return true;
            }
            if (dictionary != null)
            {
                var ie2 = dictionary.Select(x => x);
                result = ie2;
                return true;
            }
            result = (new List<EasyObjectClassic>()).Select(x => x);
            return true;
        }
        else
        {
            result = Convert.ChangeType(RealData, binder.Type);
            return true;
        }
    }

    private static string[] TextToLines(string text)
    {
        List<string> lines = new List<string>();
        using (StringReader sr = new StringReader(text))
        {
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }
        return lines.ToArray();
    }
    public static EasyObjectClassic FromObject(object? obj, bool ignoreErrors = false)
    {
        if (!ignoreErrors)
        {
            return new EasyObjectClassic(obj);
        }
        try
        {
            return new EasyObjectClassic(obj);
        }
        catch (Exception)
        {
            return new EasyObjectClassic(null);
        }
    }

    public static EasyObjectClassic FromJson(string? json, bool ignoreErrors = false)
    {
        if (json == null) return Null;
        if (json.StartsWith("#!"))
        {
            string[] lines = TextToLines(json);
            lines = lines.Skip(1).ToArray();
            json = String.Join("\n", lines);
        }
        if (!ignoreErrors)
        {
            return new EasyObjectClassic(JsonParser!.ParseJson(json));
        }
        try
        {
            return new EasyObjectClassic(JsonParser!.ParseJson(json));
        }
        catch (Exception)
        {
            return new EasyObjectClassic(null);
        }
    }

    public dynamic? ToObject(bool asDynamicObject = false)
    {
        //return new PlainObjectConverter(jsonParser: null, forceAscii: ForceAscii).Parse(RealData);
        if (asDynamicObject)
        {
            return this.ExportToDynamicObject();
        }
        else
        {
            return this.ExportToPlainObject();
        }
    }

    public string ToJson(bool indent = false, bool sortKeys = false)
    {
        PlainObjectConverter poc = new PlainObjectConverter(jsonParser: JsonParser, forceAscii: ForceAscii);
        return poc.Stringify(RealData, indent, sortKeys);
    }

#if USE_WINCONSOLE
    public static bool HasConsole()
    {
        try
        {
            // Attempt to get a console property
            int left = Console.CursorLeft;
            return true;
        }
        catch (IOException)
        {
            // If an exception is caught, no console is available
            return false;
        }
    }
    // ReSharper disable once MemberCanBePrivate.Global
    public static void AllocConsole()
    {
        if (IsConsoleApplication) return;
        WinConsole.Alloc();
    }
    // ReSharper disable once MemberCanBePrivate.Global
    public static void FreeConsole()
    {
        if (IsConsoleApplication) return;
        WinConsole.Free();
    }
    public static void ReallocConsole()
    {
        if (IsConsoleApplication) return;
        FreeConsole();
        AllocConsole();
    }
#endif

    public static string ToPrintable(object? x, string? title = null, bool noIndent = false)
    {
        PlainObjectConverter poc = new PlainObjectConverter(jsonParser: JsonParser, forceAscii: ForceAscii);
        return poc.ToPrintable(ShowDetail, x, title, noIndent: noIndent);
    }

    public static void Echo(
        object? x,
        string? title = null,
        bool noIndent = false,
        uint maxDepth = 0,
        List<string>? hideKeys = null
        )
    {
        hideKeys ??= new List<string>();
        if (maxDepth > 0 || hideKeys.Count > 0)
        {
            var eo = FromObject(x);
            x = eo.Clone(
                maxDepth: maxDepth,
                hideKeys: hideKeys,
                always: false);
        }
        string s = ToPrintable(x, title, noIndent: noIndent);
        Console.WriteLine(s);
        System.Diagnostics.Debug.WriteLine(s);
    }
    public static void Log(
        object? x,
        string? title = null,
        bool noIndent = false,
        uint maxDepth = 0,
        List<string>? hideKeys = null
        )
    {
        hideKeys ??= new List<string>();
        if (maxDepth > 0 || hideKeys.Count > 0)
        {
            var eo = FromObject(x);
            x = eo.Clone(
                maxDepth: maxDepth,
                hideKeys: hideKeys,
                always: false);
        }
        string s = ToPrintable(x, title, noIndent: noIndent);
        Console.Error.WriteLine("[Log] " + s);
        System.Diagnostics.Debug.WriteLine("[Log] " + s);
    }
    public static void Debug(
        object? x,
        string? title = null,
        bool noIndent = false,
        uint maxDepth = 0,
        List<string>? hideKeys = null
        )
    {
        if (!DebugOutput) return;
        hideKeys ??= new List<string>();
        if (maxDepth > 0 || hideKeys.Count > 0)
        {
            var eo = FromObject(x);
            x = eo.Clone(
                maxDepth: maxDepth,
                hideKeys: hideKeys,
                always: false);
        }
        string s = ToPrintable(x, title, noIndent: noIndent);
        Console.Error.WriteLine("[Debug] " + s);
        System.Diagnostics.Debug.WriteLine("[Debug] " + s);
    }
    public static void Message(
        object? x,
        string? title = null,
        bool noIndent = false,
        uint maxDepth = 0,
        List<string>? hideKeys = null
        )
    {
        if (title == null) title = "Message";
        string s = ToPrintable(x, title: title, noIndent: noIndent);
        NativeMethods.MessageBoxW(IntPtr.Zero, s, title, 0);
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int MessageBoxW(
            IntPtr hWnd, string lpText, string lpCaption, uint uType);
    }
    private EasyObjectClassic TryAssoc(string name)
    {
        try
        {
            if (list == null) return Null;
            for (int i = 0; i < list.Count; i++)
            {
                var pair = list[i].AsList!;
                if (pair[0].Cast<string>() == name)
                {
                    return pair[1];
                }
            }
            return Null;
        }
        catch (Exception /*e*/)
        {
            return Null;
        }
    }
    public EasyObjectClassic this[string name]
    {
        get
        {
            if (list != null)
            {
                return TryAssoc(name);
            }
            if (dictionary == null) return Null;
            EasyObjectClassic? eo;
            dictionary.TryGetValue(name, out eo);
            if (eo == null) return Null;
            return eo;
        }
        set
        {
            if (dictionary == null)
            {
                RealData = new Dictionary<string, EasyObjectClassic>();
            }
            dictionary![name] = value;
        }
    }
    public EasyObjectClassic this[int pos]
    {
        get
        {
            if (list == null)
            {
                return WrapInternal(null);
            }
            if (list.Count < (pos + 1))
            {
                return WrapInternal(null);
            }
            return WrapInternal(list[pos]);
        }
        set
        {
            if (pos < 0) throw new ArgumentException("index below 0");
            if (list == null)
            {
                RealData = new List<EasyObjectClassic>();
            }
            while (list!.Count < (pos + 1))
            {
                list.Add(Null);
            }
            list[pos] = value;
        }
    }
    public T Cast<T>()
    {
        if (this.RealData is DateTime dt)
        {
            string? s = null;
            switch (dt.Kind)
            {
                case DateTimeKind.Local:
                    s = dt.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
                    break;
                case DateTimeKind.Utc:
                    s = dt.ToString("o");
                    break;
                case DateTimeKind.Unspecified:
                    s = dt.ToString("o").Replace("Z", "");
                    break;
            }
            return (T)Convert.ChangeType(s, typeof(T))!;
        }
        return (T)Convert.ChangeType(this.RealData, typeof(T))!;
    }
    public List<EasyObjectClassic>? AsList
    {
        get
        {
            // ReSharper disable once ArrangeAccessorOwnerBody
            return list;
        }
    }
    public Dictionary<string, EasyObjectClassic>? AsDictionary
    {
        get
        {
            // ReSharper disable once ArrangeAccessorOwnerBody
            return dictionary;
        }
    }

    public static string FullName(dynamic? x)
    {
        if (x is null) return "null";
        string fullName = ((object)x).GetType().FullName!;
        if (fullName.StartsWith("<>f__AnonymousType"))
        {
            return "AnonymousType";
        }
        return fullName!.Split('`')[0];
    }

    public static implicit operator EasyObjectClassic(bool x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(string x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(char x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(short x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(int x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(long x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(ushort x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(uint x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(ulong x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(float x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(double x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(decimal x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(sbyte x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(byte x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(DateTime x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(TimeSpan x) { return new EasyObjectClassic(x); }
    public static implicit operator EasyObjectClassic(Guid x) { return new EasyObjectClassic(x); }

    public void Nullify()
    {
        this.RealData = null;
    }

    public void Trim(
            uint maxDepth = 0,
            List<string>? hideKeys = null
        )
    {
        EasyObjectClassicEditor.Trim( this, maxDepth, hideKeys );
    }

    public EasyObjectClassic Clone(
        uint maxDepth = 0,
        List<string>? hideKeys = null,
        bool always = true
        )
    {
        return EasyObjectClassicEditor.Clone(this, maxDepth, hideKeys, always);
    }

    public EasyObjectClassic? Shift()
    {
        if (this.list == null) return null;
        if (this.list.Count == 0) return null;
        EasyObjectClassic result = this.list[0];
        this.list.RemoveAt(0);
        return result;
    }

    public void ImportFromPlainObject(object? x)
    {
        var eo = FromObject(x);
        this.RealData = eo.RealData;
    }

    public void ImportFromCommonJson(string x)
    {
        var eo = FromJson(x);
        if (eo == null)
        {
            eo = Null;
        }
        this.RealData = eo!.RealData;
    }

    public string ExportToCommonJson()
    {
        return this.ToJson(
            indent: true,
            sortKeys: false
            );
    }
    public object? ExportToPlainObject()
    {
        return new PlainObjectConverter(jsonParser: null, forceAscii: ForceAscii).Parse(RealData);
    }
    public dynamic? ExportToDynamicObject()
    {
        return EasyObjectClassicEditor.ExportToExpandoObject(this);
    }
    public static string ObjectToJson(object? x, bool indent = false)
    {
        return FromObject(x).ToJson(indent: indent); ;
    }
    public static object? ObjectToObject(object? x, bool asDynamicObject = false)
    {
        return FromObject(x).ToObject(asDynamicObject: asDynamicObject);
    }
}
