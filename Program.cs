using System;
using Rudz.Chess;
using Rudz.Chess.Fen;
using Rudz.Chess.Types;
using Rudz.Chess.Enums;
using Rudz.Chess.Factories;
using Rudz.Chess.MoveGeneration;
using System.Collections.Generic;

namespace MultiChessConsole
{
    class Program
    {
        static Random rand = new Random();
        static void Main()
        {
            // initialize the game with a FEN (https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation)
            var fen = TestFens.DEFAULT;
            var game = GameFactory.Create(fen);
            var state = new State();

            // print the initial position
            Console.WriteLine(((Position)game.Pos).ToString());
            Console.WriteLine("Evaluation: " + Evaluate.Eval((Position)game.Pos));

            // run the game
            GameEndTypes result = pvpGame((Position)game.Pos, state, 
                new HumanConsolePlayer(Players.White), new ParallelMiniMaxPlayer(Players.Black, 4, 4));

            // handle the end result
            if (result == GameEndTypes.CheckMate)
            {
                string winner = ((Position)game.Pos).SideToMove.IsWhite ? "Black" : "White";
                Console.WriteLine($"{winner} wins by checkmate!");
            }
            else if (result == GameEndTypes.StaleMate) Console.WriteLine("Draw by stalemate.");
            else Console.WriteLine("Game over.");
        }

        public static GameEndTypes pvpGame(Position pos, State state, IPlayer p0, IPlayer p1, bool verbose = false)
        {
            if (pos.SideToMove.IsBlack && p0.color == Players.Black || pos.SideToMove.IsWhite && p0.color == Players.White)
            {
                Move move = p0.nextMove(pos, state);
                if(move == Move.EmptyMove)
                    return pos.InCheck ? GameEndTypes.CheckMate : GameEndTypes.StaleMate;
                pos.MakeMove(move, state);
                Console.WriteLine(pos.ToString());
                if (verbose)
                {
                    Console.WriteLine($"rule50: {pos.State.Rule50}");
                    Console.WriteLine($"ply: {pos.Ply}");
                    Console.WriteLine("Evaluation: " + Evaluate.Eval(pos) + "\n");
                }
            }

            // game loop
            while (true)
            {
                Move m1 = p1.nextMove(pos, state);
                if (m1 == Move.EmptyMove)
                    return pos.InCheck ? GameEndTypes.CheckMate : GameEndTypes.StaleMate;
                pos.MakeMove(m1, state);
                Console.WriteLine(pos.ToString());
                if (verbose)
                {
                    Console.WriteLine($"rule50: {pos.State.Rule50}");
                    Console.WriteLine($"ply: {pos.Ply}");
                    Console.WriteLine("Evaluation: " + Evaluate.Eval(pos) + "\n");
                }

                Move m0 = p0.nextMove(pos, state);
                if (m0 == Move.EmptyMove)
                    return pos.InCheck ? GameEndTypes.CheckMate : GameEndTypes.StaleMate;
                pos.MakeMove(m0, state);
                Console.WriteLine(pos.ToString());
                if (verbose)
                {
                    Console.WriteLine($"rule50: {pos.State.Rule50}");
                    Console.WriteLine($"ply: {pos.Ply}");
                    Console.WriteLine("Evaluation: " + Evaluate.Eval(pos) + "\n");
                }
            }
        }
    }
}
