using System;
using System.Diagnostics;

internal static class Program
{
    private static void FindMaxSeq(int[] arr)
    {
        int max = int.MinValue;
        int count = 0;

        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] > max)
            {
                max = arr[i];
                count = 1;
            }
            else if (arr[i] == max)
            {
                count++;
            }
        }

        sw.Stop();

        Console.WriteLine($"seq:\tmax: {max}\tcount: {count}\ttime: {sw.Elapsed.TotalSeconds:F6}");
    }

    private static void Main()
    {
        Random rnd = new Random();

        int n = 1_000_000;
        int[] arr = new int[n];

        for (int i = 0; i < n; i++)
        {
            arr[i] = rnd.Next(0, 100000);
        }

        Console.WriteLine($"array length: {n}");
        FindMaxSeq(arr);
    }
}