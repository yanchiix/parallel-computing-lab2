using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Globalization;

class Program
{
    static void ThreadFuncMutex(int[] arr, int start, int end, ref int max, ref int count, object mtx)
    {
        for (int i = start; i < end; i++)
        {
            lock (mtx)
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
        }
    }

    static double FindMaxSeq(int[] arr, int n)
    {
        int max = int.MinValue;
        int count = 0;
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < n; i++)
        {
            if (arr[i] > max) { max = arr[i]; count = 1; }
            else if (arr[i] == max) { count++; }
        }
        sw.Stop();
        return sw.Elapsed.TotalMilliseconds;
    }

    static double FindMaxMutex(int p, int[] arr, int n)
    {
        Thread[] threads = new Thread[p];
        int step = n / p;
        int max = int.MinValue;
        int count = 0;
        object mtx = new object();
        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < p; i++)
        {
            int s = i * step;
            int e = (i == p - 1) ? n : (i + 1) * step;
            threads[i] = new Thread(() => ThreadFuncMutex(arr, s, e, ref max, ref count, mtx));
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();
        sw.Stop();
        return sw.Elapsed.TotalMilliseconds;
    }

    static void Main()
    {
        Random rnd = new Random();
        int[] sizes = { 1000, 10000, 100000, 1000000, 10000000 };
        int p = 4;

        using (StreamWriter wr = new StreamWriter("benchmark_mutex.csv"))
        {
            wr.WriteLine("ArraySize,SeqTime,MutexTime");
            foreach (int n in sizes)
            {
                int[] arr = new int[n];
                for (int i = 0; i < n; i++) arr[i] = rnd.Next(0, n);

                double tSeq = FindMaxSeq(arr, n);
                double tMtx = FindMaxMutex(p, arr, n);
                wr.WriteLine($"{n},{tSeq.ToString(CultureInfo.InvariantCulture)},{tMtx.ToString(CultureInfo.InvariantCulture)}");
            }
        }
    }
}