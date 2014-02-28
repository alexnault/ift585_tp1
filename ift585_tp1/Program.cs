using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ift585_tp1
{
    class Program
    {
        static void Main(string[] args)
        {
            int bufferLength = 0;
            int timeout = 0;
            int errorType = 0;
            int protocolType = 0;
            string file = "";
            string savePath = "";

#if (DEBUG)

            if (args.Length < 6)
            {
                Console.WriteLine("Not enough args set. Needs 6."); // Check for null array

            }
            else
            {
                bufferLength = Convert.ToInt32(args[0]);
                timeout = Convert.ToInt32(args[1]);
                file = args[2];
                savePath = args[3];
                protocolType = Convert.ToInt32(args[4]);
                errorType = Convert.ToInt32(args[5]);
            }
#else
            const string f = "./config.txt";
            int ctrParam = 0;
            List<string> lines = new List<string>();
            using (StreamReader r = new StreamReader(f))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    lines.Add(line);
                    ctrParam++;
                }
            }

            if (ctrParam < 6)
            {
                Console.WriteLine("Not enough args set. Needs 6."); // Check for null array

            }
            else
            {
                bufferLength = Convert.ToInt32(lines[0]);
                timeout = Convert.ToInt32(lines[1]);
                file = lines[2];
                savePath = lines[3];
                protocolType = Convert.ToInt32(lines[4]);
                errorType = Convert.ToInt32(lines[5]);
            }
#endif
            Network network = new Network(errorType, timeout);
            EndDevice receiver = new EndDevice(network, bufferLength, null, savePath, timeout, protocolType);
            EndDevice emitter = new EndDevice(network, bufferLength, file, null, timeout, protocolType);


            Thread networkThread = new Thread(new ThreadStart(network.Start));
            Thread receiverThread = new Thread(new ThreadStart(receiver.Start));
            Thread emitterThread = new Thread(new ThreadStart(emitter.Start));

            networkThread.Start();
            receiverThread.Start();
            emitterThread.Start();
        }
    }
}
