using System;

namespace Autolabor.PM1.Sample {
    class Program {
        static void Main() {
            try {
                Console.WriteLine(Methods.Initialize("", new Config { }, out _));
                Methods.Shutdown();
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}
