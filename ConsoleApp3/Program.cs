using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

public class Game
{
    private static readonly Dictionary<int, string> moves = new Dictionary<int, string>();
    private static readonly Dictionary<string, int> indexMap = new Dictionary<string, int>();
    private static readonly Dictionary<string, string> results = new Dictionary<string, string>();
    private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

    static void Main(string[] args)
    {
        if (args.Length < 3 || args.Length % 2 == 0 || args.Length != args.Distinct().Count())
        {
            Console.WriteLine("Error: Incorrect number of unique arguments. You must provide an odd number of unique moves.");
            return;
        }

        for (int i = 0; i < args.Length; i++)
        {
            moves[i + 1] = args[i];
            indexMap[args[i]] = i + 1;
        }

        GenerateTable();

        byte[] key = GenerateKey(32);
        byte[] computerMove = GenerateMove();
        byte[] hmac = GenerateHMAC(computerMove, key);

        Console.WriteLine("HMAC: " + ByteArrayToString(hmac));
        ShowMenu();

        int userMoveIndex;
        while (true)
        {
            string userInput = Console.ReadLine();
            if (userInput == "?")
            {
                Console.WriteLine("\nResults are from the user's perspective:");
                ShowHelpTable();
                ShowMenu();
                continue;
            }
            else if (!int.TryParse(userInput, out userMoveIndex) || userMoveIndex < 0 || userMoveIndex > moves.Count)
            {
                Console.WriteLine("Invalid input. Please enter a valid move (1-{0}) or '0' to exit.", moves.Count);
                ShowMenu();
                continue;
            }

            if (userMoveIndex == 0)
            {
                return;
            }

            string userMove = moves[userMoveIndex];
            Console.WriteLine("Your move: " + userMove);
            Console.WriteLine("Computer move: " + moves[computerMove[0] % moves.Count + 1]);

            string result = results[GetResultKey(userMoveIndex, computerMove[0] % moves.Count + 1)];
            Console.WriteLine(result);
            Console.WriteLine("HMAC key: " + ByteArrayToString(key));
            break;
        }
    }

    private static void GenerateTable()
    {
        int n = moves.Count;
        string[] movesArray = new string[n];
        moves.Values.CopyTo(movesArray, 0);

        for (int i = 0; i < n; i++)
        {
            string move1 = movesArray[i];
            for (int j = 0; j < n; j++)
            {
                string move2 = movesArray[j];
                int compare = (n + j - i) % n;
                if (compare == 0)
                {
                    results[GetResultKey(i + 1, j + 1)] = "Draw!";
                }
                else if (compare <= n / 2)
                {
                    results[GetResultKey(i + 1, j + 1)] = "Win!";
                }
                else
                {
                    results[GetResultKey(i + 1, j + 1)] = "Lose!";
                }
            }
        }
    }

    private static string GetResultKey(int move1Index, int move2Index)
    {
        return $"{moves[move1Index]} vs {moves[move2Index]}";
    }

    private static void ShowMenu()
    {
        Console.WriteLine("Available moves:");
        foreach (var move in moves)
        {
            Console.WriteLine($"{move.Key} - {move.Value}");
        }
        Console.WriteLine("0 - exit");
        Console.WriteLine("? - help");
        Console.WriteLine("Enter your move: ");
    }

    private static void ShowHelpTable()
    {
        int n = moves.Count;
        string[] movesArray = new string[n];
        moves.Values.CopyTo(movesArray, 0);

        int[] columnWidths = new int[n + 1]; 
        columnWidths[0] = Math.Max("v PC\\User >".Length, movesArray.Max(m => m.Length));
        for (int i = 0; i < n; i++)
        {
            columnWidths[i + 1] = Math.Max(movesArray[i].Length, results.Values.Max(r => r.Length));
        }

        Console.Write("+");
        for (int i = 0; i <= n; i++)
        {
            Console.Write(new string('-', columnWidths[i] + 2) + "+");
        }
        Console.WriteLine();

        Console.Write("| ");
        Console.Write("v PC\\User >".PadRight(columnWidths[0]));
        Console.Write(" |");
        for (int i = 0; i < n; i++)
        {
            Console.Write($" {movesArray[i].PadRight(columnWidths[i + 1])} |");
        }
        Console.WriteLine();

        Console.Write("+");
        for (int i = 0; i <= n; i++)
        {
            Console.Write(new string('-', columnWidths[i] + 2) + "+");
        }
        Console.WriteLine();

        for (int i = 0; i < n; i++)
        {
            Console.Write("| ");
            Console.Write(movesArray[i].PadRight(columnWidths[0]));
            Console.Write(" |");
            for (int j = 0; j < n; j++)
            {
                Console.Write($" {results[GetResultKey(i + 1, j + 1)].PadRight(columnWidths[j + 1])} |");
            }
            Console.WriteLine();
        }

        Console.Write("+");
        for (int i = 0; i <= n; i++)
        {
            Console.Write(new string('-', columnWidths[i] + 2) + "+");
        }
        Console.WriteLine();
    }

    private static byte[] GenerateKey(int length)
    {
        byte[] key = new byte[length];
        rng.GetBytes(key);
        return key;
    }

    private static byte[] GenerateMove()
    {
        byte[] move = new byte[1];
        rng.GetBytes(move);
        return move;
    }

    private static byte[] GenerateHMAC(byte[] data, byte[] key)
    {
        using (var hmac = new HMACSHA256(key))
        {
            return hmac.ComputeHash(data);
        }
    }

    private static string ByteArrayToString(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }
}