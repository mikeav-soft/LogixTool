using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Common
{
    public class RadixConverter
    {
        /// <summary>
        /// Преобразовывает последовательность байт в строку Hex-формата.
        /// </summary>
        /// <param name="bytes">Последовательность байт.</param>
        /// <param name="firstOctateIsHigh">Параметр задающий порядок вывода 16-ти ричных символов. При значении True
        /// в каждой паре полубайта (один байт) сначала выводится старший полубайт, затем младший.</param>
        /// <param name="reverseByteSequense">Параметр задающий при значении True вывод байт начиная со старшего и заканчивая младшим.</param>
        /// <returns></returns>
        public static string ToHexString(byte[] bytes, bool firstOctateIsHigh = true, bool reverseByteSequense = false)
        {
            const string MAP = "0123456789ABCDEF";

            string result = "";
            byte tempByte;

            for (int ix = 0; ix < bytes.Length * 2; ix++)
            {
                tempByte = bytes[ix / 2];
                bool firstOctate = (ix & 0x01) == 0;

                if (firstOctate == !firstOctateIsHigh)
                {
                    result += MAP[tempByte & 0x0F];
                }
                else
                {
                    result += MAP[(tempByte & 0xF0) >> 4];
                }
            }

            if (reverseByteSequense)
            {
                result = new string(result.Reverse().ToArray());
            }

            return result;
        }

        /// <summary>
        /// Преобразовывает строку последовательность байт в Bin-формата.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToBinaryString(byte[] bytes)
        {
            string result = "";
            if (bytes == null || bytes.Length <= 0)
            {
                return null;
            }

            foreach (byte b in bytes)
            {
                result = ToBinaryString(b) + result;
            }

            return result;
        }
        /// <summary>
        /// Преобразовывает байт в строку Bin-формата.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static string ToBinaryString(byte b)
        {
            string s = "";
            for (byte ix = 0; ix < 8; ix++)
            {
                if ((b & (1 << ix)) == (1 << ix))
                {
                    s = "1" + s;
                }
                else
                {
                    s = "0" + s;
                }
            }
            return s;
        }
        /// <summary>
        /// Преобразовывает строку Hex-формата в последовательность байт читая строку слева направо.
        /// </summary>
        /// <param name="str">Входная строка.</param>
        /// <param name="firstOctateIsHigh">Параметр задающий что последовательность полубайт идет попарно начиная со старшего.</param>
        /// <returns></returns>
        public static byte[] BytesFromHexString(string str, bool firstOctateIsHigh = true)
        {
            const string MAP = "0123456789ABCDEF";

            List<byte> result = new List<byte>();

            byte halfByte = 0;

            for (int ix = 0; ix < str.Length; ix++)
            {
                halfByte = (byte)MAP.IndexOf(str[ix]);

                if ((ix & 0x01) == 0)
                {
                    if (firstOctateIsHigh)
                    {
                        result.Add((byte)(halfByte << 4));
                    }
                    else
                    {
                        result.Add(halfByte);
                    }
                }
                else
                {
                    if (firstOctateIsHigh)
                    {
                        result[result.Count - 1] |= halfByte;
                    }
                    else
                    {
                        result[result.Count - 1] |= (byte)(halfByte << 4);
                    }
                }
            }

            return result.ToArray();
        }
        /// <summary>
        /// Преобразовывает Bin-формата в строку последовательность байт.
        /// </summary>
        /// <param name="bin"></param>
        /// <returns></returns>
        public static byte[] BytesFromBinaryString(string bin)
        {
            List<byte> bytes = new List<byte>();


            if (bin == null || bin.Trim() == "")
            {
                return null;
            }

            string tempstr = bin.Replace(" ", "");
            tempstr = String.Join("", tempstr.Reverse().ToArray());

            for (int ix = 0; ix < tempstr.Length; ix++)
            {
                int bit = ix - ((ix / 8) * 8);

                if (bit == 0)
                {
                    bytes.Add(0);
                }

                switch (tempstr[ix])
                {
                    case '0':
                        break;

                    case '1':
                        bytes[bytes.Count - 1] = (byte)(bytes[bytes.Count - 1] | ((byte)(1 << bit)));
                        break;

                    default:
                        return null;
                }
            }


            return bytes.ToArray();
        }

    }
}
