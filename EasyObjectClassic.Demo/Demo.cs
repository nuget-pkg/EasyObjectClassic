//using Demo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Demo;
using Global;
using static Global.EasyObjectClassic;
// ReSharper disable OperatorIsCanBeUsed
// ReSharper disable UnusedVariable
// ReSharper disable CollectionNeverQueried.Local
// ReSharper disable EmptyForStatement
try {
    SetupConsoleEncoding();
    ShowDetail = true;
    Console.WriteLine("(1)");
    var eoNull = Null;
    Echo(eoNull.ToJson());
    Echo(eoNull.ToPrintable());
    Echo(eoNull);
    Echo(Null);
    Console.WriteLine("(2)");
    var eo = FromObject(new { a = 123 });
    Echo(eo);
    Console.WriteLine("(3)");
    Echo(eo.TypeValue, "eo.TypeValue");
    Console.WriteLine("(4)");
    EasyObject.AssertEqual(eo.TypeValue, @object);
    Console.WriteLine("(5)");
    var a = eo["a"];
    Console.WriteLine("(5.1)");
    Echo(FullName(a));
    Echo(a.GetType() == typeof(double));
    Console.WriteLine("(5.1.1)");
    //Echo(a.ToObject());
    Console.WriteLine("(5.1.1.1)");
    ////Echo(poc.Stringify(a, true));
    Console.WriteLine("(5.1.2)");
    Echo(a, "a");
    Console.WriteLine("(5.2)");
    Console.WriteLine(eo["a"]);
    Console.WriteLine("(6)");
    EasyObject.AssertEqual(eo["a"].Cast<int>(), 123);
    Console.WriteLine("(7)");
    EasyObject.AssertEqual(eo.Keys, new List<string> { "a" });
    Echo(eo[0], "eo[0]");
    EasyObject.AssertEqual(eo[0].TypeValue, @null);
    EasyObject.AssertTrue(eo[0].IsNull);
    Echo(eo[1], "eo[1]");
    EasyObject.AssertEqual(eo[1].TypeValue, @null);
    EasyObject.AssertTrue(eo[1].IsNull);
    foreach (var pair in eo.Dynamic) Echo(pair, "pair");
    eo = FromObject(null);
    Echo(eo.TypeValue, "eo.TypeValue");
    EasyObject.AssertEqual(eo.TypeValue, @null);
    eo["b"] = true;
    EasyObject.AssertEqual(eo["b"].Cast<bool>(), true);
    Echo(eo["b"].TypeValue, "eo.b.TypeValue");
    EasyObject.AssertEqual(eo["b"].TypeValue, boolean);
    eo[3] = 777;
    Echo(eo[3].Cast<int>());
    Echo(eo.TypeValue, "eo.TypeValue");
    EasyObject.AssertEqual(eo.TypeValue, array);
    EasyObject.AssertEqual(eo.Count, 4);
    EasyObject.AssertEqual(eo[0].TypeValue, @null);
    EasyObject.AssertEqual(eo[3].Cast<int>(), 777);
    foreach (var e in eo.Dynamic) Echo(e, "e");
    var eo2 = FromObject(eo); // UnWrap() test
    var eo3 = FromJson("""
                       { a: 123, b: [11, 22, 33] }
                       """);
    Echo(eo3, "eo3");
    Echo(eo3["b"][1]);
    var list = new List<int>();
    foreach (var e in eo3["b"].Dynamic) list.Add((int)e);
    Echo(list, "list");
    Echo(eo3["b"].TypeName);
    Echo(eo3["b"].IsArray);
    Echo(eo3["b"].IsNull);
    eo3["b"].Add(777);
    eo3.AsDictionary!["b"].Add(888);
    Echo(eo3);
    var i = FromObject(123);
    Echo(i.Cast<double>());
    var iList1 = eo3["b"].AsList!.Select(x => x.Cast<int>()).ToList();
    Echo(iList1.GetType().FullName);
    Echo(iList1.GetType().ToString());
    var dict = eo3.AsDictionary;
    Echo(dict);
    Echo(dict["a"].Cast<double>());
    foreach (var e in i.Dynamic) Echo(e);
    var bigJson = File.ReadAllText("assets/qiita-9ea0c8fd43b61b01a8da.json");
    //Echo(bigJson);
    var sw = new Stopwatch();
    TimeSpan ts;
    sw.Start();
    for (var c = 0; c < 5; c++) {
        //var test = FromJson(bigJson);
    }
    sw.Stop();
    Console.WriteLine("■EasyObjectClassic");
    ts = sw.Elapsed;
    Console.WriteLine($"　{ts}");
    Console.WriteLine($"　{ts.Hours}時間 {ts.Minutes}分 {ts.Seconds}秒 {ts.Milliseconds}ミリ秒");
    Console.WriteLine($"　{sw.ElapsedMilliseconds}ミリ秒");
    sw.Start();
    for (var c = 0; c < 5; c++) {
        //JObject jsonObject = JObject.Parse(bigJson);
    }
    sw.Stop();
    Console.WriteLine("■Newtonsoft.Json");
    ts = sw.Elapsed;
    Console.WriteLine($"　{ts}");
    Console.WriteLine($"　{ts.Hours}時間 {ts.Minutes}分 {ts.Seconds}秒 {ts.Milliseconds}ミリ秒");
    Console.WriteLine($"　{sw.ElapsedMilliseconds}ミリ秒");
    Echo(DateTime.Now);
    Echo(FromJson(null));
    var exc1 = new Exchangeable1();
    Echo(exc1, "exc1");
    var exc2 = new Exchangeable2();
    Echo(exc2, "exc2");
    var exc3 = new Exchangeable3();
    Echo(exc3, "exc3");
    var exc4 = new Exchangeable4();
    Echo(exc4, "exc4");
    var myData = new MyData(123, "xyz");
    Echo(myData.N, "myData.N");
    Echo(myData.S, "myData.S");
    var myData2 = new MyData("""{n: 456, s: "ABC"}""");
    Echo(myData2.RealData == null);
    Echo(myData2.N, "myData2.N");
    Echo(myData2.S, "myData2.S");
    Echo(myData.ExportToCommonJson(), "myData.ExportToCommonJson()");
    Echo(myData2.ExportToCommonJson(), "myData2.ExportToCommonJson()");
    var trimmedJson = """
                      {
                        x: [
                          1,
                          2,
                          3,
                          [
                            "a",
                            "b",
                            "c",
                            [ 11, 22, 33]
                          ]
                        ],
                        y: {
                          a: 1111, b: 2222
                        }
                      }
                      """;
    EasyObjectClassic trimTest;
    trimTest = FromJson(trimmedJson);
    Echo(trimTest, maxDepth: 1);
    trimTest.Trim(1);
    Echo(trimTest, "(1)");
    trimTest = FromJson(trimmedJson);
    Echo(trimTest, maxDepth: 2);
    trimTest.Trim(2);
    Echo(trimTest, "(2)");
    trimTest = FromJson(trimmedJson);
    Echo(trimTest, hideKeys: ["a"]);
    trimTest.Trim(hideKeys: ["a"]);
    Echo(trimTest, "(3)");
    string[] myArgs = ["apple", "melon", "peach"];
    var eoArgs = FromObject(myArgs);
    var first = eoArgs.Shift();
    Echo(new { first, eoArgs });
    myArgs = Array.ConvertAll((eoArgs.ToObject()!.ToArray() as object[])!, obj => obj?.ToString() ?? "");
    Echo(new { myArgs });
    EasyObjectClassic ast;
    ast = FromJson(BabelOutput.AstJson);
    ast.Trim(hideKeys: ["loc", "start", "end"], maxDepth: 2);
    Echo(ast, "ast(1)");
    ast = FromJson(BabelOutput.AstJson);
    ast.Trim(hideKeys: ["loc", "start", "end"], maxDepth: 3);
    Echo(ast, "ast(2)");
    var noError = FromJson("\n", true);
    Echo(noError, "noError");
    var transfered = FromObject(EasyObject.NewArray(11, 22, 33));
    Log(transfered, "transfered");
    Log("⁅記号2026-0331⁆❝❞↪️ ↩️ ℴ➺➢ ▸ ヾ➠▶🈂＼／：＊“≪≫￤；‘｀＃％＄＆＾～￤⁅⁆≪≫＋ー＊＝◉");
    Log("[END]");
}
catch (Exception e) {
    EasyObject.Abort(e);
}
internal class Exchangeable1 : IExportToPlainObject {
    public object ExportToPlainObject() {
        return 123;
    }
}
internal class Exchangeable2 {
    public object ExportToPlainObject() {
        return 456;
    }
}
internal class Exchangeable3 : IExportToCommonJson {
    public string ExportToCommonJson() {
        return "[11, 22, 33]";
    }
}
internal class Exchangeable4 {
    public string ExportToCommonJson() {
        return "[111, 222, 333]";
    }
}
internal class MyData : EasyObjectClassic {
    public MyData(int n, string s) {
        ImportFromPlainObject(new { n, s });
    }
    public MyData(string json) {
        ImportFromCommonJson(json);
    }
    public int N => Dynamic.n;
    public string S => Dynamic.s;
}