using ChessChallenge.API;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        public int AnlBoard(Board board)
        {
            if (board.IsInCheckmate())
            {
                return board.IsWhiteToMove ? -100000 : 100000;
            }
            if (board.IsDraw())
            {
                return 0;
            }
            int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
            int m = 0;
            int[] pawnvalue = {0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5, -5,-10,  0,  0,-10, -5,  5,
            5, 10, 10,-20,-20, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0};
            int[] pawnwr = { 0, 0, 0, 0, 0, 0, 0, 0 }; // to check Double/Triple pawn =))))
            foreach (PieceList x in board.GetAllPieceLists())
            {
                if (x.Count == 0)
                {
                    continue;
                }
                foreach (Piece y in x)
                {
                    m += pieceValues[(int)y.PieceType] * (y.IsWhite ? 1 : -1);
                    if (y.IsPawn)
                    {
                        m += pawnvalue[(System.Math.Abs((y.IsWhite ? 7 : 0) - y.Square.Rank) * 8) + y.Square.File] * (y.IsWhite ? 1 : -1);
                        pawnwr[y.Square.File] += (1 * (y.IsWhite ? 1 : -1));
                    }
                }
            }
            foreach (int x in pawnwr)
            {
                m += Math.Max(x, 0) * 50;
            }
            return m;
        }
        public KeyValuePair<Move, int> Minimax(Board board, int depth = 0)
        {
            if (depth == 0)
            {
                int d = board.IsWhiteToMove ? -1000000 : 1000000;
                Move move = Move.NullMove;
                foreach (Move x in board.GetLegalMoves())
                {
                    Board m = board;
                    m.MakeMove(x);
                    int k = AnlBoard(m);
                    m.UndoMove(x);
                    if (board.IsWhiteToMove)
                    {

                        if (d < k)
                        {
                            d = k;
                            move = x;
                        }
                    }
                    else
                    {
                        if (d > k)
                        {
                            d = k;
                            move = x;
                        }
                    }

                }
                return new KeyValuePair<Move, int>(move, d);
            }
            else
            {
                bool Maxplay = board.IsWhiteToMove;
                int d = Maxplay ? -1000000 : 1000000;
                Move move = Move.NullMove;
                foreach (Move x in board.GetLegalMoves())
                {
                    Board m = board;
                    m.MakeMove(x);
                    int k = Minimax(m, depth - 1).Value;
                    if (Maxplay)
                    {

                        if (d < k)
                        {
                            d = k;
                            move = x;
                        }
                    }
                    else
                    {
                        if (d > k)
                        {
                            d = k;
                            move = x;
                        }
                    }
                    m.UndoMove(x);
                }
                return new KeyValuePair<Move, int>(move, d);
            }
        }
        public Move Think(Board board, Timer timer)
        {
            Move move = Minimax(board, 2).Key;
            return move;
        }
    }
}