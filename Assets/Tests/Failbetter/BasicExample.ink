LIST kettleState = (cold), boiling, recently_boiled

-> start
=== start ===
Once upon a time... 

... a kettle was {kettleState}.

 * {LIST_VALUE(kettleState) == 1} Was List Value 0?

 * {kettleState == cold} Was the kettle cold?

 * {kettleState == kettleState.cold} Was the kettle kettleState.cold?

 * {kettleState != cold} Was the kettle not cold?

 * {kettleState < boiling} Was the kettle less than boiling?

 * {kettleState > cold} Was the kettle more than cold?

 * {kettleState ? (cold,boiling)} Was the kettle both cold and boiling?

- They lived happily ever after.
    -> END
