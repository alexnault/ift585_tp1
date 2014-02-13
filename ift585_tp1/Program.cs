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
            // TODO (Cisco) : Make app params work as intended

            /////PROGRAM PARAMS////
            // La taille	du tampon utilisé de chaque	côté du	thread simulant le réseau (tampon d'envoi).
            int bufferLength = 1;
            // Le délai de temporisation	(time-out) de l'émetteur.
		    int timeout = 1000;
            // Le fichier à copier
            string file = "./in/input.txt";
            // L'emplacement de destination	pour la	copie du fichier.
            string savePath = "./blabla/out/chat.txt";
            ///////////////////////

            Network network = new Network();
            EndDevice receiver = new EndDevice(network, bufferLength, file, null, timeout);
            EndDevice emitter = new EndDevice(network, bufferLength, null, savePath, timeout);

            Thread networkThread = new Thread(new ThreadStart(network.Start));
            Thread receiverThread = new Thread(new ThreadStart(receiver.Start));
            Thread emitterThread = new Thread(new ThreadStart(emitter.Start));

            networkThread.Start();
            receiverThread.Start();
            emitterThread.Start();
        }
    }
}
