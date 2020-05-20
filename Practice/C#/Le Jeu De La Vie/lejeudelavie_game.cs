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
	private int width;
	private int height;
	private int[,] table;
	
	public int Width => width;
	public int Height => height;
	public int[,] Table => table;
	
	public Game(int w, int h)
	{
		this.width = w;
		this.height = h;
		this.table = new int[h,w];
	}
	
	public void SetLine(int h, string line)
	{
		char[] charLine = line.ToCharArray();
		if (charLine.Length <= table.GetLength(1))
		{
			for (int i = 0; i < charLine.Length; i++)
			{
				table[h,i] = int.Parse(charLine[i].ToString());
			}
		}
	}
	
	public string Result()
	{
		string res = "";
		
		for (int i = 0; i < table.GetLength(0); i++)
		{
			for (int j = 0; j < table.GetLength(1); j++)
			{
				res += table[i,j].ToString();
			}
			res += "\n";
		}
		
		return res;
	}
}