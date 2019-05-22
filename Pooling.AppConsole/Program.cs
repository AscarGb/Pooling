using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pooling.AppConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("press to start");
            Console.ReadLine();

            var bigListPool = new Pool<List<long>>(Creator, Clearer, 10);

            Task.Run(async () =>
            {
                List<Task> tl = new List<Task>();

                for (int i = 0; i < 100; i++)
                {
                    var task = Task.Run(async () =>
                    {
                        using (var pooledList = bigListPool.Rent())
                        {                            
                            var list = pooledList.item;
                            await Task.Delay(1000);                           
                        }
                    });

                    tl.Add(task);
                }

                await Task.WhenAll(tl);

                Console.WriteLine(bigListPool.Count());
            });

            Console.ReadLine();
        }

        static List<long> Creator() => new List<long>(1024 * 1024);


        static void Clearer(List<long> l) => l.ForEach(i => i = 0);
    }

}
