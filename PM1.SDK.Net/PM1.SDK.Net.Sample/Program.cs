using System;

namespace Autolabor.PM1.Sample {
    class Program {
        static void Main() {
            Console.WriteLine("id\tv\tw\tr\ts\ta\tt\t形态\t速度\t尺度");
            for(var i = 0; i < 64; ++i) {
                Console.Write("{0}\t", i);
                for (var j = 0; j < 6; ++j)
                    Console.Write("{0}\t", ((i >> j) & 1) == 1 ? '*' : ' ');
                Console.WriteLine();
            }
        }
    }
}
