package time

import kotlinx.cinterop.ExperimentalForeignApi
import kotlinx.cinterop.addressOf
import kotlinx.cinterop.convert
import kotlinx.cinterop.usePinned
import platform.posix.fclose
import platform.posix.fflush
import platform.posix.fopen
import platform.posix.fwrite

@OptIn(ExperimentalForeignApi::class)
class NativeCsvFileSink(
    path: String = "native.csv",
    private val flushEvery: Int = 200
) : SampleSink {

    @OptIn(ExperimentalForeignApi::class)
    private val file = fopen(path, "w")
    private var count: Int = 0

    private val t0 = nanoTime()

    init {
        require(file != null) { "Cannot open file: $path" }
        writeLine("t_ns,id,duration_ns\n")
        fflush(file)
    }

    override fun onSample(id: String, tNs: Long, durationNs: Long) {
        val relT = (nanoTime() - t0)
        writeLine("$relT,$id,$durationNs\n")
        count++
        if (count % flushEvery == 0) fflush(file)
    }

    fun close() {
        if (file != null) {
            fflush(file)
            fclose(file)
        }
    }

    private fun writeLine(s: String) {
        val bytes = s.encodeToByteArray()
        bytes.usePinned { pinned ->
            fwrite(pinned.addressOf(0), 1.convert(), bytes.size.convert(), file)
        }
    }
}