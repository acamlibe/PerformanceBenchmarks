using System.Text;
using BenchmarkDotNet.Running;
using PerformanceBenchmarks;

// BenchmarkRunner.Run<ExceptionBenchmarks>();

int Rows = 1000;
int Columns = 10;

const char Separator = ',';

var random = new Random();

var sb = new StringBuilder();

for (int row = 0; row < Rows; row++)
{
    int[] nums = new int[Columns];

    for (int col = 0; col < Columns; col++)
    {
        int num = random.Next(1000);

        nums[col] = num;
    }

    sb.AppendLine(string.Join(Separator, nums));
}

string _csv = sb.ToString();


int methodOneSum = 0;
string[] csvLines = _csv.Split('\n');

int rowCount = csvLines.Length;
int colCount = csvLines[0].Split(Separator).Length + 1;

System.Console.WriteLine(rowCount);
System.Console.WriteLine(colCo);

int[,] data = new int[rowCount, colCount];

for (int row = 0; row < rowCount; row++)
{
    string line = csvLines[row];
    string[] lineValues = line.Split(Separator);

    for (int col = 0; col < colCount; col++)
    {
        string value = lineValues[col];
        int.TryParse(value, out data[row, col]);

        methodOneSum += data[row, col];
    }
}


int rCount = 0;

ReadOnlySpan<char> csvSpan = _csv.AsSpan();
foreach (char c in csvSpan)
{
    if (c == '\n')
    {
        rCount++;
    }
}

int methodTwoSum = 0;
Span<Range> lineRanges = stackalloc Range[rCount];

Range firstLine = lineRanges[0];
int cCount = csvSpan[firstLine].Count(Separator) + 1;

int[,] data2 = new int[rCount, cCount];

Span<Range> valueRanges = stackalloc Range[cCount];

for (int row = 0; row < rCount; row++)
{
    Range lineRange = lineRanges[row];

    ReadOnlySpan<char> line = csvSpan[lineRange];
    
    int splitCount = line.Split(valueRanges, Separator);

    for (int col = 0; col < splitCount; col++)
    {
        Range range = valueRanges[col];

        ReadOnlySpan<char> value = line[range];
        int.TryParse(value, out data2[row, col]);

        methodTwoSum += data2[row, col];
    }
}

System.Console.WriteLine(methodOneSum);
System.Console.WriteLine(methodTwoSum);