using System;
using System.Collections.Generic;

namespace EIP
{
    /// <summary>
    /// Класс позволяющий оперировать с массивом байт.
    /// </summary>
    internal static class BitExtractor
    {
        /// <summary>
        /// Получает последовательность байт из другой последовательности с
        /// заданным диапазоном Бит.
        /// </summary>
        /// <param name="b">Исходная последовательность байт.</param>
        /// <param name="bitIndex">Стартовый бит.</param>
        /// <param name="bitLength">Длина выбираемых бит.</param>
        /// <returns></returns>
        internal static byte[] GetBitRange(byte[] b, Int64 bitIndex, Int64 bitLength)
        {
            if (b == null)
            {
                throw new ArgumentNullException("b", "Argument of input Bytes is NULL");
            }

            if (bitIndex < 0)
            {
                return null;
            }

            if (bitLength < 0)
            {
                return null;
            }

            if (b.Length <= 0 || (Int64)(b.Length * 8) < bitIndex + bitLength)
            {
                return null;
            }

            List<byte> bytes = new List<byte>();

            for (Int64 ix = bitIndex; ix < bitIndex + bitLength; ix++)
            {
                Int64 readedByte = ix / 8;
                Int32 readedBit = (Int32)(ix - ((ix / 8) * 8));
                Int64 writedByte = (ix - bitIndex) / 8;
                Int32 writedBit = (Int32)((ix - bitIndex) - (((ix - bitIndex) / 8) * 8));

                if (writedBit == 0)
                {
                    bytes.Add(0);
                }

                byte temp = (byte)(b[readedByte] & ((byte)(1 << readedBit)));
                temp = (byte)(temp >> readedBit);
                bytes[bytes.Count - 1] = (byte)(bytes[bytes.Count - 1] | (temp << writedBit));
            }

            return bytes.ToArray();
        }
    }
}
