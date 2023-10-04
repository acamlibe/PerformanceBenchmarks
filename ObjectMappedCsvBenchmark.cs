using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace PerformanceBenchmarks;

[MemoryDiagnoser(true)]
public class ObjectMappedCsvBenchmark
{
    public class Person
    {
        public string FirstName { get; set; } = null!;
        public string MiddleInitial { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public int Age { get; set; }
        public int CSharpExperienceYears { get; set; }
        public int SQLExperienceYears { get; set; }
        public int HTMLExperienceYears { get; set; }

        public string FullName
        {
            get
            {
                return string.Concat(LastName, ", ", FirstName, " ", MiddleInitial);
            }
        }
        public override string ToString()
        {
            return $"\t{FullName} - Age: {Age} C#: {CSharpExperienceYears}, SQL: {SQLExperienceYears}, HTML: {HTMLExperienceYears}";
        }
    }

    private const char Separator = ',';
    private const char NewLine = '\n';

    [Params(10_000, 100_000)]
    public int N { get; set; }

    private string _csv = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var random = new Random();

        string[] firstNameList = ["Ali", "Patryck", "David", "Dinesh"];
        string[] lastNameList = ["Smith", "Johnson", "Williams", "Davis"];
        string[] middleInitialList = ["O", "A", "S", "D"];

        var sb = new StringBuilder();

        for (int row = 0; row < N; row++)
        {
            string firstName = firstNameList[random.Next(firstNameList.Length)];
            string middleInitial = middleInitialList[random.Next(middleInitialList.Length)];
            string lastName = lastNameList[random.Next(lastNameList.Length)];

            string age = random.Next(18, 65).ToString();
            string csharpExp = random.Next(0, 20).ToString();
            string sqlExp = random.Next(0, 20).ToString();
            string htmlExp = random.Next(0, 20).ToString();

            string[] values = [firstName, middleInitial, lastName, age, csharpExp, sqlExp, htmlExp];
            sb.AppendLine(string.Join(Separator, values));
        }

        //Remove last \n char
        sb.Length--;

        _csv = sb.ToString();
    }

    [Benchmark]
    public void Base()
    {
        string[] csvLines = _csv.Split(NewLine);
        int rowCount = csvLines.Length;

        Person[] people = new Person[rowCount];

        for (int row = 0; row < rowCount; row++)
        {
            string line = csvLines[row];
            string[] lineValues = line.Split(Separator);

            string firstName = lineValues[0];
            string middleInitial = lineValues[1];
            string lastName = lineValues[2];

            int.TryParse(lineValues[3], out int age);
            int.TryParse(lineValues[4], out int csharpExp);
            int.TryParse(lineValues[5], out int sqlExp);
            int.TryParse(lineValues[6], out int htmlExp);

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

            people[row] = person;
        }
    }

    [Benchmark]
    public void WithSpan()
    {
        ReadOnlySpan<char> csvSpan = _csv.AsSpan();

        int rowCount = csvSpan.Count(NewLine) + 1;

        Span<Range> lineRanges = stackalloc Range[rowCount];
        csvSpan.Split(lineRanges, NewLine);

        Span<Range> valueRanges = stackalloc Range[7];

        Person[] people = new Person[rowCount];

        for (int row = 0; row < rowCount; row++)
        {
            Range lineRange = lineRanges[row];
            ReadOnlySpan<char> line = csvSpan[lineRange];
            line.Split(valueRanges, Separator);

            ReadOnlySpan<char> firstNameSpan = line[valueRanges[0]];
            ReadOnlySpan<char> middleInitialSpan = line[valueRanges[1]];
            ReadOnlySpan<char> lastNameSpan = line[valueRanges[2]];

            string firstName = firstNameSpan.ToString();
            string middleInitial = middleInitialSpan.ToString();
            string lastName = lastNameSpan.ToString();

            int.TryParse(line[valueRanges[3]], out int age);
            int.TryParse(line[valueRanges[4]], out int csharpExp);
            int.TryParse(line[valueRanges[5]], out int sqlExp);
            int.TryParse(line[valueRanges[6]], out int htmlExp);

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

            people[row] = person;
        }
    }

    [Benchmark]
    public void WithStringPool()
    {
        ReadOnlySpan<char> csvSpan = _csv.AsSpan();

        int rowCount = csvSpan.Count(NewLine) + 1;

        Span<Range> lineRanges = stackalloc Range[rowCount];
        csvSpan.Split(lineRanges, NewLine);

        Span<Range> valueRanges = stackalloc Range[7];

        Person[] people = new Person[rowCount];

        for (int row = 0; row < rowCount; row++)
        {
            Range lineRange = lineRanges[row];
            ReadOnlySpan<char> line = csvSpan[lineRange];
            line.Split(valueRanges, Separator);

            ReadOnlySpan<char> firstNameSpan = line[valueRanges[0]];
            ReadOnlySpan<char> middleInitialSpan = line[valueRanges[1]];
            ReadOnlySpan<char> lastNameSpan = line[valueRanges[2]];

            string firstName = StringPool.Shared.GetOrAdd(firstNameSpan);
            string middleInitial = StringPool.Shared.GetOrAdd(middleInitialSpan);
            string lastName = StringPool.Shared.GetOrAdd(lastNameSpan);

            int.TryParse(line[valueRanges[3]], out int age);
            int.TryParse(line[valueRanges[4]], out int csharpExp);
            int.TryParse(line[valueRanges[5]], out int sqlExp);
            int.TryParse(line[valueRanges[6]], out int htmlExp);

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

            people[row] = person;
        }
    }

    [Benchmark]
    public void WithSpanOwner()
    {
        ReadOnlySpan<char> csvSpan = _csv.AsSpan();

        int rowCount = csvSpan.Count(NewLine) + 1;

        Span<Range> lineRanges = stackalloc Range[rowCount];
        csvSpan.Split(lineRanges, NewLine);

        Span<Range> valueRanges = stackalloc Range[7];

        using SpanOwner<Person> personBuffer = SpanOwner<Person>.Allocate(rowCount);

        for (int row = 0; row < rowCount; row++)
        {
            Range lineRange = lineRanges[row];
            ReadOnlySpan<char> line = csvSpan[lineRange];
            line.Split(valueRanges, Separator);

            ReadOnlySpan<char> firstNameSpan = line[valueRanges[0]];
            ReadOnlySpan<char> middleInitialSpan = line[valueRanges[1]];
            ReadOnlySpan<char> lastNameSpan = line[valueRanges[2]];

            string firstName = StringPool.Shared.GetOrAdd(firstNameSpan);
            string middleInitial = StringPool.Shared.GetOrAdd(middleInitialSpan);
            string lastName = StringPool.Shared.GetOrAdd(lastNameSpan);

            int.TryParse(line[valueRanges[3]], out int age);
            int.TryParse(line[valueRanges[4]], out int csharpExp);
            int.TryParse(line[valueRanges[5]], out int sqlExp);
            int.TryParse(line[valueRanges[6]], out int htmlExp);

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

            personBuffer.Span[row] = person;
        }
    }

    [Benchmark]
    public MemoryOwner<Person> WithMemoryOwner()
    {
        ReadOnlySpan<char> csvSpan = _csv.AsSpan();

        int rowCount = csvSpan.Count(NewLine) + 1;

        Span<Range> lineRanges = stackalloc Range[rowCount];
        csvSpan.Split(lineRanges, NewLine);

        Span<Range> valueRanges = stackalloc Range[7];

        MemoryOwner<Person> personBuffer = MemoryOwner<Person>.Allocate(rowCount);

        for (int row = 0; row < rowCount; row++)
        {
            Range lineRange = lineRanges[row];
            ReadOnlySpan<char> line = csvSpan[lineRange];
            line.Split(valueRanges, Separator);

            ReadOnlySpan<char> firstNameSpan = line[valueRanges[0]];
            ReadOnlySpan<char> middleInitialSpan = line[valueRanges[1]];
            ReadOnlySpan<char> lastNameSpan = line[valueRanges[2]];

            string firstName = StringPool.Shared.GetOrAdd(firstNameSpan);
            string middleInitial = StringPool.Shared.GetOrAdd(middleInitialSpan);
            string lastName = StringPool.Shared.GetOrAdd(lastNameSpan);

            int.TryParse(line[valueRanges[3]], out int age);
            int.TryParse(line[valueRanges[4]], out int csharpExp);
            int.TryParse(line[valueRanges[5]], out int sqlExp);
            int.TryParse(line[valueRanges[6]], out int htmlExp);

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

            personBuffer.Span[0] = person;
        }

        return personBuffer;
    }
}