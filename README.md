# Chess Engine

`ParallelMiniMaxPlayer.cs` contains the class of interest in this project.

### My contributions:
- Parallel processing
- Negamax
- Alpha-beta pruning
- Transposition Table
- An evaluation function (a very incomplete one)

### [rudzen/ChessLib](https://github.com/rudzen/ChessLib)'s contributions:
- move generation
- board representation (bitboards)

### TODO
- Make the eval function at least somewhat decent
- Fix castling being detected as illegal move

Parallelism comes by evaluating each of the immediate child nodes of a given position with staggered threads:
For instance, if I had 4 possible moves and 2 threads, thread 0 would evaluate moves 0 and 2, while thread 1 would evaluate moves 1 and 3.

I commented out En Passant code in the lib due to apparent bugs, so this version of chess doesn't have en passant. Most games never have an en passant move anyway so not a big deal