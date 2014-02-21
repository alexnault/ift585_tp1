using HammingCode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ift585_tp1
{
    class Frame
    {
        // Cannot be edited
        public int id { get; private set; }
        public int mustResend { get; set; }
        protected int type { get; private set; }
        protected int dataLength { get; private set; }
        public byte[] data { get; private set; }

        ///////////////////
        /*public int ttl;
        public int type;
        public int dataLength;
        protected byte[] data;*/
        protected int checksum;

        private byte[] _bytes;

        public const int NB_BYTES = 5; // TODO change to NB_MAX_DATA_BYTES

        /// <summary>
        /// Create a frame with its header's infos
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="ttl"></param>
        public Frame(int id, int type, byte[] data, int mustResend = 0)
        {
            this.id = id;
            this.mustResend = mustResend;
            this.type = type;
            this.dataLength = data.Length;
            this.data = data;

            this._bytes = toBytes();

            //this.checksum = findChecksum();
        }

        /// <summary>
        /// Build a Frame from a frame formed of bytes
        /// </summary>
        /// <param name="bytes"></param>
        public Frame(byte[] bytes)
        {
            fromBytes(bytes);
        }

        public void fromBytes(byte[] bytes)
        {
            this._bytes = bytes; // TODO _bytes might have errors, we have to fix it.

            byte[] id = new byte[sizeof(int)];
            Array.Copy(bytes, 0, id, 0, sizeof(int));
            this.id = BitConverter.ToInt32(id, 0);

            byte[] timeout = new byte[4];
            Array.Copy(bytes, 4, timeout, 0, sizeof(int));
            this.mustResend = BitConverter.ToInt32(timeout, 0);

            byte[] type = new byte[4];
            Array.Copy(bytes, 8, type, 0, sizeof(int));
            this.type = BitConverter.ToInt32(type, 0);

            byte[] len = new byte[4];
            Array.Copy(bytes, 12, len, 0, sizeof(int));
            this.dataLength = BitConverter.ToInt32(len, 0);

            byte[] data = new byte[this.dataLength];
            Array.Copy(bytes, 16, data, 0, this.dataLength);
            this.data = data;

            byte[] checksum = new byte[4];
            Array.Copy(bytes, 16 + this.dataLength, checksum, 0, sizeof(int));
            this.checksum = BitConverter.ToInt32(checksum, 0);

            //Console.WriteLine(this); // TODO remove
        }

        public byte[] toBytes()
        {
            if (_bytes == null)
            {
                byte[] frame = new byte[5 * sizeof(int) + data.Length];
                BitConverter.GetBytes(id).CopyTo(frame, 0 * sizeof(int));
                BitConverter.GetBytes(mustResend).CopyTo(frame, 1 * sizeof(int));
                BitConverter.GetBytes(type).CopyTo(frame, 2 * sizeof(int));
                BitConverter.GetBytes(data.Length).CopyTo(frame, 3 * sizeof(int));
                data.CopyTo(frame, 4 * sizeof(int));

                BitConverter.GetBytes(findChecksum(frame, 0, frame.Length)).CopyTo(frame, 4 * sizeof(int) + data.Length);

                _bytes = frame;
            }
            return _bytes;
        }

        protected int findChecksum(byte[] bytes, int start, int end)
        {
            int checksum = 0;
            foreach (byte b in bytes)
            {
                checksum += b;
            }
            return checksum;
        }

        public bool checksumIsFine()
        {
            if (checksum >= 0) // if checksum is set
            {
                return findChecksum(_bytes, 0, _bytes.Length - sizeof(int)) == checksum;
            }
            return true;
        }

        public override string ToString()
        {
            string s = string.Format("Frame : id={0} ttl={1} type={2} dataLength={3}", id, mustResend, type, dataLength);
            string dataString = string.Empty;
            for (int i = 0; i < dataLength; i++)
            {
                dataString += data[i] + " ";
            }
            s += string.Format(" data={0} \n", dataString, checksum);
            return s;
        }

    }
}
