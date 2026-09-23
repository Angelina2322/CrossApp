using System.Text.Json;
using Core;

const string Student = "Павлище Ангеліна, група ФЕІ-34";
const string Domain = "Склад (товари, партії, залишки, переміщення)";

Console.OutputEncoding = System.Text.Encoding.UTF8;

bool jsonOutput = args.Contains("--json");
EnvironmentReport report = EnvironmentInfo.Collect();

if (jsonOutput)
{
    var info = new
    {
        Application = "CrossApp",
        Student,
        OSDescription = report.OsDescription,
        OSVersion = report.OsVersion,
        ProcessArchitecture = report.ProcessArchitecture,
        DotNetVersion = report.ClrVersion,
        Runtime = report.FrameworkDescription,
        RidDetected = report.DetectedRid,
        RidReported = report.ReportedRid,
        BaseDirectory = report.BaseDirectory,
        CurrentDirectory = report.CurrentDirectory,
        Domain
    };
    Console.WriteLine(JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true }));
}
else
{
    Console.WriteLine("CrossApp – інформація про середовище");
    Console.WriteLine($"Студентка: {Student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription): {report.OsDescription}");
    Console.WriteLine($"ОС (Environment)  : {report.OsVersion}");
    Console.WriteLine($"Архітектура       : {report.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR) : {report.ClrVersion}");
    Console.WriteLine($"Runtime           : {report.FrameworkDescription}");
    Console.WriteLine($"RID (визначено)   : {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET)    : {report.ReportedRid}");
    Console.WriteLine($"Каталог застосунку: {report.BaseDirectory}");
    Console.WriteLine($"Поточний каталог  : {report.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {Domain}");
}