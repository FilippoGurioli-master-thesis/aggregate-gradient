package simple.gradient

import it.unibo.collektive.aggregate.api.Aggregate
import it.unibo.collektive.aggregate.api.share
import kotlin.Double.Companion.POSITIVE_INFINITY

fun Aggregate<Node>.gradient(source: Boolean): Double =
    share(POSITIVE_INFINITY) { field ->
        if (source) return@share 0.0

        val best = field.neighbors
            .list
            .minOfOrNull { (nbr, nbrGrad) ->
                if (nbrGrad.isInfinite()) POSITIVE_INFINITY
                else nbrGrad + Position.distance(localId.position, nbr.position)
            }
        best ?: POSITIVE_INFINITY
    }
