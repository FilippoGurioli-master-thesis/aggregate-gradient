package simple.gradient

import it.unibo.collektive.Collektive

class CollektiveEngine(val nodeCount: Int, private val maxDegree: Int = 3) {
    private val networkManager = NetworkManager(maxDegree)

    // mutable set of sources (you can control this from C#)
    val sources = mutableSetOf<Int>()

    private val devices: List<Collektive<Int, Int>>
    private val lastValues: IntArray

    init {
        lastValues = IntArray(nodeCount) { Int.MAX_VALUE }

        devices = (0 until nodeCount).map { id ->
            val network = SimpleNetwork(networkManager, id)
            Collektive(id, network) {
                // This block is the aggregate program: executed on each cycle()
                val value = hopCountGradient(localId in sources)
                lastValues[localId] = value
                value
            }
        }
    }

    fun setSource(nodeId: Int, isSource: Boolean) {
        if (isSource) sources.add(nodeId) else sources.remove(nodeId)
    }

    fun clearSources() {
        sources.clear()
    }

    fun stepOnce() {
        // one aggregate round
        devices.reversed().forEach { it.cycle() }
    }

    fun stepMany(rounds: Int) {
        repeat(rounds) { stepOnce() }
    }

    fun getValue(nodeId: Int): Int = lastValues[nodeId]
}