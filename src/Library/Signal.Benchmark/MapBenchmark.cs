using BenchmarkDotNet.Attributes;

namespace Signal.Benchmark;

[MemoryDiagnoser]
[ShortRunJob]
public class MapBenchmark
{
    private static readonly Signal<int> _signalInt = new(10);
    private static readonly IReadOnlySignal<int> _signalIntMapped;
    
    private static readonly Signal<string> _signalString = new("test");
    private static readonly IReadOnlySignal<string> _signalStringMapped;

    static MapBenchmark()
    {
        _signalIntMapped = _signalInt.Map(v => v * 2);
        _signalStringMapped = _signalString.Map(v => v);
    }
    
    [Benchmark]
    public async ValueTask NoOpInt()
    {
        var signal = new Signal<int>(10);
        var mappedSignal = signal.Map(value => value * 2);
    }
    [Benchmark]
    public async ValueTask GetValueAsyncInt()
    {
        var signal = new Signal<int>(10);
        var mappedSignal = signal.Map(value => value * 2);
        await mappedSignal.GetValueAsync();
    }
    [Benchmark]
    public async ValueTask GetValueAsyncIntStatic()
    {
        await _signalIntMapped.GetValueAsync();
    }
    
    [Benchmark]
    public async ValueTask NoOpString()
    {
        var signal = new Signal<string>("test");
        var mappedSignal = signal.Map(value => value);
    }
    [Benchmark]
    public async ValueTask GetValueAsyncString()
    {
        var signal = new Signal<string>("test");
        var mappedSignal = signal.Map(value => value);
        await mappedSignal.GetValueAsync();
    }
    [Benchmark]
    public async ValueTask GetValueAsyncStringStatic()
    {
        await _signalStringMapped.GetValueAsync();
    }
}