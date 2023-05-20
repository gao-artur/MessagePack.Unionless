#if !DEBUG
using BenchmarkDotNet.Running;
#endif

namespace MessagePack.Unionless.Benchmark;

public class Program
{
    public static void Main(string[] args)
    {
#if DEBUG
        var benchmark = new UnionSerializerBenchmark();
        benchmark.Setup();
#else
        BenchmarkRunner.Run<UnionSerializerBenchmark>();
#endif
    }
}