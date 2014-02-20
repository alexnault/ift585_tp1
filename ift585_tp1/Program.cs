using System;
using System.Collections.Generic;
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
            bool isParam = true;
            // TODO (Cisco) : Make app params work as intended
            #if (DEBUG)
                /////PROGRAM PARAMS////
                // La taille	du tampon utilisé de chaque	côté du	thread simulant le réseau (tampon d'envoi).
                int bufferLength = 3;
                // Le délai de temporisation	(time-out) de l'émetteur.
		        int timeout = 1000;
                // Le fichier à copier
                string file = "./input.txt";
                // L'emplacement de destination	pour la	copie du fichier.
                string savePath = "./blabla/out/chat.txt";
                ///////////////////////
            #else
            
            int bufferLength = 0;
            int timeout = 0;
            int windowSize = 0;
            string protocolType = "";
            string errorType = "";
            string file = "";
            string savePath = "";
            if (args.Length == 0)
            {
                Console.WriteLine("args is null"); // Check for null array
                isParam = false;
            }
            else
            {
                bufferLength = Convert.ToInt32(args[0]);
                timeout = Convert.ToInt32(args[1]);
                file = args[2];
                savePath = args[3];
                //windowSize = Convert.ToInt32(args[4]);
                //protocolType = args[5];
                //errorType = args[6];
            }
            #endif

            if (isParam)
            {
                Network network = new Network();
                EndDevice receiver = new EndDevice(network, bufferLength, null, savePath, timeout);
                EndDevice emitter = new EndDevice(network, bufferLength, file, null, timeout);


                Thread networkThread = new Thread(new ThreadStart(network.Start));
                Thread receiverThread = new Thread(new ThreadStart(receiver.Start));
                Thread emitterThread = new Thread(new ThreadStart(emitter.Start));

                networkThread.Start();
                receiverThread.Start();
                emitterThread.Start();
            }
            else
                Console.ReadLine();
        }
    }
}
