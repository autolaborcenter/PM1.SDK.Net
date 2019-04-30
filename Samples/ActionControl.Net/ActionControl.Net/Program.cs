using System;
using System.Runtime.InteropServices;
using System.Threading;
using Autolabor.PM1;

namespace ActionControl.Net
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
                double progress = 0;
                string port = Methods.Initialize("", out progress); // 初始化连接
                Console.WriteLine("connected to " + port + "[PM1" +
                    (Methods.State == StateEnum.Unlocked ? "解锁]" : "锁定/错误]"));
                Console.WriteLine("动作\n" +
                    "[quit:		退出\n" +
                    " lock:		锁定\n" +
                    " unlock:	解锁\n" +
                    " front:		直行1m\n" +
                    " back:		后退1m\n" +
                    " left:		左转90°\n" +
                    " left-:		左后退90°\n" +
                    " right:		右转90°\n" +
                    " right-:	右后退90°\n" +
                    " inverse:	逆时针转90°\n" +
                    " clockwise:	顺时针转90°]");
                string cmd = string.Empty;
                while(true)
                {
                    cmd = Console.ReadLine().Trim();
                    if (cmd.Equals("quit"))         // 退出
                    {
                        break;
                    }
                    else if (cmd.Equals("lock"))    // 锁定
                    {
                        Methods.State = StateEnum.Locked;
                        while (Methods.State != StateEnum.Locked)
                        {
                            Thread.Sleep(100);
                        }
                        Console.WriteLine("[已锁定]");
                    }
                    else if (cmd.Equals("unlock"))  // 解锁
                    {
                        Methods.State = StateEnum.Unlocked;
                        while (Methods.State != StateEnum.Unlocked)
                        {
                            Thread.Sleep(100);
                        }
                        Console.WriteLine("[已解锁]");
                    }
                    else
                    {
                        progress = 0;
                        Thread thread = new Thread(() =>
                        {
                            try
                            {
                                switch (cmd)
                                {
                                    case "front":       // 直行1m
                                        Methods.GoStraight(0.1, 1, out progress);
                                        break;
                                    case "back":        // 后退1m
                                        Methods.GoStraight(-0.1, 1, out progress);
                                        break;
                                    case "left":        // 左转90°
                                        Methods.GoArcVA(0.1, 0.5, 3.14 / 2, out progress);
                                        break;
                                    case "left-":       // 左后退90°
                                        Methods.GoArcVA(-0.1, 0.5, 3.14 / 2, out progress);
                                        break;
                                    case "right":       // 右转90°
                                        Methods.GoArcVA(0.1, -0.5, 3.14 / 2, out progress);
                                        break;
                                    case "right-":      // 右后退90°
                                        Methods.GoArcVA(-0.1, -0.5, 3.14 / 2, out progress);
                                        break;
                                    case "inverse":     // 逆时针转90°
                                        Methods.TurnAround(0.25, 3.14 / 2, out progress);
                                        break;
                                    case "clockwise":   // 顺时针转90°
                                        Methods.TurnAround(-0.25, 3.14 / 2, out progress);
                                        break;
                                    default:
                                        Console.WriteLine("[unknown command]");
                                        progress = -1;
                                        break;
                                }
                            }
                            catch { }
                        });
                        if (progress < 0)
                        {
                            continue;
                        }
                        thread.IsBackground = true;
                        thread.Start();
                        Console.WriteLine("[按下Esc键可终止正在执行的动作]");
                        Console.Write("[ 0%]");
                        double last = 0;
                        while (progress < 1)
                        {
                            Thread.Sleep(100);
                            Console.Write("\b\b\b\b");
                            if ((progress - last) >= 0.01)
                            {
                                for (int i = 0; i < (int)((progress - last) / 0.01); i++)
                                {
                                    Console.Write("=");
                                }
                                last = progress;
                            }
                            Console.Write("{0:D2}%]", (int)(progress * 100));
                            if (GetKeyState(27) < 0) // Esc键
                            {
                                Methods.CancelAction();
                                Console.Write("动作已终止");
                                break;
                            }
                        }
                        thread.Join();
                        Console.WriteLine();
                    }
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
                Methods.ShutdownSafety();            // 断开连接
            }
        }
    }
}
