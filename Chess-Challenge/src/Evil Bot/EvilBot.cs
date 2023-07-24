using System;
using ChessChallenge.API;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
namespace ChessChallenge.Example {

	public class EvilBot : IChessBot
	{
        static public int getdist(int a, int b)
        {
            return Math.Max(a, b);
        }
        Dictionary<ulong, int> TransportationTable = new Dictionary<ulong, int>();
        public IEnumerable<Move> MoveOrder(Board board)
        {
            Move[] Order = board.GetLegalMoves();
            IEnumerable<Move> afterorder;
            if (board.IsWhiteToMove)
            {
                afterorder = from x in Order
                             orderby MoveOrderPoint(board, x) descending
                             select x;
            }
            else
            {
                afterorder = from x in Order
                             orderby MoveOrderPoint(board, x)
                             select x;
            }
            return afterorder;
        }
        public int MoveOrderPoint(Board board, Move move)
        {
            int k = 0;
            if (move.IsCastles)//castle always better =))
            {
                k += 10;
            }
            if (move.IsPromotion)//promotion always better =)))
            {
                k += 15;
            }
            board.MakeMove(move);
            k += AnlBoard(board);
            board.UndoMove(move);
            return k;
        }
        public int QuieSearch(Board board, int alpha=int.MinValue, int beta=int.MaxValue, int MdepSearch = 2)
        {
            int k = Eval(board);
            Move[] CaptureMove = board.GetLegalMoves(true);
            if (CaptureMove.Length == 0 || MdepSearch == 0)
            {
                return k;
            }
            else
            {
                for (int i = 0; i < CaptureMove.Length; i++)
                {
                    board.MakeMove(CaptureMove[i]);
                    int value = (!board.IsWhiteToMove) ? QuieSearch(board, alpha, beta, MdepSearch - 1) : -QuieSearch(board, -beta, -alpha, MdepSearch - 1);
                    board.UndoMove(CaptureMove[i]);

                    if (!board.IsWhiteToMove)
                    {
                        k = Math.Max(k, value);
                        alpha = Math.Max(alpha, k);
                    }
                    else
                    {
                        k = Math.Min(k, value);
                        beta = Math.Min(beta, k);
                    }

                    if (alpha >= beta)
                    {
                        break;
                    }
                }
                return k;
            }
        }

        public int Eval(Board board)
        {
            if (Is_EndGame(board))
            {
                return AnlEndgame(board);
            }
            else
            {
                return AnlBoard(board);
            }
        }
        public bool Is_EndGame(Board board)
        {
            int b = 0; int w = 0;
            foreach (PieceList x in board.GetAllPieceLists())
            {
                if (x.Count == 0)
                {
                    continue;
                }
                else
                {
                    if (x.TypeOfPieceInList == PieceType.Knight || x.TypeOfPieceInList == PieceType.Bishop || x.TypeOfPieceInList == PieceType.Rook || x.TypeOfPieceInList == PieceType.Queen)
                    {
                        if (x.IsWhitePieceList)
                        {
                            w += x.Count;
                        }
                        else
                        {
                            b += x.Count;
                        }
                    }
                }
            }
            bool k = (b <= 2) || (w <= 2);
            return k;
        }
        public int AnlEndgame(Board board)
        {
            if (board.IsInCheckmate())
            {
                return board.IsWhiteToMove ? -100000 : 100000;
            }
            if (board.IsDraw())
            {
                return 0;
            }
            int[] pieceValues = { 0, 150, 300, 350, 500, 900, 10000 };
            int m = 0;
            int[] pawnvalue = {0,  0,  0,  0,  0,  0,  0,  0,
            30, 50, 60, 60, 60, 60, 50, 30,
            20, 25, 30, 40, 40, 30, 25, 20,
            10,  15, 15, 20, 20, 15,  15,  10,
            0,  0,  0, 0, 0,  0,  0,  0,
            -5, -5,-5,  -5,  -5,-5, -5,  -5,
            -10, -10, -10,-10,-10, -10, -10,  -10,
            0,  0,  0,  0,  0,  0,  0,  0};
            int[] kingvalue = {-50,-40,-30,-20,-20,-30,-40,-50,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-30,  0,  0,  0,  0,-30,-30,
            -50,-30,-30,-30,-30,-30,-30,-50};
            int[] queenvalue = {-20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -5,  0,  2,  2,  2,  2,  0, -5,
            -2,  0,  2,  2,  2,  2,  -2, -5,
            -10,  3,  3,  3,  3,  3,  0,-10,
            -10,  0,  5, 5,  5,  0,  0,-10,
            -20,-10,-10, 5, 5,-10,-10,-20};
            int[] rookvalue = {  0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  10,  10,  0,  0, -5,
            0,  0,  0,  10,  10,  0,  0,  0};
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
                    if (y.IsRook)
                    {
                        m += rookvalue[(System.Math.Abs((y.IsWhite ? 7 : 0) - y.Square.Rank) * 8) + y.Square.File] * (y.IsWhite ? 1 : -1);

                    }
                    if (y.IsQueen)
                    {
                        m += queenvalue[(System.Math.Abs((y.IsWhite ? 7 : 0) - y.Square.Rank) * 8) + y.Square.File] * (y.IsWhite ? 1 : -1);
                    }
                    if (y.IsKing)
                    {
                        m += kingvalue[(System.Math.Abs((y.IsWhite ? 7 : 0) - y.Square.Rank) * 8) + y.Square.File] * (y.IsWhite ? 1 : -1);
                    }
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
        public int AnlBoard(Board board)
        {
            if (board.IsInCheckmate())
            {
                return board.IsWhiteToMove ? -100000 : 100000;
            }
            if (board.IsDraw())
            {
                if (board.PlyCount < 40)
                {
                    return -100;//avoid soon draw
                }
                return 0;
            }
            int[] pieceValues = { 0, 100, 330, 300, 500, 900, 10000 };
            int m = 0;
            //castleable => better
            m += (board.HasKingsideCastleRight(true) ? 5 : 0) + (board.HasKingsideCastleRight(false) ? 5 : 0) +
            (board.HasQueensideCastleRight(true) ? 5 : 0) + (board.HasQueensideCastleRight(false) ? 5 : 0);
            int[] pawnvalue = {0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5, 10, 25, 25, 10,  5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5, -5,-10,  0,  0,-10, -5,  5,
            5, 10, 10,-20,-20, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0};
            int[] kingvalue = {-30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-50,-60,-60,-50,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
            10, 20,  -5,  -10,  -10,  -5, 20, 20,
            20, 30, 20,  -5,  -5, -5, 30, 20};
            int[] queenvalue = {-20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -5,  0,  2,  2,  2,  2,  0, -5,
            -2,  0,  2,  2,  2,  2,  -2, -5,
            -10,  3,  3,  3,  3,  3,  0,-10,
            -10,  0,  5, 5,  5,  0,  0,-10,
            -20,-10,-10, 5, 5,-10,-10,-20};
            int[] rookvalue = {  0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  10,  10,  0,  0, -5,
            0,  0,  0,  10,  10,  0,  0,  0};
            int[] bishopvalue = {-20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,};
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
                    if (y.IsBishop)
                    {
                        m += bishopvalue[(System.Math.Abs((y.IsWhite ? 7 : 0) - y.Square.Rank) * 8) + y.Square.File] * (y.IsWhite ? 1 : -1);
                    }
                    if (y.IsRook)
                    {
                        m += rookvalue[(System.Math.Abs((y.IsWhite ? 7 : 0) - y.Square.Rank) * 8) + y.Square.File] * (y.IsWhite ? 1 : -1);

                    }
                    if (y.IsQueen)
                    {
                        m += queenvalue[(System.Math.Abs((y.IsWhite ? 7 : 0) - y.Square.Rank) * 8) + y.Square.File] * (y.IsWhite ? 1 : -1);
                    }
                    if (y.IsKing)
                    {
                        m += kingvalue[(System.Math.Abs((y.IsWhite ? 7 : 0) - y.Square.Rank) * 8) + y.Square.File] * (y.IsWhite ? 1 : -1);
                    }
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
        public KeyValuePair<Move, int> Minimax(Board board, int depth = 0, int alpha = int.MinValue, int beta = int.MaxValue)
        {
            if (depth == 0)
            {
                int d = board.IsWhiteToMove ? int.MinValue : int.MaxValue;
                Move move = Move.NullMove;
                foreach (Move x in board.GetLegalMoves())
                {
                    Board m = board;
                    m.MakeMove(x);
                    int k = QuieSearch(m);
                    m.UndoMove(x);
                    if (board.IsWhiteToMove)
                    {
                        if (d < k)
                        {
                            d = k;
                            move = x;
                        }
                        alpha = Math.Max(alpha, d);
                    }
                    else
                    {
                        if (d > k)
                        {
                            d = k;
                            move = x;
                        }
                        beta = Math.Min(beta, d);
                    }

                    if (alpha >= beta)
                        break;
                }
                return new KeyValuePair<Move, int>(move, d);
            }
            else
            {
                bool Maxplay = board.IsWhiteToMove;
                int d = Maxplay ? int.MinValue : int.MaxValue;
                Move[] list = MoveOrder(board).ToArray();
                Move move;
                try
                {
                    move = list[0];
                }
                catch
                {
                    move = Move.NullMove;
                }
                foreach (Move x in list)
                {
                    Board m = board;
                    m.MakeMove(x);
                    if (m.IsInCheckmate()) { m.UndoMove(x); return new KeyValuePair<Move, int>(x, Maxplay ? -100000 : 100000); }
                    int k;
                    if (!TransportationTable.TryGetValue(board.ZobristKey, out k))
                    {
                        k = Minimax(m, depth - 1, alpha, beta).Value;
                    }
                    if (Maxplay)
                    {
                        if (d < k)
                        {
                            d = k;
                            move = x;
                        }
                        alpha = Math.Max(alpha, d);
                    }
                    else
                    {
                        if (d > k)
                        {
                            d = k;
                            move = x;
                        }
                        beta = Math.Min(beta, d);
                    }
                    m.UndoMove(x);

                    if (alpha >= beta)
                        break;
                }
                return new KeyValuePair<Move, int>(move, d);
            }
        }
        public Move Think(Board board, Timer timer)
        {
            KeyValuePair<Move, int> res = Minimax(board, 2);
            if (!TransportationTable.TryAdd(board.ZobristKey, res.Value))
            {
                TransportationTable[board.ZobristKey] = res.Value;
            }
            Move move = res.Key;
            Console.WriteLine(res.Value);
            return move;
        }
    }
}