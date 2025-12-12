package org.example

import simple.gradient.CollektiveEngine
import simple.gradient.CollektiveEngineWithDistance
import simple.gradient.Position
import simple.gradient.jvmCreate
import simple.gradient.jvmDestroy
import simple.gradient.jvmGetValue
import simple.gradient.jvmSetSource
import simple.gradient.jvmStep
import kotlin.random.Random

fun main4() {
    val engine = CollektiveEngine(10)

    engine.setSource(0, true)

    println("Connections of source (0): ${engine.getNeighborhood(0)}")
}

fun main2() {
    val engine = CollektiveEngine(10)

    engine.setSource(0, true)

    repeat(10) { round ->
        println("Round $round")
        engine.stepOnce()
        (0 until 10).forEach { id -> println("$id -> ${engine.getValue(id)}")
        }
        println()
    }
}

fun main3() {
    val handle = jvmCreate(3, 3)
    jvmSetSource(handle, 0,true)
    for (r in 0 until 1)
    {
        println("Round $r")
        jvmStep(handle, 1)
        for (id in 0 until 3)
        {
            val value = jvmGetValue(handle, id)
            println("  Device $id -> $value")
        }
        println()
    }

    jvmDestroy(handle)
}

fun main() {
    val nodeCount = 10
    val maxDistance = 4.0
    val engine = CollektiveEngineWithDistance(nodeCount, maxDistance)
    engine.setSource(0, true)
    (0 until nodeCount).forEach { id ->
        engine.updateNodePosition(id, Position(0.0, maxDistance * id, 0.0))
    }

    repeat(10) { round ->
        println("Round $round")
        engine.stepOnce()
        (0 until nodeCount).forEach { id -> println("$id -> ${engine.getValue(id)}") }
        println()
    }

    println("New positions:")
    val rng = Random(42)
    (0 until nodeCount).forEach { id ->
        val x = rng.nextInt(10)
        val y = rng.nextInt(10)
        println("$id -> $x, $y")
        engine.updateNodePosition(id, Position(x.toDouble(), y.toDouble(), 0.0))
    }

    repeat(10) { round ->
        println("Round $round")
        engine.stepOnce()
        (0 until nodeCount).forEach { id -> println("$id -> ${engine.getValue(id)}") }
        println()
    }
}
