using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

internal static class CollektiveApiWithDistance
{
    private const string LibName = "simple_gradient";

    [DllImport(LibName, EntryPoint = "create_with_distance", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Create(int nodeCount, double maxDistance);

    [DllImport(LibName, EntryPoint = "destroy_with_distance", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Destroy(int handle);

    [DllImport(LibName, EntryPoint = "set_source_with_distance", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetSource(int handle, int nodeId, [MarshalAs(UnmanagedType.I1)] bool isSource);

    [DllImport(LibName, EntryPoint = "clear_sources_with_distance", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ClearSources(int handle);

    [DllImport(LibName, EntryPoint = "step_with_distance", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Step(int handle, int rounds);

    [DllImport(LibName, EntryPoint = "get_value_with_distance", CallingConvention = CallingConvention.Cdecl)]
    public static extern double GetValue(int handle, int nodeId);

    [DllImport(LibName, EntryPoint = "get_neighborhood_with_distance", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetNeighborhoodNative(int handle, int nodeId, out int size);

    [DllImport(LibName, EntryPoint = "free_neighborhood_with_distance", CallingConvention = CallingConvention.Cdecl)]
    private static extern void FreeNeighborhood(IntPtr ptr);

    [DllImport(LibName, EntryPoint = "update_position", CallingConvention = CallingConvention.Cdecl)]
    private static extern void UpdatePosition(int handle, int nodeId, double x, double y, double z);

    [DllImport(LibName, EntryPoint = "step_and_get_state_with_distance", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr StepAndGetStateWithDistance(int handle, int rounds, out int outSize);

    [DllImport(LibName, EntryPoint = "free_state_buffer", CallingConvention = CallingConvention.Cdecl)]
    private static extern void FreeStateBuffer(IntPtr ptr);

    public static List<(double value, List<int> neighbors)> StepAndGetState(int handle, int rounds, UnityCsvTiming timing)
    {
        var tCall0 = timing.NowTicks();
        var ptr = StepAndGetStateWithDistance(handle, rounds, out var sizeBytes);
        var tCall1 = timing.NowTicks();
        if (ptr == IntPtr.Zero || sizeBytes <= 0)
            return new List<(double, List<int>)>();
        try
        {
            var tParse0 = timing.NowTicks();
            var bytes = new byte[sizeBytes];
            Marshal.Copy(ptr, bytes, 0, sizeBytes);
            int offset = 0;
            int nodeCount = ReadInt32LE(bytes, ref offset);
            var result = new List<(double value, List<int> neighbors)>(nodeCount);
            for (int i = 0; i < nodeCount; i++)
            {
                double value = ReadDoubleLE(bytes, ref offset);
                int nCount = ReadInt32LE(bytes, ref offset);
                var neigh = new List<int>(nCount);
                for (int k = 0; k < nCount; k++)
                    neigh.Add(ReadInt32LE(bytes, ref offset));
                result.Add((value, neigh));
            }

            var tParse1 = timing.NowTicks();
            timing.Write("step.native.call", tCall0, tCall1);
            timing.Write("step.native.parse", tParse0, tParse1);
            return result;
        }
        finally
        {
            FreeStateBuffer(ptr);
        }
    }

    private static int ReadInt32LE(byte[] b, ref int o)
    {
        int v = b[o]
                | (b[o + 1] << 8)
                | (b[o + 2] << 16)
                | (b[o + 3] << 24);
        o += 4;
        return v;
    }

    private static double ReadDoubleLE(byte[] b, ref int o)
    {
        long v =
            (long)b[o]
            | ((long)b[o + 1] << 8)
            | ((long)b[o + 2] << 16)
            | ((long)b[o + 3] << 24)
            | ((long)b[o + 4] << 32)
            | ((long)b[o + 5] << 40)
            | ((long)b[o + 6] << 48)
            | ((long)b[o + 7] << 56);
        o += 8;
        return BitConverter.Int64BitsToDouble(v);
    }

    public static void UpdatePosition(int handle, int nodeId, Vector3 position) => UpdatePosition(handle, nodeId, position.x, position.y, position.z);

    public static List<int> GetNeighborhood(int handle, int nodeId)
    {
        int size;
        IntPtr ptr = GetNeighborhoodNative(handle, nodeId, out size);
        if (size == 0 || ptr == IntPtr.Zero)
            return new List<int>();
        var result = new int[size];
        Marshal.Copy(ptr, result, 0, size);
        FreeNeighborhood(ptr);
        return new List<int>(result);
    }
}
