using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Autolabor.PM1;

namespace ContinuousControl.Net
{
    class Program
    {
        [DllImport("user32.dll", EntryPoint = "GetKeyState")]
        public static extern short GetKeyState(int nVirtKey);

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
                Console.WriteLine("操作\n[方向键控制前后左右]\n[Esc键退出程序]");
                while(GetKeyState(0x1b) >= 0)
                {
                    double v = 0, w = 0;
                    bool up = GetKeyState(0x26) < 0;
                    bool down = GetKeyState(0x28) < 0;
                    bool left = GetKeyState(0x25) < 0;
                    bool right = GetKeyState(0x27) < 0;
                    if (up && !down && !left && !right)     // 前
                    {
                        v = 0.1;
                        w = 0;
                    }
                    else if (up && !down && left && !right) // 左前
                    {
                        v = 0.1;
                        w = 0.2;
                    }
                    else if (up && !down && !left && right) // 右前
                    {
                        v = 0.1;
                        w = -0.2;
                    }
                    else if (!up && down && !left && !right)// 后
                    {
                        v = -0.1;
                        w = 0;
                    }
                    else if (!up && down && left && !right) // 左后
                    {
                        v = -0.1;
                        w = -0.2;
                    }
                    else if (!up && down && !left && right) // 右后
                    {
                        v = -0.1;
                        w = 0.2;
                    }
                    else if (!up && !down && left && !right)// 逆时针原地转
                    {
                        v = 0;
                        w = 0.2;
                    }
                    else if (!up && !down && !left && right)// 顺时针原地转
                    {
                        v = 0;
                        w = -0.2;
                    }
                    Methods.VelocityTarget = (v, w);
                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("press any key to exit");
                Console.ReadKey();
            }
            finally
            {
                Methods.ShutdownSafety();                   // 断开连接
            }
        }
    }
}
