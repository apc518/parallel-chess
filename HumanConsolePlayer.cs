namespace MultiChessConsole
{
    using Rudz.Chess;
    using Rudz.Chess.Types;
    using Rudz.Chess.Enums;
    using Rudz.Chess.MoveGeneration;
    using System;
    internal class HumanConsolePlayer : IPlayer
    {
        static Random rand = new Random();
        public Players color;
        Players IPlayer.color { get => color; set => color = value; }

        public HumanConsolePlayer(Players color)
        {
            this.color = color;
        }

        public Move nextMove(Position pos, State state)
        {
            var validMoves = pos.GenerateMoves();

            if (validMoves.Length == 0) return Move.EmptyMove; // checkmate or stalemate

            while (true)
            {
                // get command
                var cmd = Console.ReadLine();

                if (cmd == "exit") Environment.Exit(0);

                Move move;

                // try to parse the move, if the parse fails or the move is invalid we query the user again
                try
                {
                    Square orig = new Square(cmd[1] - 49, cmd[0] - 97);
                    Square dest = new Square(cmd[3] - 49, cmd[2] - 97);
                    if (cmd.Length > 4) // castling
                    {
                        // there appears to be a library bug that prevents castling from actually working...
                        move = Move.Create(orig, dest, MoveTypes.Castling);
                    }
                    else
                    {
                        move = Move.Create(orig, dest);
                    }

                    ExtMove m = (ExtMove)move;

                    // handle an illegal move
                    if (!validMoves.Contains(m)) throw new Exception("Illegal Move.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Invalid: " + e.Message);
                    continue;
                }

                return move;
            }
        }
    }
}