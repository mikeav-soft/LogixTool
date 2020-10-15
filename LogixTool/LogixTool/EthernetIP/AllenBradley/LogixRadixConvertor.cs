using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogixTool.Common;

namespace LogixTool.EthernetIP.AllenBradley
{
    public class LogixRadixConvertor
    {
        /// <summary>
        /// Преобразовывает значение байта в символ ASCII ControLogix.
        /// </summary>
        /// <param name="b">Входной байт.</param>
        /// <returns></returns>
        public static string GetASCIISymbol(byte b)
        {
            string result = "";

            // Номера символов за которыми не закреплено символов и отображаются как номер с символом '$'.
            if ((b >= 0 && b <= 8) || (b == 11) || (b >= 14 && b <= 31) || (b >= 127 && b <= 255))
            {
                result = "$" + RadixConverter.ToHexString(new byte[] { b });
            }
            // Номера символов которые являются специальными, отображаются с символом '$'.
            else if (b >= 9 && b <= 13 || b == 36 || b == 39)
            {
                switch (b)
                {
                    case 9: result = "$t"; break;
                    case 10: result = "$l"; break;
                    case 12: result = "$p"; break;
                    case 13: result = "$r"; break;
                    case 36: result = "$$"; break;
                    case 39: result = "$'"; break;
                }
            }
            // Отображаем латинские буквы или другие символы.
            else
            {
                result = Convert.ToChar(b).ToString();
            }
            return result;
        }
        /// <summary>
        /// Преобразовывает массив байт в строку символов ASCII ControLogix.
        /// </summary>
        /// <param name="b">Входной массив байт.</param>
        /// <returns></returns>
        public static string GetASCIIString(byte[] b)
        {
            string [] s = b.Select(t=>GetASCIISymbol(t)).ToArray();
            return String.Join("", s);
        }
        /// <summary>
        /// Преобразовывает строку символов ASCII ControLogix в массив байт.
        /// </summary>
        /// <param name="s">Входная строка.</param>
        /// <returns></returns>
        public static byte[] FromASCIIString(string s, ushort byteSize)
        {
            List<byte> bytes = new List<byte>();

            for (int ix = 0; ix < s.Length; ix++)
            {
                char c = s[ix];

                if (c == '$')
                {
                    // Случай когда после префикса '$' идут два символа hex 0...F.
                    if (ix + 1 < s.Length && "0123456789ABCDEF".Contains(s[ix + 1]))
                    {
                        if (ix + 2 < s.Length && "0123456789ABCDEF".Contains(s[ix + 2]))
                        {
                            // Ковертируем один байт из 8-ми битной 16-ти разрядной (Hex) строки представления. 
                            byte[] b = RadixConverter.BytesFromHexString(s[ix + 1].ToString() + s[ix + 2].ToString());
                            
                            // Проверка что после преобразования имеем один байт.
                            if (b == null || b.Length!=1)
                            {
                                return null;
                            }

                            // Проверяем что преобразованная символ с префиксом '$' соответствует данному диапазону.
                            if ((b[0] >= 0 && b[0] <= 8) || (b[0] == 11) || (b[0] >= 14 && b[0] <= 31) || (b[0] >= 127 && b[0] <= 255))
                            {
                                bytes.Add(b[0]);
                                ix += 2;
                            }
                            else
                            {
                                return null;
                            }

                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        // Случай когда после префикса '$' идет один специальный символ.
                        if (ix + 1 < s.Length)
                        {
                            switch (s[ix + 1])
                            {
                                case 't': bytes.Add(9); break;
                                case 'l': bytes.Add(10); break;
                                case 'p': bytes.Add(12); break;
                                case 'r': bytes.Add(13); break;
                                case '$': bytes.Add(36); break;
                                case '\'': bytes.Add(39); break;
                                default: return null;
                            }

                            ix += 1;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    bytes.Add(Convert.ToByte(c));
                }
            }

            // Проверяем полученные байты.
            if (bytes == null || bytes.Count == 0 || bytes.Count > byteSize)
            {
                return null;
            }

            // Создаем результирующий массив с заданным размером и перемещаем туда полученные байты.
            byte[] result = new byte[byteSize];
            for (int ix = 0; ix < result.Length; ix++)
            {
                if (bytes.Count > ix)
                {
                    result[ix] = bytes[ix];
                }
                else
                {
                    result[ix] = 0;
                }
            }

            return result;
        }
        /// <summary>
        /// Преобразовывает массив байт в строку символов бинарного формата ControLogix.
        /// </summary>
        /// <param name="b">Входной массив байт.</param>
        /// <returns></returns>
        public static string GetBinaryString (byte [] b)
        {
            return "2#" + String.Join(" ", b.Select(t => RadixConverter.ToBinaryString(new byte[] { t })).Reverse().ToArray());
        }
        /// <summary>
        /// Преобразовывает строку символов бинарного формата ControLogix в массив байт.
        /// </summary>
        /// <param name="s">Входная строка.</param>
        /// <returns></returns>
        public static byte[] FromBinaryString(string s, ushort byteSize)
        {
            // Присваиваем строку промежуточной переменной.
            string text = s;
            string prefix = "2#";

            // Проверяем строку на длину.
            if (text == null || text.Length < prefix.Length + 1 || byteSize == 0)
            {
                return null;
            }

            // Проверяем что строка начинается с префикса.

            if (!text.StartsWith(prefix))
            {
                return null;
            }

            // Выделяем строку после префикса и удаляем все пробелы.
            text = s.Substring(prefix.Length, text.Length - prefix.Length).Replace(" ", "");

            if (text.Any(c => c != '0' && c != '1'))
            {
                return null;
            }

            // Преобразовываем строку в массив байт.
            byte[] bytes = RadixConverter.BytesFromBinaryString(text);

            // Проверяем полученные байты.
            if (bytes == null || bytes.Length == 0 || bytes.Length > byteSize)
            {
                return null;
            }

            // Создаем результирующий массив с заданным размером и перемещаем туда полученные байты.
            byte[] result = new byte[byteSize];
            for (int ix = 0; ix < result.Length; ix++)
            {
                if (bytes.Length>ix)
                {
                    result[ix] = bytes[ix];
                }
                else
                {
                    result[ix] = 0;
                }
            }

            return result;
        }
        /// <summary>
        /// Преобразовывает массив байт в строку символов шестнадцетиричного формата ControLogix.
        /// </summary>
        /// <param name="b">Входной массив байт.</param>
        /// <returns></returns>
        public static string GetHexString(byte[] b)
        {
            return "16#" + String.Join(" ", b.Select(t => RadixConverter.ToHexString(new byte[] { t })).Reverse().ToArray());
        }
        /// <summary>
        /// Преобразовывает строку символов шестнадцетиричного формата ControLogix в массив байт.
        /// </summary>
        /// <param name="s">Входная строка.</param>
        /// <returns></returns>
        public static byte[] FromHexString(string s, ushort byteSize)
        {
            // Присваиваем строку промежуточной переменной.
            string text = s;
            string prefix = "16#";


            // Проверяем строку на длину.
            if (text == null || text.Length < prefix.Length + 1 || byteSize == 0)
            {
                return null;
            }

            // Проверяем что строка начинается с префикса.

            if (!text.StartsWith(prefix))
            {
                return null;
            }

            // Выделяем строку после префикса и удаляем все пробелы.
            text = s.Substring(prefix.Length, text.Length - prefix.Length).Replace(" ", "");
            text = new String(text.Reverse().ToArray());

            if (text.Any(c => !"0123456789ABCDEF".Contains(c)))
            {
                return null;
            }

            // Преобразовываем строку в массив байт.
            byte[] bytes = RadixConverter.BytesFromHexString(text,false);

            // Проверяем полученные байты.
            if (bytes == null || bytes.Length == 0 || bytes.Length > byteSize)
            {
                return null;
            }

            // Создаем результирующий массив с заданным размером и перемещаем туда полученные байты.
            byte[] result = new byte[byteSize];
            for (int ix = 0; ix < result.Length; ix++)
            {
                if (bytes.Length > ix)
                {
                    result[ix] = bytes[ix];
                }
                else
                {
                    result[ix] = 0;
                }
            }


            return result;
        }
        /// <summary>
        /// Преобразовывает массив байт в строку символов десятичного формата ControLogix.
        /// </summary>
        /// <param name="b">Входной массив байт.</param>
        /// <returns></returns>
        public static string GetNumericString(byte[] b)
        {
            if (b == null || b.Length == 0 || b.Length > 8)
            {
                return null;
            }

            switch (b.Length)
            {
                case 1: return ((sbyte)b[0]).ToString();
                case 2: return BitConverter.ToInt16(b, 0).ToString();
                case 4: return BitConverter.ToInt32(b, 0).ToString();
                case 8: return BitConverter.ToInt64(b, 0).ToString();
                default: return null;
            }
        }
        /// <summary>
        /// Преобразовывает строку символов десятичного формата ControLogix со знаком в массив байт.
        /// </summary>
        /// <param name="s">Входная строка.</param>
        /// <returns></returns>
        public static byte[] FromNumericString(string s, ushort byteSize)
        {
            if (s == null || s.Length == 0 || !s.ContainsOnly("-0123456789".ToCharArray()) || byteSize < 0 || byteSize > 8)
            {
                return null;
            }

            byte[] result;

            switch (byteSize)
            {
                case 1:
                    {
                        sbyte sbyteValue = 0;
                        if (!sbyte.TryParse(s, out sbyteValue))
                        {
                            return null;
                        }
                        result = new byte [] {(byte)sbyteValue};
                    }
                    break;
                case 2:
                    {

                        Int16 int16Value = 0;
                        if (!Int16.TryParse(s, out int16Value))
                        {
                            return null;
                        }
                        result = BitConverter.GetBytes(int16Value);
                    }
                    break;
                case 4:
                    {
                        Int32 int32Value = 0;
                        if (!Int32.TryParse(s, out int32Value))
                        {
                            return null;
                        }
                        result = BitConverter.GetBytes(int32Value);
                    }
                    break;
                case 8:
                    {
                        Int64 int64Value = 0;
                        if (!Int64.TryParse(s, out int64Value))
                        {
                            return null;
                        }
                        result = BitConverter.GetBytes(int64Value);
                    }
                    break;

                default: return null;
            }

            return result;
        }
        /// <summary>
        /// Преобразовывает массив байт в строку символов формата числа с плавающей запятой ControLogix.
        /// </summary>
        /// <param name="b">Входной массив байт.</param>
        /// <returns></returns>
        public static string GetFloatString(byte[] b)
        {
            if (b == null || b.Length != 4)
            {
                return null;
            }

            return BitConverter.ToSingle(b.ToArray(), 0).ToString();
        }
        /// <summary>
        /// Преобразовывает строку символов формата числа с плавающей запятой ControLogix в массив байт.
        /// </summary>
        /// <param name="s">Входная строка.</param>
        /// <returns></returns>
        public static byte[] FromFloatString(string s)
        {
            if (s == null || s.Length == 0)
            {
                return null;
            }

            float value = 0;

            if (!float.TryParse(s, out value))
            {
                return null;
            }

            return BitConverter.GetBytes(value);
        }
        /// <summary>
        /// Преобразовывает байт в строку символов формата числа Булевого значения ControLogix.
        /// Значение байта может быть 0x00, что соответствет логическому нулю или 0xFF что соответствует логической единице.
        /// </summary>
        /// <param name="b">Значение байта.</param>
        /// <returns></returns>
        public static string GetBoolString(byte b)
        {
            if (b == 0)
            {
                return "0";
            }
            else if (b == 0xFF)
            {
                return "1";
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte? FromBoolString(string s)
        {
            if (s == "0")
            {
                return 0;
            }
            else if (s == "1")
            {
                return 0xFF;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Преобразовывает массив байт в строку символов ASCII ControLogix.
        /// </summary>
        /// <param name="b">Входной массив байт.</param>
        /// <returns></returns>
        public static string GetDecimalString(byte[] b)
        {
            string[] s = b.Select(t => t.ToString()).ToArray();
            return String.Join(" ", s);
        }

    }
}
