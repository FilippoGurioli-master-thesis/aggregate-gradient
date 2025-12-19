using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public sealed class UnityCsvTiming : IDisposable
{
    private readonly StreamWriter _w;
    private readonly long _t0;
    private int _count;

    public UnityCsvTiming(string fileName = "unity.csv")
    {
        var path = Path.Combine(Application.persistentDataPath, fileName);
        _w = new StreamWriter(path, append: false);
        _w.WriteLine("t_ns,id,duration_ns");
        _t0 = Stopwatch.GetTimestamp();
    }

    public long NowTicks() => Stopwatch.GetTimestamp();

    public void Write(string id, long startTicks, long endTicks)
    {
        long tNs = ToNs(endTicks - _t0);
        long durNs = ToNs(endTicks - startTicks);
        _w.WriteLine($"{tNs},{id},{durNs}");
        if (++_count % 200 == 0) _w.Flush();
    }

    private static long ToNs(long ticks) =>
        (long)(ticks * (1_000_000_000.0 / Stopwatch.Frequency));

    public void Dispose()
    {
        _w.Flush();
        _w.Dispose();
    }
}
