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
        string[] inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        
        Game play = new Game(width, height);
        
        for (int i = 0; i < height; i++)
        {
            string line = Console.ReadLine();
            play.SetLine(i, line);
        }

        // Write an answer using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

		play.NextState();

        Console.WriteLine(play.Result());
    }
}

interface IGame
{
	int Width { get;}
	int Height { get; }
	int[,] Table { get; }
}

class Game : IGame
{
	private int _width;
	private int _height;
	private int[,] _table;
	private int[,] _newTable;
	
	public int Width => _width;
	public int Height => _height;
	public int[,] Table => _table;
	
	public Game(int w, int h)
	{
		_width = w;
		_height = h;
		_table = new int[h,w];
		_newTable = new int[h,w];
	}
	
	public void SetLine(int h, string line)
	{
		char[] charLine = line.ToCharArray();
		if (charLine.Length <= _table.GetLength(1))
		{
			for (int i = 0; i < charLine.Length; i++)
			{
				_table[h,i] = int.Parse(charLine[i].ToString());
			}
		}
	}

	public void NextState()
	{
		int indCol = 0;
		int indLine = 0;

		for (int i = 0; i < _table.GetLength(0); i++)
		{
			for (int j = 0; j < _table.GetLength(1); j++)
			{
				int neighbours = GetNeighbours(i, j);

				if (_table[i, j] == 1 && (neighbours < 2 || neighbours > 3))
					_newTable[i, j] = 0;
				else if (neighbours == 3 && _table[i, j] == 0)
					_newTable[i, j] = 1;
				else
					_newTable[i, j] = _table[i, j];
			}
		}
	}

	public int GetNeighbours(int indCol, int indLine)
	{
		int cellAlive = 0;
		// Check previous line !
		if (indCol - 1 >= 0)
		{
			if (indLine - 1 >= 0)
			{
				if (IsAlive(indCol - 1, indLine - 1))
					cellAlive++;
			}
			
			if (IsAlive(indCol - 1, indLine))
				cellAlive++;
			
			if (indLine + 1 < _table.GetLength(1))
			{
				if (IsAlive(indCol - 1, indLine + 1))
					cellAlive++;
			}
		}

		// Check current line !
		if (indLine - 1 >= 0)
		{
			if (IsAlive(indCol, indLine - 1))
				cellAlive++;
		}

		if (indLine + 1 < _table.GetLength(1))
		{
			if (IsAlive(indCol, indLine + 1))
				cellAlive++;
		}

		// Check next line ! 
		if (indCol + 1 < _table.GetLength(0))
		{
			if (indLine - 1 >= 0)
			{
				if (IsAlive(indCol + 1, indLine - 1))
					cellAlive++;
			}
			
			if (IsAlive(indCol + 1, indLine))
				cellAlive++;
			
			if (indLine + 1 < _table.GetLength(1))
			{
				if (IsAlive(indCol + 1, indLine + 1))
					cellAlive++;
			}
		}

		return cellAlive;
	}

	public bool IsAlive(int indCol, int indLine)
	{
		// Console.Error.WriteLine($"{indCol} {indLine}");
		return _table[indCol, indLine] == 1;
	}
	
	public string Result()
	{
		string res = "";
		
		for (int i = 0; i < _newTable.GetLength(0); i++)
		{
			for (int j = 0; j < _newTable.GetLength(1); j++)
			{
				res += _newTable[i,j].ToString();
			}
			res += "\n";
		}
		
		return res;
	}
}