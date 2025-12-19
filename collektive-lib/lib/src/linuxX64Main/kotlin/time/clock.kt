package time

import kotlin.time.TimeSource

actual fun nanoTime(): Long = TimeSource.Monotonic.markNow().elapsedNow().inWholeNanoseconds