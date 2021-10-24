using Rudz.Chess;
using Rudz.Chess.Enums;
using Rudz.Chess.Types;
using Rudz.Chess.MoveGeneration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rudz.Chess.Factories;

namespace MultiChessConsole
{
    internal class ParallelMiniMaxPlayer : IPlayer
    {
        public Players color; // white or black
        Players IPlayer.color { get => color; set => color = value; }

        public int total_depth;
        public int num_threads;

        // some diagnostics
        private int abPrunes = 0;
        private int nodesEvaluated = 0;
        private int entriesRemoved = 0;

        // last ply which was a point of no return (capture or pawn move)
        private int pnrPly = 0;

        // transposition table (https://www.chessprogramming.org/Transposition_Table)
        private Dictionary<State, PositionData> tt = new Dictionary<State, PositionData>();

        // control access to the transposition table; only one thread at a time can write to it
        private static Mutex tt_mut = new Mutex();

        // enables waiting until unused positions are cleared from the transposition table to compute a move
        private static Mutex pnr_mut = new Mutex();

        public ParallelMiniMaxPlayer(Players color, int total_depth = 1, int num_threads = 1)
        {
            this.color = color;
            this.total_depth = total_depth;
            this.num_threads = num_threads;
        }

        public Move nextMove(Position pos, State state)
        {
            // wait for unneeded TT entries to be cleared.
            pnr_mut.WaitOne();
            // if we just had a point of no return move, set pnrPly to the current ply.
            if (pos.State.Rule50 < 2) pnrPly = pos.Ply;
            else Console.WriteLine("pos.State.Rule50 isn't 0 I guess");

            var validMoves = pos.GenerateMoves();
            int[] evals = new int[validMoves.Length];

            // create threads
            Task[] threads = new Task[num_threads];

            // spin them off
            for(int i = 0; i < num_threads; i++)
            {
                threads[i] = Task.Run(() => {
                    //Console.WriteLine($"spinning off thread {i}");
                    miniMaxStarter(pos, state, validMoves, evals, i);
                });
                Thread.Sleep(10); // otherwise the tasks get confused about what i is (they all end up with i=6)
            }

            // wait for them
            Task.WaitAll(threads);

            // find out what we've calculated is the best move
            int best_idx = 0;
            if (pos.SideToMove.IsWhite)
            {
                for (int i = 0; i < evals.Length; i++) if (evals[i] > evals[best_idx]) best_idx = i;
            }
            else
            {
                for (int i = 0; i < evals.Length; i++) if (evals[i] < evals[best_idx]) best_idx = i;
            }


            // only show diagnostics if single-threaded (otherwise diagnostics will be corrupted)
            if(num_threads == 1)
            {
                Console.WriteLine($"Total alpha-beta prunes: {abPrunes}");
                Console.WriteLine($"Total nodes evaluated: {nodesEvaluated}");
            }
            else
            {
                Console.WriteLine("No diagnostics for multi-threaded execution.");
            }

            pnr_mut.ReleaseMutex();

            Task.Run(() => { clearOldTTEntries(); });

            return validMoves[best_idx];
        }

        private void clearOldTTEntries()
        {
            // for each position in the transposition table, if it came before the last point of no return, remove it.
            // to check this we compare the ply (half-move) of the last PNR to the ply of the position
            pnr_mut.WaitOne();
            foreach(var entry in tt)
            {
                if (entry.Value.ply < pnrPly) 
                {
                    entriesRemoved++;
                    tt.Remove(entry.Key);
                }
            }
            Console.WriteLine($"Removed {entriesRemoved} entries from the transposition table; tt.Count now {tt.Count}.");
            pnr_mut.ReleaseMutex();
        }

        private void miniMaxStarter(Position pos, State state, MoveList validMoves, int[] evals, int thread_id)
        {
            if (validMoves.Length == 0) return; // checkmate or stalemate

            // stagger threads to take care of every num_thread-th move
            for (int i = thread_id; i < validMoves.Length;  i += num_threads)
            {
                Move move = validMoves[i];
                //Console.WriteLine($"Thread {thread_id} is evaluating move {move} which is at position {i}...");

                // generate child node
                var game = GameFactory.Create(pos.FenNotation);
                State simState = new State(state);
                Position simPos = (Position)game.Pos;
                simPos.MakeMove(move, simState);

                // evaluate node
                evals[i] = miniMax(simPos, simState, total_depth - 1, -Evaluate.W_INFINITE, Evaluate.W_INFINITE);
            }
        }

        private int miniMax(Position pos, State state, int depth, int alpha, int beta)
        {
            // base case
            if (depth == 0)
            {
                nodesEvaluated++;
                return Evaluate.Eval(pos);
            }

            // generate moves; this could be supplemented by the transposition table in the future
            MoveList validMoves = pos.GenerateMoves();

            if (validMoves.Length == 0) // checkmate or stalemate
            {
                if (pos.InCheck) return pos.SideToMove.IsBlack ? Evaluate.W_INFINITE : -Evaluate.W_INFINITE; // checkmate
                else return 0; // stalemate
            }

            // bestScore initializes to the worst possible score for whomever is moving
            int bestScore = pos.SideToMove.IsBlack ? Evaluate.W_INFINITE : -Evaluate.W_INFINITE;

            // generate all child nodes (before evaluating any of them
            Position[] simPoses = new Position[validMoves.Length];
            for (int i = 0; i < validMoves.Length; i++)
            {
                Move move = validMoves[i];
                var game = GameFactory.Create(pos.FenNotation);
                State simState = new State(state);
                Position simPos = (Position)game.Pos;

                simPos.MakeMove(move, simState);

                simPoses[i] = simPos;
            }

            // sort child nodes based on what we already know to improve a-b pruning
            if (pos.SideToMove.IsWhite)
                Array.Sort(simPoses, whiteComparePositions);
            else
                Array.Sort(simPoses, blackComparePositions);


            // evaluate the child nodes with minimax
            foreach (Position simPos in simPoses)
            {
                int tempEval = miniMax(simPos, new State(simPos.State), depth - 1, alpha, beta); // recurse

                // update tt
                tt_mut.WaitOne();
                // critical section
                if (tt.ContainsKey(simPos.State) && tt[simPos.State].evalDepth < depth)
                {
                    tt[simPos.State].eval = tempEval;
                    tt[simPos.State].evalDepth = depth;
                }
                else if(!tt.ContainsKey(simPos.State))
                    tt[simPos.State] = new PositionData(tempEval, depth, simPos.Ply);
                // end of critical section
                tt_mut.ReleaseMutex();
                

                // alpha-beta pruning
                if (pos.SideToMove.IsWhite)
                {
                    // update best score
                    if (tempEval > bestScore) bestScore = tempEval;
                    if (tempEval > alpha) alpha = tempEval;
                }
                else
                {
                    // update best score
                    if (tempEval < bestScore) bestScore = tempEval;
                    if (tempEval < beta) beta = tempEval;
                }

                // try to prune
                if (beta <= alpha)
                {
                    abPrunes++;
                    break; 
                }
            }

            return bestScore;
        }

        // essential a Compare() override for Position so they can be sorted
        // according to what we know in the transposition table
        private int variableComparePositions(Position x, Position y, Players us)
        {
            bool ttHasX = tt.ContainsKey(x.State);
            bool ttHasY = tt.ContainsKey(y.State);
            int result = 0;

            // ascending order
            if (ttHasX && ttHasY) result = tt[x.State].eval - tt[y.State].eval;
            else if (ttHasX) result = -1;
            else if (ttHasY) result = 1;

            if (us == Players.White) result = -result; // descending order if White

            return result;
        }

        private int blackComparePositions(Position x, Position y)
        {
            return variableComparePositions(x, y, Players.Black);
        }

        private int whiteComparePositions(Position x, Position y)
        {
            return variableComparePositions(x, y, Players.White);
        }
    }
}