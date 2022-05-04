using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PR3
{
    class Threader
    {
        public Thread thread;
        public static bool shutDown;
        public static int ConsumersAlive;
    }

    class Producer : Threader
    {
        int ProducerNumber;
        public Queue<int> producerq = new();
        Random randomNubmer = new();
        bool sleep;

        public Producer(int num)
        {
            ProducerNumber = num;
            sleep = false;
            shutDown = false;
            thread = new Thread(Manufacte);
            thread.Start();
        }
        private void Manufacte()
        {
        prod:
            if (!sleep)
            {
                int num = randomNubmer.Next(1, 100);
                lock (Program.q)
                    Program.q.Enqueue(num);
                lock (producerq)
                    producerq.Enqueue(num);
                if (Program.q.Count >= 100)
                {
                    sleep = true;
                }
            }
            else
            {
                if (Program.q.Count <= 80) sleep = false;     
            }

            try
            {
                Thread.Sleep(500);
            }
            catch
            {
                return;
            }

            goto prod;
        }
    }

    class Consumer : Threader
    {
        int ConsumerNumber;
        public Queue<int> consumerq = new();

        public Consumer(int num)
        {
            ConsumerNumber = num;
            ConsumersAlive = 2;
            thread = new Thread(Consume);
            thread.Start();
        }
        private void Consume()
        {
        Cons:
            lock (Program.q)
            {
                if (Program.q.Count > 0)
                {
                    lock (consumerq)
                        consumerq.Enqueue(Program.q.Dequeue());
                }
                else if (shutDown)
                {
                    ConsumersAlive--;
                    return;
                }
            }
            Thread.Sleep(500);
            goto Cons;
        }
    }

    class Program
    {
        public static Queue<int> q = new();

        public async static void Print()
        {
            Queue<int> temp;
        Print:
            Console.Clear();
            lock (q) temp = q;
            lock (temp)
            foreach (var num in temp)
            {
                Console.Write(num + " ");
            }
            Console.WriteLine();
            Console.WriteLine("\nq - остановить производство.");
            await Task.Delay(500);
            if (Threader.ConsumersAlive == 0) {
                Console.Clear();
                return;
            } 
            goto Print;
        }

        static void Main(string[] args)
        {
            Thread print = new(Print);
            Threader[] Economy =
            {
                new Producer(1),
                new Producer(2),
                new Producer(3),
                new Consumer(1),
                new Consumer(2)
            };

            print.Start();

            ConsoleKey key = Console.ReadKey().Key;
            if (key == ConsoleKey.Q)
            {
                for (int i = 0; i < 3; i++)
                {
                    Economy[i].thread.Interrupt();
                    Threader.shutDown = true;
                }
            }
        }
    }
}
