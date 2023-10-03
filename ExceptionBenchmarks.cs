using BenchmarkDotNet.Attributes;

namespace PerformanceBenchmarks;

[MemoryDiagnoser(true)]
public class ExceptionBenchmarks
{
    [Params(10_000)]
    public int N { get; set; }

    private char[] _data = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _data = new char[N];

        var random = new Random();

        for (int i = 0; i < N; i++)
        {
            _data[i] = (char)random.Next(128);
        }
    }

    [Benchmark]
    public void IntParse()
    {
        int[] numbers = new int[_data.Length];

        for (int i = 0; i < _data.Length; i++)
        {
            string character = _data[i].ToString();

            try
            {
                numbers[i] = int.Parse(character);
            }
            catch
            {
                numbers[i] = 0;
            }
        }
    }

    [Benchmark]
    public void IntTryParse()
    {
        int[] numbers = new int[_data.Length];

        for (int i = 0; i < _data.Length; i++)
        {
            string character = _data[i].ToString();

            int.TryParse(character, out numbers[i]);
        }
    }
}