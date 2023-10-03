using System.Text;
using BenchmarkDotNet.Attributes;

namespace PerformanceBenchmarks;

[MemoryDiagnoser(true)]
public class CsvBenchmark
{
    private const char Separator = ',';


    [Params(1000, 10_000, 100_000)]
    public int Rows { get; set; }

    [Params(10, 20)]
    public int Columns { get; set; }

    private string _csv = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
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

        //Remove last \n char
        sb.Length--;

        _csv = sb.ToString();
    }

    [Benchmark]
    public void Base()
    {
        string[] csvLines = _csv.Split('\n');

        int rowCount = csvLines.Length;
        int colCount = csvLines[0].Split(Separator).Length;

        int[,] data = new int[rowCount, colCount];

        for (int row = 0; row < rowCount; row++)
        {
            string line = csvLines[row];
            string[] lineValues = line.Split(Separator);

            for (int col = 0; col < colCount; col++)
            {
                string value = lineValues[col];
                int.TryParse(value, out data[row, col]);
            }
        }
    }

    [Benchmark]
    public void WithSpan()
    {
        int rowCount = 1;

        ReadOnlySpan<char> csvSpan = _csv.AsSpan();
        foreach (char c in csvSpan)
        {
            if (c == '\n')
            {
                rowCount++;
            }
        }

        Span<Range> lineRanges = stackalloc Range[rowCount];
        csvSpan.Split(lineRanges, '\n');

        Range firstLine = lineRanges[0];
        int colCount = csvSpan[firstLine].Count(Separator) + 1;

        int[,] data = new int[rowCount, colCount];

        Span<Range> valueRanges = stackalloc Range[colCount];

        for (int row = 0; row < rowCount; row++)
        {
            Range lineRange = lineRanges[row];

            ReadOnlySpan<char> line = csvSpan[lineRange];
            
            int splitCount = line.Split(valueRanges, Separator);

            for (int col = 0; col < splitCount; col++)
            {
                Range range = valueRanges[col];

                ReadOnlySpan<char> value = line[range];
                int.TryParse(value, out data[row, col]);
            }
        }
    }
}