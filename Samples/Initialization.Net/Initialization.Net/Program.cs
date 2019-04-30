using System;
using System.Threading;
using Autolabor.PM1;

namespace Initialization.Net
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("initializing...");
                double progress;
                string port = Methods.Initialize("", out progress); // 初始化连接
                Console.WriteLine("connected to " + port);
                Methods.State = StateEnum.Unlocked;             // 解锁
                while (Methods.State != StateEnum.Unlocked)
                {
                    Thread.Sleep(100);
                }
                Console.WriteLine("moving...");
                Methods.TurnAround(0.25, 1.57, out progress);   // 以0.25rad/s的速度原地转90°
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Methods.ShutdownSafety();                       // 断开连接
            }
            Console.WriteLine("press any key to exit");
            Console.ReadKey();
        }
    }
}
