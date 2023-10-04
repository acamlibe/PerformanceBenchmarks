using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.Toolkit.HighPerformance;

namespace PerformanceBenchmarks;

[MemoryDiagnoser(true)]
public partial class FileWriteBenchmarks
{
    private const char Separator = ',';
    private const char NewLine = '\n';

    [Params(1_000, 100_000)]
    public int N { get; set; }

    private Person[] _people = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _people = new Person[N];

        string[] firstNameList = ["Ali", "Patryck", "David", "Dinesh"];
        string[] lastNameList = ["Smith", "Johnson", "Williams", "Davis"];
        string[] middleInitialList = ["O", "A", "S", "D"];

        var random = new Random();
        var sb = new StringBuilder();

        for (int row = 0; row < N; row++)
        {
            string firstName = firstNameList[random.Next(firstNameList.Length)];
            string middleInitial = middleInitialList[random.Next(middleInitialList.Length)];
            string lastName = lastNameList[random.Next(lastNameList.Length)];

            int age = random.Next(18, 65);
            int csharpExp = random.Next(0, 20);
            int sqlExp = random.Next(0, 20);
            int htmlExp = random.Next(0, 20);

            var person = new Person
            {
                FirstName = firstName,
                MiddleInitial = middleInitial,
                LastName = lastName,
                Age = age,
                CSharpExperienceYears = csharpExp,
                SQLExperienceYears = sqlExp,
                HTMLExperienceYears = htmlExp
            };

            _people[row] = person;
        }
    }

    [Benchmark]
    public void WriteAllLines()
    {
        string[] lines = new string[_people.Length];

        for (int i = 0; i < _people.Length; i++)
        {
            lines[i] = _people[i].ToString();
        }

        string path = "/home/alic/Projects/PerformanceBenchmarks/output/writealllines.csv";

        File.WriteAllLines(path, lines);
    }

    [Benchmark]
    public void WriteAllText()
    {
        var sb = new StringBuilder(); 

        for (int i = 0; i < _people.Length; i++)
        {
            string line = _people[i].ToString();

            sb.AppendLine(line);
        }

        string csv = sb.ToString();
        string path = "/home/alic/Projects/PerformanceBenchmarks/output/writealltext.csv";

        File.WriteAllText(path, csv);
    }

    [Benchmark]
    public void StreamWriter()
    {
        string path = "/home/alic/Projects/PerformanceBenchmarks/output/streamwriter.csv";

        using var fs = new FileStream(
            path, 
            FileMode.Create, 
            FileAccess.Write, 
            FileShare.None, 
            bufferSize: 4096);

        using var sr = new StreamWriter(fs);

        for (int i = 0; i < _people.Length; i++)
        {
            string line = _people[i].ToString();

            sr.WriteLine(line);
        }
    }

    [Benchmark]
    public void FileStream()
    {
        string path = "/home/alic/Projects/PerformanceBenchmarks/output/filestream.csv";

        using var fs = new FileStream(
            path, 
            FileMode.Create, 
            FileAccess.Write, 
            FileShare.None, 
            bufferSize: 4096);

        for (int i = 0; i < _people.Length; i++)
        {
            string line = _people[i].ToString() + '\n';

            byte[] encoded = Encoding.Unicode.GetBytes(line);
            
            fs.Write(encoded, 0, encoded.Length);
        }
    }

    [Benchmark]
    public void FileStreamMemoryMarshalSpan()
    {
        string path = "/home/alic/Projects/PerformanceBenchmarks/output/filestreamspan.csv";

        using var fs = new FileStream(
            path, 
            FileMode.Create, 
            FileAccess.Write, 
            FileShare.None, 
            bufferSize: 4096);

        for (int i = 0; i < _people.Length; i++)
        {
            string line = _people[i].ToString() + '\n';

            ReadOnlySpan<byte> encoded = MemoryMarshal.AsBytes(line.AsSpan());
            
            fs.Write(encoded);
        }
    }
}