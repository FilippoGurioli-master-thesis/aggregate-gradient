using System.Runtime.InteropServices;

internal static class CollektiveNativeApi
{
    private const string LibName = "simple_gradient";

    [DllImport(LibName, EntryPoint = "create", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Create(int nodeCount, int maxDegree);

    [DllImport(LibName, EntryPoint = "destroy", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Destroy(int handle);

    [DllImport(LibName, EntryPoint = "set_source", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern void SetSource(
        int handle,
        int nodeId,
        [MarshalAs(UnmanagedType.I1)] bool isSource);

    [DllImport(LibName, EntryPoint = "clear_sources", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ClearSources(int handle);

    [DllImport(LibName, EntryPoint = "step", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Step(int handle, int rounds);

    [DllImport(LibName, EntryPoint = "get_value", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetValue(int handle, int nodeId);
}
