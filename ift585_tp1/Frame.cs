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
        public enum Type
        {
            Normal = 0,
            ACK = 1,
            NAK = 2
        };

        public int id { get; private set; }
        public int mustResend { get; set; }
        public Type type { get; private set; }
        protected int dataLength { get; private set; }
        public byte[] data { get; private set; }

        protected int checksum;

        private byte[] _bytes;

        public const int NB_MAX_DATA_BYTES = 7;

        /// <summary>
        /// Create a frame with its header's infos
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="ttl"></param>
        public Frame(int id, Type type, byte[] data, int mustResend = 0)
        {
            this.id = id;
            this.mustResend = mustResend;
            this.type = type;
            this.dataLength = data.Length;
            this.data = data;
            this._bytes = toBytes();
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
            this.type = (Type)BitConverter.ToInt32(type, 0);

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
                BitConverter.GetBytes(this.id).CopyTo(frame, 0 * sizeof(int));
                BitConverter.GetBytes(this.mustResend).CopyTo(frame, 1 * sizeof(int));
                BitConverter.GetBytes((int)this.type).CopyTo(frame, 2 * sizeof(int));
                BitConverter.GetBytes(this.data.Length).CopyTo(frame, 3 * sizeof(int));
                this.data.CopyTo(frame, 4 * sizeof(int));

                this.checksum = findChecksum(frame, 0, frame.Length - sizeof(int));
                BitConverter.GetBytes(this.checksum).CopyTo(frame, 4 * sizeof(int) + data.Length);

                _bytes = frame;
            }
            return _bytes;
        }

        protected int findChecksum(byte[] bytes, int start, int end)
        {
            if (start > end)
                throw new InvalidOperationException("Start has to be lower than end.");

            int checksum = 0;
            for (int i = start; i < end; i++ )
            {
                checksum += bytes[i];
            }
            return checksum;
        }

        public bool checksumIsFine()
        {
            return findChecksum(_bytes, 0, _bytes.Length - sizeof(int)) == checksum;
        }

        public override string ToString()
        {
            string s = string.Format("Frame : id={0} type={1} dataLength={2}", id, type, dataLength);
            string dataString = string.Empty;
            for (int i = 0; i < dataLength; i++)
            {
                dataString += data[i] + " ";
            }
            //s += string.Format(" data={0} \n", dataString, checksum);
            s += string.Format(" checksum={0}", checksum);
            return s;
        }

        public string DataToString()
        {
            try
            {
                string s = string.Empty;
                for (int i = 0; i < dataLength; i++)
                {
                    s += Convert.ToChar(data[i]);
                }
                return s;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
    }
}
