using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable once CheckNamespace
namespace Global;
public enum EasyObjectClassicType {
    // ReSharper disable once InconsistentNaming
    @string,
    // ReSharper disable once InconsistentNaming
    number,
    // ReSharper disable once InconsistentNaming
    boolean,
    // ReSharper disable once InconsistentNaming
    @object,
    // ReSharper disable once InconsistentNaming
    array,
    // ReSharper disable once InconsistentNaming
    @null
}
internal class EasyObjectClassicConverter : IConvertParsedResult {
    public object? ConvertParsedResult(object? x, string origTypeName) {
        if (x is Dictionary<string, object>) {
            var dict = x as Dictionary<string, object>;
            var keys = dict!.Keys;
            var result = new Dictionary<string, EasyObjectClassic>();
            foreach (var key in keys) {
                var eo = new EasyObjectClassic();
                eo.RealData = dict[key];
                result[key] = eo;
            }
            return result;
        }
        if (x is List<object>) {
            var list = x as List<object>;
            var result = new List<EasyObjectClassic>();
            foreach (var e in list!) {
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
    IImportFromCommonJson {
    // ReSharper disable once MemberCanBePrivate.Global
    public static readonly IParseJson DefaultJsonParser = new CSharpJsonHandlerClassic(true);
    public static IParseJson? JsonParser /*= null*/;
    // ReSharper disable once MemberCanBePrivate.Global
    public static bool DebugOutput /*= false*/;
    public static bool ShowDetail /*= false*/;
    // ReSharper disable once MemberCanBePrivate.Global
    public static bool ForceAscii /*= false*/;
    public object? RealData /*= null*/;
    static EasyObjectClassic() {
        ClearSettings();
    }
    public EasyObjectClassic() {
        RealData = null;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public EasyObjectClassic(object? x) {
        RealData = new PlainObjectConverterClassic(JsonParser, false, new EasyObjectClassicConverter()).Parse(x, true);
    }
    public dynamic Dynamic => this;
    public static EasyObjectClassic Null => new();
    public static EasyObjectClassic EmptyArray => new(new List<EasyObjectClassic>());
    public static EasyObjectClassic EmptyObject => new(new Dictionary<string, EasyObjectClassic>());

    // ReSharper disable once InconsistentNaming
    public static EasyObjectClassicType @string => EasyObjectClassicType.@string;
    // ReSharper disable once InconsistentNaming
    public static EasyObjectClassicType boolean => EasyObjectClassicType.boolean;
    // ReSharper disable once InconsistentNaming
    public static EasyObjectClassicType @object => EasyObjectClassicType.@object;
    // ReSharper disable once InconsistentNaming
    public static EasyObjectClassicType array => EasyObjectClassicType.array;
    // ReSharper disable once InconsistentNaming
    public static EasyObjectClassicType @null => EasyObjectClassicType.@null;
    public bool IsString => TypeValue == EasyObjectClassicType.@string;
    public bool IsNumber => TypeValue == EasyObjectClassicType.number;
    public bool IsBoolean => TypeValue == EasyObjectClassicType.boolean;
    public bool IsObject => TypeValue == EasyObjectClassicType.@object;
    public bool IsArray => TypeValue == EasyObjectClassicType.array;
    public bool IsNull => TypeValue == EasyObjectClassicType.@null;
    public EasyObjectClassicType TypeValue {
        get {
            var obj = ExposeInternalObjectHelper(this);
            if (obj == null) return EasyObjectClassicType.@null;
            switch (Type.GetTypeCode(obj.GetType())) {
                case TypeCode.Boolean:
                    return EasyObjectClassicType.boolean;
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
                    return EasyObjectClassicType.number;
                case TypeCode.Object:
                    return obj is List<EasyObjectClassic> ? EasyObjectClassicType.array : EasyObjectClassicType.@object;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                default:
                    if (obj is TimeSpan || obj is Guid) return @string;
                    return EasyObjectClassicType.@null;
            }
        }
    }
    public string TypeName => TypeValue.ToString();

    // ReSharper disable once InconsistentNaming
    public List<EasyObjectClassic>? RealList => RealData as List<EasyObjectClassic>;

    // ReSharper disable once InconsistentNaming
    public Dictionary<string, EasyObjectClassic>? RealDictionary => RealData as Dictionary<string, EasyObjectClassic>;
    public int Count {
        get {
            if (RealList != null) return RealList.Count;
            if (RealDictionary != null) return RealDictionary.Count;
            return 0;
        }
    }
    public List<string> Keys {
        get {
            var keys = new List<string>();
            if (RealDictionary == null) return keys;
            foreach (var key in RealDictionary.Keys) keys.Add(key);
            return keys;
        }
    }
    public EasyObjectClassic this[string name] {
        get {
            if (RealList != null) return TryAssoc(name);
            if (RealDictionary == null) return Null;
            EasyObjectClassic? eo;
            RealDictionary.TryGetValue(name, out eo);
            if (eo == null) return Null;
            return eo;
        }
        set {
            if (RealDictionary == null) RealData = new Dictionary<string, EasyObjectClassic>();
            RealDictionary![name] = value;
        }
    }
    public EasyObjectClassic this[int pos] {
        get {
            if (RealList == null) return WrapInternal(null);
            if (RealList.Count < pos + 1) return WrapInternal(null);
            return WrapInternal(RealList[pos]);
        }
        set {
            if (pos < 0) throw new ArgumentException("index below 0");
            if (RealList == null) RealData = new List<EasyObjectClassic>();
            while (RealList!.Count < pos + 1) RealList.Add(Null);
            RealList[pos] = value;
        }
    }
    public List<EasyObjectClassic>? AsList {
        get {
            // ReSharper disable once ArrangeAccessorOwnerBody
            return RealList;
        }
    }
    public Dictionary<string, EasyObjectClassic>? AsDictionary {
        get {
            // ReSharper disable once ArrangeAccessorOwnerBody
            return RealDictionary;
        }
    }
    public string[] AsStringArray {
        get {
            if (RealList != null)
                return
                    RealList!
                        .Select(i =>
                            i.IsString ? i.Cast<string>() : i.ToJson(keyAsSymbol: true, indent: false))
                        .ToArray();
            if (RealDictionary != null) return RealDictionary.Keys.Select(i => i).ToArray();
            return [];
        }
    }
    public List<string> AsStringList => AsStringArray.ToList();
    public string ExportToCommonJson() {
        return ToJson(
            true
        );
    }
    public object? ExportToPlainObject() {
        return new PlainObjectConverterClassic(null, ForceAscii).Parse(RealData);
    }
    public object? ExposeInternalObject() {
        return ExposeInternalObjectHelper(this);
    }
    public void ImportFromCommonJson(string x) {
        var eo = FromJson(x);
        RealData = eo.RealData;
    }
    public void ImportFromPlainObject(object? x) {
        var eo = FromObject(x);
        RealData = eo.RealData;
    }
    public static void ClearSettings() {
        JsonParser = DefaultJsonParser;
        DebugOutput = false;
        ShowDetail = false;
        ForceAscii = false;
    }
    public static void SetupConsoleEncoding(Encoding? encoding = null) {
        if (encoding == null) encoding = Encoding.UTF8;
        try {
            Console.OutputEncoding = encoding;
            Console.InputEncoding = encoding;
            Console.SetError(
                new StreamWriter(
                    Console.OpenStandardError(), encoding) {
                    AutoFlush = true
                });
        }
        catch (Exception) {
            // Ignore exceptions related to console encoding
        }
    }
    public override string ToString() {
        return ToPrintable();
    }
    public string ToPrintable(bool compact = false) {
        return ToPrintable(this, compact: compact);
    }
    public static EasyObjectClassic NewArray(params object[] args) {
        var result = EmptyArray;
        for (var i = 0; i < args.Length; i++) result.Add(FromObject(args[i]));
        return result;
    }
    public static EasyObjectClassic NewObject(params object[] args) {
        if (args.Length % 2 != 0)
            throw new ArgumentException("EasyObjectClassic.NewObject() requires even number arguments");
        var result = EmptyObject;
        for (var i = 0; i < args.Length; i += 2) result.Add(args[i].ToString()!, FromObject(args[i + 1]));
        return result;
    }
    private static object? ExposeInternalObjectHelper(object? x) {
        while (x is EasyObjectClassic) x = ((EasyObjectClassic)x).RealData;
        return x;
    }
    private static EasyObjectClassic WrapInternal(object? x) {
        if (x is EasyObjectClassic) return (x as EasyObjectClassic)!;
        return new EasyObjectClassic(x);
    }
    public bool ContainsKey(string name) {
        if (RealDictionary == null) return false;
        return RealDictionary.ContainsKey(name);
    }
    public EasyObjectClassic Add(object x) {
        if (RealList == null) RealData = new List<EasyObjectClassic>();
        var eo = x is EasyObjectClassic ? (x as EasyObjectClassic)! : new EasyObjectClassic(x);
        RealList!.Add(eo);
        return this;
    }
    public EasyObjectClassic Add(string key, object? x) {
        if (RealDictionary == null) RealData = new Dictionary<string, EasyObjectClassic>();
        var eo = x is EasyObjectClassic ? (x as EasyObjectClassic)! : new EasyObjectClassic(x);
        RealDictionary!.Add(key, eo);
        return this;
    }
    public override bool TryGetMember(
        GetMemberBinder binder, out object result) {
        result = Null;
        var name = binder.Name;
        if (RealList != null) {
            var assoc = TryAssoc(name);
            result = assoc;
        }
        if (RealDictionary == null) return true;
        EasyObjectClassic? eo;
        RealDictionary.TryGetValue(name, out eo);
        if (eo == null) eo = Null;
        result = eo;
        return true;
    }
    public override bool TrySetMember(
        SetMemberBinder binder, object? value) {
        value = ExposeInternalObjectHelper(value);
        if (RealDictionary == null) RealData = new Dictionary<string, EasyObjectClassic>();
        var name = binder.Name;
        RealDictionary![name] = WrapInternal(value);
        return true;
    }
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
        result = Null;
        var idx = indexes[0];
        if (idx is int) {
            var pos = (int)indexes[0];
            if (RealList == null) {
                result = WrapInternal(null);
                return true;
            }
            if (RealList.Count < pos + 1) {
                result = WrapInternal(null);
                return true;
            }
            result = WrapInternal(RealList[pos]);
            return true;
        }
        if (RealList != null) {
            var assoc = TryAssoc((string)idx);
            result = assoc;
        }
        if (RealDictionary == null) {
            result = Null;
            return true;
        }
        EasyObjectClassic? eo /*= Null*/;
        RealDictionary.TryGetValue((string)idx, out eo);
        if (eo == null) eo = Null;
        result = eo;
        return true;
    }
    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value) {
        if (value is EasyObjectClassic) value = ((EasyObjectClassic)value).RealData;
        var idx = indexes[0];
        if (idx is int) {
            var pos = (int)indexes[0];
            if (pos < 0) throw new ArgumentException("index is below 0");
            if (RealList == null) RealData = new List<EasyObjectClassic>();
            while (RealList!.Count < pos + 1) RealList.Add(Null);
            RealList[pos] = WrapInternal(value);
            return true;
        }
        if (RealDictionary == null) RealData = new Dictionary<string, EasyObjectClassic>();
        var name = (string)indexes[0];
        RealDictionary![name] = WrapInternal(value);
        return true;
    }
    public override bool TryConvert(ConvertBinder binder, out object? result) {
        if (binder.Type == typeof(IEnumerable)) {
            if (RealList != null) {
                var ie1 = RealList.Select(x => x);
                result = ie1;
                return true;
            }
            if (RealDictionary != null) {
                var ie2 = RealDictionary.Select(x => x);
                result = ie2;
                return true;
            }
            result = new List<EasyObjectClassic>().Select(x => x);
            return true;
        }
        result = Convert.ChangeType(RealData, binder.Type);
        return true;
    }
    private static string[] TextToLines(string text) {
        var lines = new List<string>();
        using (var sr = new StringReader(text)) {
            string? line;
            while ((line = sr.ReadLine()) != null) lines.Add(line);
        }
        return lines.ToArray();
    }
    public static EasyObjectClassic FromObject(object? obj, bool ignoreErrors = false) {
        if (!ignoreErrors) return new EasyObjectClassic(obj);
        try {
            return new EasyObjectClassic(obj);
        }
        catch (Exception) {
            return new EasyObjectClassic(null);
        }
    }
    public static EasyObjectClassic FromJson(string? json, bool ignoreErrors = false) {
        if (json == null) return Null;
        if (json.StartsWith("#!")) {
            var lines = TextToLines(json);
            lines = lines.Skip(1).ToArray();
            json = string.Join("\n", lines);
        }
        if (!ignoreErrors) return new EasyObjectClassic(JsonParser!.ParseJson(json));
        try {
            return new EasyObjectClassic(JsonParser!.ParseJson(json));
        }
        catch (Exception) {
            return new EasyObjectClassic(null);
        }
    }
    public dynamic? ToObject(bool asDynamicObject = false) {
        if (asDynamicObject) return ExportToDynamicObject();
        return ExportToPlainObject();
    }
    public string ToJson(bool indent = false, bool sortKeys = false, bool keyAsSymbol = false) {
        var poc = new PlainObjectConverterClassic(JsonParser, ForceAscii);
        return poc.Stringify(RealData, indent, sortKeys, keyAsSymbol);
    }
    public static string ToPrintable(object? x, string? title = null, bool compact = false) {
        var poc = new PlainObjectConverterClassic(JsonParser, ForceAscii);
        return poc.ToPrintable(ShowDetail, x, title, compact);
    }
    public static void Echo(
        object? x,
        string? title = null,
        bool compact = false,
        uint maxDepth = 0,
        List<string>? hideKeys = null
    ) {
        hideKeys ??= new List<string>();
        if (maxDepth > 0 || hideKeys.Count > 0) {
            var eo = FromObject(x);
            x = eo.Clone(
                maxDepth,
                hideKeys,
                false);
        }
        var s = ToPrintable(x, title, compact);
        Console.WriteLine(s);
        System.Diagnostics.Debug.WriteLine(s);
    }
    public static void Log(
        object? x,
        string? title = null,
        bool compact = false,
        uint maxDepth = 0,
        List<string>? hideKeys = null
    ) {
        hideKeys ??= new List<string>();
        if (maxDepth > 0 || hideKeys.Count > 0) {
            var eo = FromObject(x);
            x = eo.Clone(
                maxDepth,
                hideKeys,
                false);
        }
        var s = ToPrintable(x, title, compact);
        Console.Error.WriteLine("[Log] " + s);
        System.Diagnostics.Debug.WriteLine("[Log] " + s);
    }
    public static void Debug(
        object? x,
        string? title = null,
        bool compact = false,
        uint maxDepth = 0,
        List<string>? hideKeys = null
    ) {
        if (!DebugOutput) return;
        hideKeys ??= new List<string>();
        if (maxDepth > 0 || hideKeys.Count > 0) {
            var eo = FromObject(x);
            x = eo.Clone(
                maxDepth,
                hideKeys,
                false);
        }
        var s = ToPrintable(x, title, compact);
        Console.Error.WriteLine("[Debug] " + s);
        System.Diagnostics.Debug.WriteLine("[Debug] " + s);
    }
    public static void Message(
        object? x,
        string? title = null,
        bool compact = false,
        uint maxDepth = 0,
        List<string>? hideKeys = null
    ) {
        if (title == null) title = "Message";
        var s = ToPrintable(x, title, compact);
        NativeMethods.MessageBoxW(IntPtr.Zero, s, title, 0);
    }
    private EasyObjectClassic TryAssoc(string name) {
        try {
            if (RealList == null) return Null;
            for (var i = 0; i < RealList.Count; i++) {
                var pair = RealList[i].AsList!;
                if (pair[0].Cast<string>() == name) return pair[1];
            }
            return Null;
        }
        catch (Exception /*e*/) {
            return Null;
        }
    }
    public T Cast<T>() {
        if (RealData is DateTime dt) {
            string? s = null;
            switch (dt.Kind) {
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
        return (T)Convert.ChangeType(RealData, typeof(T))!;
    }
    public static string FullName(dynamic? x) {
        if (x is null) return "null";
        var fullName = ((object)x).GetType().FullName!;
        if (fullName.StartsWith("<>f__AnonymousType")) return "AnonymousType";
        return fullName.Split('`')[0];
    }
    public static implicit operator EasyObjectClassic(bool x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(string x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(char x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(short x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(int x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(long x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(ushort x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(uint x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(ulong x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(float x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(double x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(decimal x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(sbyte x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(byte x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(DateTime x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(TimeSpan x) {
        return new EasyObjectClassic(x);
    }
    public static implicit operator EasyObjectClassic(Guid x) {
        return new EasyObjectClassic(x);
    }
    public void Nullify() {
        RealData = null;
    }
    public void Trim(
        uint maxDepth = 0,
        List<string>? hideKeys = null
    ) {
        EasyObjectClassicEditor.Trim(this, maxDepth, hideKeys);
    }
    public EasyObjectClassic Clone(
        uint maxDepth = 0,
        List<string>? hideKeys = null,
        bool always = true
    ) {
        return EasyObjectClassicEditor.Clone(this, maxDepth, hideKeys, always);
    }
    public EasyObjectClassic? Shift() {
        if (RealList == null) return null;
        if (RealList.Count == 0) return null;
        var result = RealList[0];
        RealList.RemoveAt(0);
        return result;
    }
    public dynamic? ExportToDynamicObject() {
        return EasyObjectClassicEditor.ExportToExpandoObject(this);
    }
    public static string ObjectToJson(object? x, bool indent = false) {
        return FromObject(x).ToJson(indent);
    }
    public static object? ObjectToObject(object? x, bool asDynamicObject = false) {
        return FromObject(x).ToObject(asDynamicObject);
    }
    private static class NativeMethods {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int MessageBoxW(
            IntPtr hWnd, string lpText, string lpCaption, uint uType);
    }
}