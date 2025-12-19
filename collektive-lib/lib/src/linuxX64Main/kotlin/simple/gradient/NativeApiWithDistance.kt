package simple.gradient

import arrow.atomic.AtomicInt
import kotlinx.cinterop.*
import platform.posix.memcpy
import time.NativeCsvFileSink
import time.TimeMeasurer
import kotlin.Double.Companion.POSITIVE_INFINITY
import kotlin.experimental.ExperimentalNativeApi

private val nextHandle = AtomicInt(1)
private val engines = mutableMapOf<Int, CollektiveEngineWithDistance>()
private val sink = NativeCsvFileSink("native.csv")
private val tm = TimeMeasurer(sink)

@OptIn(ExperimentalNativeApi::class)
@CName("create_with_distance")
fun createWithDistance(nodeCount: Int, maxDistance: Double): Int {
    val handle = nextHandle.addAndGet(1)
    val engine = CollektiveEngineWithDistance(nodeCount, maxDistance)
    engines[handle] = engine
    return handle
}

@OptIn(ExperimentalNativeApi::class)
@CName("destroy_with_distance")
fun destroyWithDistance(handle: Int) {
    engines.remove(handle)
    if (engines.isEmpty()) sink.close()
}

@OptIn(ExperimentalNativeApi::class)
@CName("set_source_with_distance")
fun setSourceWithDistance(handle: Int, nodeId: Int, isSource: Boolean) {
    val engine = engines[handle] ?: return
    engine.setSource(nodeId, isSource)
}

@OptIn(ExperimentalNativeApi::class)
@CName("clear_sources_with_distance")
fun clearSourcesWithDistance(handle: Int) {
    val engine = engines[handle] ?: return
    engine.clearSources()
}

@OptIn(ExperimentalNativeApi::class)
@CName("step_with_distance")
fun stepWithDistance(handle: Int, rounds: Int) {
    val engine = engines[handle] ?: return
    tm.start("step.compute")
    engine.stepMany(rounds)
    tm.stop("step.compute")
}

@OptIn(ExperimentalNativeApi::class)
@CName("get_value_with_distance")
fun getValueWithDistance(handle: Int, nodeId: Int): Double {
    val engine = engines[handle] ?: return POSITIVE_INFINITY
    return engine.getValue(nodeId)
}

@OptIn(ExperimentalNativeApi::class, ExperimentalForeignApi::class)
@CName("get_neighborhood_with_distance")
fun getNeighborhoodWithDistance(
    handle: Int,
    nodeId: Int,
    outSize: CPointer<IntVar>
): CPointer<IntVar>? {
    val engine = engines[handle]
    if (engine == null) {
        outSize.pointed.value = 0
        return null
    }

    val neighbors: List<Int> =
        engine.getNeighborhood(nodeId).toList()

    outSize.pointed.value = neighbors.size
    if (neighbors.isEmpty()) return null

    val ptr = nativeHeap.allocArray<IntVar>(neighbors.size)
    for (i in neighbors.indices) {
        ptr[i] = neighbors[i]
    }
    return ptr
}

@OptIn(ExperimentalNativeApi::class, ExperimentalForeignApi::class)
@CName("free_neighborhood_with_distance")
fun freeNeighborhoodWithDistance(ptr: CPointer<IntVar>?) {
    if (ptr != null) {
        nativeHeap.free(ptr)
    }
}

@OptIn(ExperimentalNativeApi::class, ExperimentalForeignApi::class)
@CName("update_position")
fun updatePosition(handle: Int, nodeId: Int, x: Double, y: Double, z: Double) {
    engines[handle]?.updateNodePosition(nodeId, Position(x,y,z))
}

//--------------------------------------------------------------------------------------------------

@OptIn(ExperimentalNativeApi::class, ExperimentalForeignApi::class)
@CName("step_and_get_state_with_distance")
fun stepAndGetStateWithDistance(
    handle: Int,
    rounds: Int,
    outSize: CPointer<IntVar>
): CPointer<ByteVar>? {
    val engine = engines[handle]
    if (engine == null) {
        outSize.pointed.value = 0
        return null
    }
    tm.start("step.service")
    tm.start("step.compute")
    engine.stepMany(rounds)
    tm.stop("step.compute")
    val values = engine.getValues() // assumes size == nodeCount
    val nodeCount = values.size
    var totalBytes = 0
    totalBytes += 4 // nodeCount
    for (i in 0 until nodeCount) {
        val neigh = engine.getNeighborhood(i)
        totalBytes += 8 // double value
        totalBytes += 4 // neighborCount
        totalBytes += 4 * neigh.size // neighbors
    }
    val bytes = ByteArray(totalBytes)
    var offset = 0
    offset = writeIntLE(bytes, offset, nodeCount)
    for (i in 0 until nodeCount) {
        offset = writeDoubleLE(bytes, offset, values[i])
        val neigh = engine.getNeighborhood(i)
        offset = writeIntLE(bytes, offset, neigh.size)
        for (n in neigh) {
            offset = writeIntLE(bytes, offset, n)
        }
    }
    outSize.pointed.value = bytes.size
    val ptr = nativeHeap.allocArray<ByteVar>(bytes.size)
    bytes.usePinned { pinned ->
        memcpy(ptr, pinned.addressOf(0), bytes.size.convert())
    }
    tm.stop("step.service")
    return ptr
}

@OptIn(ExperimentalNativeApi::class, ExperimentalForeignApi::class)
@CName("free_state_buffer")
fun freeStateBuffer(ptr: CPointer<ByteVar>?) {
    if (ptr != null) nativeHeap.free(ptr)
}

private fun writeIntLE(buf: ByteArray, offset0: Int, v: Int): Int {
    var o = offset0
    buf[o++] = (v and 0xFF).toByte()
    buf[o++] = ((v ushr 8) and 0xFF).toByte()
    buf[o++] = ((v ushr 16) and 0xFF).toByte()
    buf[o++] = ((v ushr 24) and 0xFF).toByte()
    return o
}

private fun writeLongLE(buf: ByteArray, offset0: Int, v: Long): Int {
    var o = offset0
    buf[o++] = (v and 0xFFL).toByte()
    buf[o++] = ((v ushr 8) and 0xFFL).toByte()
    buf[o++] = ((v ushr 16) and 0xFFL).toByte()
    buf[o++] = ((v ushr 24) and 0xFFL).toByte()
    buf[o++] = ((v ushr 32) and 0xFFL).toByte()
    buf[o++] = ((v ushr 40) and 0xFFL).toByte()
    buf[o++] = ((v ushr 48) and 0xFFL).toByte()
    buf[o++] = ((v ushr 56) and 0xFFL).toByte()
    return o
}

private fun writeDoubleLE(buf: ByteArray, offset0: Int, v: Double): Int {
    val bits = v.toBits()
    return writeLongLE(buf, offset0, bits)
}
