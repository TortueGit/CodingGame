using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{
    static void Main(string[] args)
    {
        Game myGame = new Game();
        for (int i = 0; i < 8; i++)
        {
            string boardRow = Console.ReadLine();
            myGame.AddBoardRow(i, boardRow);

            if (boardRow.Contains('K'))
                myGame.WhiteKingPos = new Tuple<int, int>(i, boardRow.IndexOf('K'));
            
            if (boardRow.Contains('k'))
                myGame.BlackKingPos = new Tuple<int, int>(i, boardRow.IndexOf('k'));
        }
        char result = 'N';
        // Write an answer using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        if (myGame.IsKingInChessMat(true) && myGame.IsInChessPos(myGame.WhiteKingPos, true))
            result = 'B';
        
        if (myGame.IsKingInChessMat(false) && myGame.IsInChessPos(myGame.BlackKingPos, false))
            result = 'W';

        Console.WriteLine(result);
    }
}

class Game
{
    private string[] _boardChess;
    private Tuple<int, int> _whiteKingPos;
    private Tuple<int, int> _blackKingPos;

    public Game()
    {
        _boardChess = new string[8];
        _whiteKingPos = new Tuple<int, int>(-1, -1);
        _blackKingPos = new Tuple<int, int>(-1, -1);
    }

    public string[] BoardChess => _boardChess;

    public Tuple<int, int> WhiteKingPos
    {
        get => _whiteKingPos;
        set => _whiteKingPos = value;
    }

    public Tuple<int, int> BlackKingPos
    {
        get => _blackKingPos;
        set => _blackKingPos = value;
    }

    public void AddBoardRow(int indexRow, string row)
    {
        _boardChess[indexRow] = row;
    }

    public bool IsInChessPos(Tuple<int, int> pos, bool isWhite)
    {
        int indCol = pos.Item1;
        int indLine = pos.Item2;

        if (IsPawnTargetingPos(indCol, indLine, isWhite))
            return true;
        if (IsRookTargetingPos(indCol, indLine, isWhite))
            return true;
        if (IsBishopTargetingPos(indCol, indLine, isWhite))
            return true;
        if (IsKnightTargeting(indCol, indLine, isWhite))
            return true;
        if (IsKingTargettingPos(indCol, indLine, isWhite))
            return true;
        if (IsRookTargetingPos(indCol, indLine, isWhite, true) || IsBishopTargetingPos(indCol, indLine, isWhite, true))
            return true;

        return false;
    }

    public bool IsPawnTargetingPos(int indCol, int indLine, bool isWhite)
    {
        char pawn = 'p';
        List<Tuple<int, int>> pos = new List<Tuple<int, int>>();
        if (isWhite)
        {
            if (indCol - 1 < 0)
                return false;

            if (indLine - 1 > 0)
                pos.Add(new Tuple<int, int>(indCol - 1, indLine - 1));
            if (indLine + 1 < _boardChess[indCol - 1].Length)
                pos.Add(new Tuple<int, int>(indCol - 1, indLine + 1));
        }

        if (!isWhite)
        {
            if (indCol + 1 >= _boardChess.GetLength(0))
                return false;

            if (indLine - 1 > 0)
                pos.Add(new Tuple<int, int>(indCol + 1, indLine - 1));
            if (indLine + 1 < _boardChess[indCol + 1].Length)
                pos.Add(new Tuple<int, int>(indCol + 1, indLine + 1));
        }

        foreach (Tuple<int, int> p in pos)
        {
            char c = (char)_boardChess[p.Item1][p.Item2];
            if (IsExpectedCharInPos(c, pawn, isWhite))
                return true;
        }

        return false;
    }

    public bool IsRookTargetingPos(int indCol, int indLine, bool isWhite, bool isQueen = false)
    {
        char rook = (isQueen) ? 'q' : 'r';

        for (int i = indCol - 1; i >= 0; i--)
        {
            char c = (char)_boardChess[i][indLine];
            if (IsDifferentExpectedCharInPos(c, rook, isWhite))
                break;
            
            if (IsExpectedCharInPos(c, rook, isWhite))
                return true;
        }

        for (int i = indCol + 1; i < _boardChess.GetLength(0); i++)
        {
            char c = (char)_boardChess[i][indLine];
            if (IsDifferentExpectedCharInPos(c, rook, isWhite))
                break;
            
            if (IsExpectedCharInPos(c, rook, isWhite))
                return true;
        }

        for (int i = indLine - 1; i > 0; i--)
        {
            char c = (char)_boardChess[indCol][i];
            if (IsDifferentExpectedCharInPos(c, rook, isWhite))
                break;
            
            if (IsExpectedCharInPos(c, rook, isWhite))
                return true;
        }

        for (int i = indLine + 1; i < _boardChess[indCol].Length; i++)
        {
            char c = (char)_boardChess[indCol][i];
            if (IsDifferentExpectedCharInPos(c, rook, isWhite))
                break;
            
            if (IsExpectedCharInPos(c, rook, isWhite))
                return true;
        }

        return false;
    }

    public bool IsKnightTargeting(int indCol, int indLine, bool isWhite)
    {
        char knight = 'n';
        List<Tuple<int, int>> positions = new List<Tuple<int, int>>();
        int mini = -2;
        int maxi = 2;
        int minj = -2;
        int maxj = 2;

        if (indCol == 0)
            mini = 0;
        else if (indCol - 1 == 0)
            mini = -1;
        
        if (indCol == _boardChess.GetLength(0)-1)
            maxi = 0;
        else if (indCol + 1 == _boardChess.GetLength(0)-1)
            maxi = 1;

        if (indLine == 0)
            minj = 0;
        else if (indLine - 1 == 0)
            minj = -1;

        if (indLine == _boardChess[indCol].Length-1)
            maxj = 0;
        else if (indLine + 1 == _boardChess[indCol].Length-1)
            maxj = 1;
        
        for (int i = mini; i <= maxi; i++)
        {
            if (i == 0)
                continue;

            for (int j = minj; j <= maxj; j++)
            {
                if (j == 0)
                    continue;
                if (Math.Abs(j) == Math.Abs(i))
                    continue;
                
                if (IsExpectedCharInPos((char)_boardChess[indCol + i][indLine + j], knight, isWhite))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsBishopTargetingPos(int indCol, int indLine, bool isWhite, bool isQueen = false)
    {
        char bishop = (isQueen) ? 'q' : 'b';
        int index = 1;
                
        for (int i = indCol-1; i >= 0; i--)
        {
            if (indLine - index < 0)
                break;

            char c = (char)_boardChess[i][indLine - index];
            if (IsDifferentExpectedCharInPos(c, bishop, isWhite))
                break;
            if (IsExpectedCharInPos(c, bishop, isWhite))
                return true;
            
            index++;
        }

        index = 1;
        for (int i = indCol + 1; i < _boardChess.GetLength(0); i++)
        {
            if (indLine + i > _boardChess[i].Length-1)
                break;

            char c = (char)_boardChess[i][indLine + index];
            if (IsDifferentExpectedCharInPos(c, bishop, isWhite))
                break;

            if (IsExpectedCharInPos(c, bishop, isWhite))
                return true;
            index++;
        }

        index = 1;
        for (int i = indCol - 1; i >= 0; i--)
        {
            if (indLine + index > _boardChess[i].Length-1)
                break;

            char c = (char)_boardChess[i][indLine + index];
            if (IsDifferentExpectedCharInPos(c, bishop, isWhite))
                break;
            if (IsExpectedCharInPos(c, bishop, isWhite))
                return true;
            index++;
        }

        index = 1;
        for (int i = indCol + 1; i < _boardChess.GetLength(0); i++)
        {
            if (indLine - i < 0)
                break;

            char c = (char)_boardChess[i][indLine - index];
            if (IsDifferentExpectedCharInPos(c, bishop, isWhite))
                break;
            if (IsExpectedCharInPos(c, bishop, isWhite))
                return true;

            index++;
        }

        return false;
    }

    public bool IsKingTargettingPos(int indCol, int indLine, bool isWhite)
    {
        char king = 'k';
        int mini = -1;
        int minj = -1;
        int maxi = 1;
        int maxj = 1;
        
        if (indCol == 0)
            mini = 0;
        else if (indCol == _boardChess.GetLength(0)-1)
            maxi = 0;
        if (indLine == 0)
            minj = 0;
        else if (indLine == _boardChess[indCol].Length-1)
            maxj = 0;

        for (int i = mini; i <= maxi; i++)
        {
            for (int j = minj; j <= maxj; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                char c = (char)_boardChess[indCol + i][indLine + j];
                if (IsExpectedCharInPos(c, king, isWhite))
                    return true;
            }
        }

        return false;
    }

    public bool IsDifferentExpectedCharInPos(char c, char expectedChar, bool isWhite)
    {
        char toFind = (isWhite) ? char.ToLower(expectedChar) : char.ToUpper(expectedChar);

        if (c != '.' && c != toFind && c != ((isWhite) ? 'K' : 'k'))
            return true;
        
        return false;
    }

    public bool IsExpectedCharInPos(char c, char expectedChar, bool isWhite)
    {
        char toFind = (isWhite) ? char.ToLower(expectedChar) : char.ToUpper(expectedChar);
                
        if (c != '.' && c == toFind)
            return true;
        
        return false;
    }

    public bool IsKingInChessMat(bool isWhite)
    {
        int kingColPos = (isWhite) ? _whiteKingPos.Item1 : _blackKingPos.Item1;
        int kingLinePos = (isWhite) ? _whiteKingPos.Item2 : _blackKingPos.Item2;
        List<Tuple<int, int>> positionsForMove = new List<Tuple<int, int>>();
        int minIndCol = -1;
        int minIndLine = -1;
        int maxIndCol = 1;
        int maxIndLine = 1;

        if (kingColPos == 0)
            minIndCol = 0;
        else if (kingColPos == _boardChess.GetLength(0)-1)
            maxIndCol = 0;
        
        if (kingLinePos == 0)
            minIndLine = 0;
        else if (kingLinePos == _boardChess[kingColPos].Length-1)
            maxIndLine = 0;

        for (int i = minIndCol; i <= maxIndCol; i++)
        {
            for (int j = minIndLine; j <= maxIndLine; j++)
            {
                if ((char)_boardChess[kingColPos+i][kingLinePos+j] == '.' || 
                    ((isWhite) ? char.IsLower((char)_boardChess[kingColPos+i][kingLinePos+j]) : char.IsUpper((char)_boardChess[kingColPos+i][kingLinePos+j])))
                {
                    positionsForMove.Add(new Tuple<int, int>(kingColPos+i, kingLinePos+j));
                }
            }
        }


        foreach (Tuple<int, int> pos in positionsForMove)
        {
            if (!IsInChessPos(pos, isWhite))
                return false;
        }

        return true;
    }
}