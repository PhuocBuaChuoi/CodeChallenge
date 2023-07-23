using ChessChallenge.API;
using System.Collections.Generic;
using System;

    public class MyBot : IChessBot
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
            
            int[] pawnwf = { 0, 0, 0, 0, 0, 0, 0, 0 }; // to check Double/Triple pawn =))))
            int[] pawnbf = { 0, 0, 0, 0, 0, 0, 0, 0 };
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
                        if (y.IsWhite)
                        {
                            pawnwf[y.Square.File] += 1;
                        }
                        else
                        {
                            pawnbf[y.Square.File] += 1;
                        }
                    }
                }
            }
        foreach (int x in pawnwf)
        {
            m += (Math.Max(x, 1) - 1) * 50;
        }
        foreach (int x in pawnbf)
        {
            m -= (Math.Max(x, 1) - 1) * 50;
        }
        return m;
        }
        public KeyValuePair<Move, int> Minimax(Board board, int depth = 0, int __alpha__ = -1000000, int __beta__ = 1000000)
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
                int d = Maxplay ? -10000000 : 10000000;
                Move move = Move.NullMove;
                foreach (Move x in board.GetLegalMoves())
                {
                    Board m = board;
                    m.MakeMove(x);
                    int k = Minimax(m, depth - 1).Value;
                    m.UndoMove(x);
                if (Maxplay)
                {
                    __alpha__ = Math.Max(__alpha__, k);
                    if (__beta__ <= __alpha__)
                    {
                        break;
                    }
                    if (d < k)
                    {
                        d = k;
                        move = x;
                    }
                }
                else
                {
                    __beta__ = Math.Min(__beta__, k);
                    if (__alpha__ >= __beta__)
                    {
                        break;
                    }
                    if (d > k)
                    {
                        d = k;
                        move = x;
                    }
                }

            }
            return new KeyValuePair<Move, int>(move, d);
            }
        }
        public Move Think(Board board, Timer timer)
        {
            Move move = Minimax(board, 3).Key;
            return move;
        }
    }