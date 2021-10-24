using Rudz.Chess;
using Rudz.Chess.Enums;
using Rudz.Chess.Types;
using Rudz.Chess.MoveGeneration;
using System;
using System.Collections.Generic;
using System.Threading;
using Rudz.Chess.Factories;

namespace MultiChessConsole
{
    class SimpleMinimaxPlayer : IPlayer
    {
        public Players color;
        Players IPlayer.color { get => color; set => color = value; }

        public int total_depth;

        public SimpleMinimaxPlayer(Players color, int total_depth = 1)
        {
            this.color = color;
            this.total_depth = total_depth;
        }

        public Move nextMove(Position pos, State state)
        {
            return simpleMiniMaxStarter(pos, state);
        }

        private Move simpleMiniMaxStarter(Position pos, State state)
        {
            var validMoves = pos.GenerateMoves();

            if (validMoves.Length == 0) return Move.EmptyMove; // checkmate or stalemate

            int best_score = -Evaluate.W_INFINITE;
            if (color == Players.Black) best_score = Evaluate.W_INFINITE;
            Move best_move = validMoves[0];

            foreach (Move move in validMoves)
            {
                var game = GameFactory.Create(pos.FenNotation);
                State simState = new State();
                if (state.NonPawnMaterial != null) state.CopyTo(simState);
                Position simPos = (Position)game.Pos;

                simPos.MakeMove(move, simState);

                int temp_score = simpleMiniMax(simPos, simState, total_depth, -Evaluate.W_INFINITE, Evaluate.W_INFINITE);

                if ((color == Players.White && temp_score > best_score) || temp_score < best_score)
                {
                    best_score = temp_score;
                    best_move = move;
                }
            }

            return best_move;
        }

        private int simpleMiniMax(Position pos, State state, int depth, int alpha, int beta)
        {
            var validMoves = pos.GenerateMoves();
            if (validMoves.Length == 0) return pos.SideToMove.IsBlack ? Evaluate.W_INFINITE : -Evaluate.W_INFINITE;
            if (depth == 0) return Evaluate.Eval(pos);

            int best_score;

            if (pos.SideToMove.IsWhite)
            {
                best_score = -Evaluate.W_INFINITE;

                foreach (Move move in validMoves)
                {
                    var game = GameFactory.Create(pos.FenNotation);
                    State simState = new State();
                    if (state.NonPawnMaterial != null) state.CopyTo(simState);
                    Position simPos = (Position)game.Pos;

                    simPos.MakeMove(move, simState);

                    int temp_score = simpleMiniMax(simPos, simState, depth - 1, alpha, beta); // recurse

                    if (temp_score > best_score) best_score = temp_score;

                    if (temp_score > alpha) alpha = temp_score;
                    if (beta <= alpha) break;
                }

                return best_score;
            }
            else // pos.SideToMove.IsBlack
            {
                best_score = Evaluate.W_INFINITE;

                foreach (Move move in validMoves)
                {
                    var game = GameFactory.Create(pos.FenNotation);
                    State simState = new State();
                    if (state.NonPawnMaterial != null) state.CopyTo(simState);
                    Position simPos = (Position)game.Pos;

                    simPos.MakeMove(move, simState);

                    int temp_score = simpleMiniMax(simPos, simState, depth - 1, alpha, beta); // recurse

                    if (temp_score < best_score) best_score = temp_score;

                    if (temp_score < beta) beta = temp_score;
                    if (beta <= alpha) break;
                }

                return best_score;
            }
        }

        private Move onePlySearch(Position pos, State state)
        {
            var validMoves = pos.GenerateMoves();

            if (validMoves.Length == 0) return Move.EmptyMove; // checkmate or stalemate

            int best_score = -Evaluate.W_INFINITE;
            if (color == Players.Black) best_score = Evaluate.W_INFINITE;
            Move best_move = validMoves[0];

            foreach (Move move in validMoves)
            {
                var game = GameFactory.Create(pos.FenNotation);
                State simState = new State();
                if (state.NonPawnMaterial != null) state.CopyTo(simState);
                Position simPos = (Position)game.Pos;

                simPos.MakeMove(move, simState);

                const string moveToCheck = "";

                if (move.ToString() == moveToCheck)
                {
                    Console.WriteLine(simPos.ToString());
                }

                int temp_score = Evaluate.Eval(simPos);

                if ((color == Players.White && temp_score > best_score) || temp_score < best_score)
                {
                    best_score = temp_score;
                    best_move = move;
                }
            }

            return best_move;
        }
    }
}
