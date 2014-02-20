using HammingCode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Thanks to Maciej Lis for his Hamming Code.
//http://maciejlis.com/hamming-code-algorithm-c-sharp/

// We edited it so it fits our code and meet our needs.

namespace ift585_tp1.Hamming
{
    class Hamming
    {
        public static Binary AddHamming(Frame frame)
        {
            BitArray bits = new BitArray(frame.toBytes());

            Binary binary = toBinary(bits);

            int columnsAmount = binary.Count();
            int rowsAmount = (int)Math.Ceiling(Math.Log(columnsAmount, 2) + 1);
            BinaryMatrix H = GenerateHMatrix(rowsAmount, columnsAmount);

            Binary verification = GenerateVerificationBits(H, binary);
            Binary binaryFrame = Binary.Concatenate(binary, verification);

            /*
            Console.WriteLine("Binary = {0}", binary);
            Console.WriteLine("H matrix:");
            Console.Write(H);
            Console.WriteLine("Verification = {0}", verification);
            Console.WriteLine("Output Frame = {0}", binaryFrame);
            Console.WriteLine();
            */
             
            return binaryFrame;
        }

        public static Frame RemoveHamming(Binary binaryFrame)
        {
            Binary receivedFrame = new Binary(binaryFrame.ToArray());

            int columnsAmount = binaryFrame.Count();
            int rowsAmount = (int)Math.Ceiling(Math.Log(columnsAmount, 2) + 1);
            BinaryMatrix H = GenerateHMatrix(rowsAmount, columnsAmount);
            

            /*if (corruptMessage.Value) // TODO remove maybe
            {
                int badBit = random.Next(0, receivedFrame.Length - 1);
                receivedFrame[badBit] = !receivedFrame[badBit];
            }*/
            Binary receivedMessage = new Binary(receivedFrame.Take(columnsAmount));
            Binary receivedVerification = new Binary(receivedFrame.Skip(columnsAmount));

            H = GenerateHMatrix(rowsAmount, columnsAmount);
            Binary receivedMessageVerification = GenerateVerificationBits(H, receivedMessage);
            Binary s = receivedVerification ^ receivedMessageVerification;
            /*
            Console.WriteLine("received frame = {0}", receivedFrame);
            Console.WriteLine("H matrix:");
            Console.Write(H);
            Console.WriteLine("received message verification: {0}", receivedMessageVerification);
            Console.WriteLine("s value: {0}", s);
            */
            BitArray test = toBitArray(receivedMessage);

            byte[] bytes = new byte[test.Length];
            test.CopyTo(bytes, 0);

            return new Frame(bytes);
        }

        private static Binary toBinary(BitArray bits)
        {
            List<bool> list = new List<bool>();

            foreach (bool bit in bits)
            {
                list.Add(bit);
            }

            return new Binary(list);
        }

        private static BitArray toBitArray(Binary binarys)
        {
            bool[] bits = new bool[binarys.Length];

            for (int i = 0; i < binarys.Length; i++)
            {
                bits[i] = binarys[i];
            }

            return new BitArray(bits);
        }

        private static int FindFaultyBit(BinaryMatrix H, Binary s)
        {
            for (int i = 0; i < H.ColumnAmount; i++)
            {
                Binary column = H.GetColumn(i);
                Binary check = s ^ column;
                if (check.Any(b => b))
                    continue;
                return i;
            }

            throw new WarningException("Faulty bit not found!");
        }

        private static BinaryMatrix GenerateHWithIdentity(BinaryMatrix H)
        {
            BinaryMatrix HWithIdentity = new BinaryMatrix(H.RowAmount, H.ColumnAmount + H.RowAmount);
            for (int y = 0; y < H.RowAmount; y++)
            {
                for (int x = 0; x < H.ColumnAmount; x++)
                {
                    HWithIdentity.Set(y, x, H.Get(y, x));
                }
            }

            for (int y = 0; y < H.RowAmount; y++)
            {
                int n = 0;
                for (int x = H.ColumnAmount; x < H.ColumnAmount + H.RowAmount; x++)
                {
                    HWithIdentity.Set(y, x, y == n);

                    n++;
                }
            }
            return HWithIdentity;
        }

        private static Binary GenerateVerificationBits(BinaryMatrix H, Binary message)
        {
            Binary verification = new Binary(new bool[H.RowAmount]);
            for (int i = 0; i < H.RowAmount; i++)
            {
                Binary row = H.GetRow(i);
                Binary addiction = row & message;
                bool verificationBit = addiction.CountOnes() % 2 == 1 ? true : false;
                verification[i] = verificationBit;
            }
            return verification;
        }

        private static BinaryMatrix GenerateHMatrix(int rowsAmount, int columnsAmount)
        {
            BinaryMatrix H = new BinaryMatrix(rowsAmount, columnsAmount);

            int n = 0;
            for (int i = 1; i <= Math.Pow(2, rowsAmount); i++)
            {
                Binary binary = new Binary(i, H.RowAmount);
                if (binary.CountOnes() >= 2)
                {
                    for (int y = 0; y < rowsAmount; y++)
                    {
                        H.Set(y, n, binary[y]);
                    }
                    n++;
                }
                if (n >= H.ColumnAmount)
                    break;
            }
            return H;
        }
    }
}
