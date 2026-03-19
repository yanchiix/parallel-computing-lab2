using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Globalization;

class Program
{
    static void ThreadFuncAtomic(int[] array, int startIndex, int finalIndex, ref int largestNumber, ref int count)
    {
        for (int i = startIndex; i < finalIndex; i++)
        {
            while (true)
            {
                int prevLargestNumber = Volatile.Read(ref largestNumber);

                if (array[i] > prevLargestNumber)
                {
                    if (Interlocked.CompareExchange(ref largestNumber, array[i], prevLargestNumber) == prevLargestNumber)
                    {
                        Interlocked.Exchange(ref count, 1);
                        break;
                    }
                }
                else if (array[i] == prevLargestNumber)
                {
                    Interlocked.Increment(ref count);
                    break;
                }
                else
                {
                    break;
                }
            }
        }
    }

    static void ThreadFuncMutex(int[] array, int startIndex, int finalIndex, ref int largestNumber, ref int count, object gateMutex)
    {
        for (int i = startIndex; i < finalIndex; i++)
        {
            lock (gateMutex)
            {
                if (array[i] > largestNumber)
                {
                    largestNumber = array[i];
                    count = 1;
                }
                else if (array[i] == largestNumber)
                {
                    count++;
                }
            }
        }
    }

    static double DefFunc(int[] array, int arrLen)
    {
        int largestNumber = int.MinValue;
        int countNum = 0;

        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < arrLen; i++)
        {
            if (array[i] > largestNumber)
            {
                largestNumber = array[i];
                countNum = 1;
            }
            else if (array[i] == largestNumber)
            {
                countNum++;
            }
        }

        sw.Stop();
        Console.WriteLine($"single:\tlargest: {largestNumber}\tcount: {countNum}\ttime: {sw.Elapsed.TotalMilliseconds:F4} ms");
        return sw.Elapsed.TotalMilliseconds;
    }

    static double ThreadsAtomic(int threadNum, int[] array, int arrLen)
    {
        Thread[] threads = new Thread[threadNum];
        int numsInThread = arrLen;

        if (threadNum > 1)
        {
            numsInThread = arrLen / threadNum;
        }

        int largestNum = int.MinValue;
        int countNum = 0;

        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < threadNum; i++)
        {
            int start = i * numsInThread;
            int end = (i == threadNum - 1) ? arrLen : (i + 1) * numsInThread;

            threads[i] = new Thread(() => ThreadFuncAtomic(array, start, end, ref largestNum, ref countNum));
            threads[i].Start();
        }

        for (int i = 0; i < threadNum; i++)
        {
            threads[i].Join();
        }

        sw.Stop();
        Console.WriteLine($"atomic:\tlargest: {largestNum}\tcount: {countNum}\ttime: {sw.Elapsed.TotalMilliseconds:F4} ms");
        return sw.Elapsed.TotalMilliseconds;
    }

    static double ThreadsMutex(int threadNum, int[] array, int arrLen)
    {
        Thread[] threads = new Thread[threadNum];
        int numsInThread = arrLen;
        object gateMutex = new object();

        if (threadNum > 1)
        {
            numsInThread = arrLen / threadNum;
        }

        int largestNum = int.MinValue;
        int countNum = 0;

        Stopwatch sw = Stopwatch.StartNew();


        for (int i = 0; i < threadNum; i++)
        {
            int start = i * numsInThread;
            int end = (i == threadNum - 1) ? arrLen : (i + 1) * numsInThread;

            threads[i] = new Thread(() => ThreadFuncMutex(array, start, end, ref largestNum, ref countNum, gateMutex));
            threads[i].Start();
        }

        for (int i = 0; i < threadNum; i++)
        {
            threads[i].Join();
        }

        sw.Stop();
        Console.WriteLine($"mutex:\tlargest: {largestNum}\tcount: {countNum}\ttime: {sw.Elapsed.TotalMilliseconds:F4} ms");
        return sw.Elapsed.TotalMilliseconds;
    }

    static void Main()
    {
        Random rand = new Random();
        
        int[] arraySizes = { 1000, 10000, 100000, 1000000, 10000000 };
        int fixedThreads = 4;

        using (StreamWriter writer = new StreamWriter("benchmark_sizes.csv"))
        {
            writer.WriteLine("ArraySize,SeqTime,AtomicTime,MutexTime");

            foreach (int n in arraySizes)
            {
                Console.WriteLine($"\n--- Testing array size: {n} ---");
                
                int[] arr = new int[n];
                for (int i = 0; i < n; i++)
                {
                    arr[i] = rand.Next(0, n);
                }

                double seqTime = DefFunc(arr, n);
                double atomicTime = ThreadsAtomic(fixedThreads, arr, n);
                double mutexTime = ThreadsMutex(fixedThreads, arr, n);

                writer.WriteLine($"{n},{seqTime.ToString(CultureInfo.InvariantCulture)},{atomicTime.ToString(CultureInfo.InvariantCulture)},{mutexTime.ToString(CultureInfo.InvariantCulture)}");
            }
        }
        
        Console.WriteLine("\nDone! Results saved to 'benchmark_sizes.csv'");
    }
}
