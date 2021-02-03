using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Common.Extension
{
    public static class Strings
    {
        /// <summary>
        /// Define if string contains only symbols from array.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <param name="charArray">Reference char array.</param>
        /// <returns></returns>
        public static bool ContainsOnly(this String s, char[] charArray)
        {
            if (s == null || charArray == null || charArray.Length == 0)
            {
                return false;
            }

            Dictionary<char, char> charDict = new Dictionary<char, char>();
            foreach (char c in charArray)
            {
                if (!charDict.ContainsKey(c))
                {
                    charDict.Add(c, c);
                }
            }

            foreach (char c in s)
            {
                if (!charDict.ContainsKey(c))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Формирует новую строку заданной длины из исходной.
        /// </summary>
        /// <param name="s">Исходная строка.</param>
        /// <param name="length">Кол-во символов ячейки.</param>
        /// <returns></returns>
        public static string InCell(this String s, int length, bool rightAligment = false)
        {
            if (s == null || length <= 0)
            {
                return s;
            }

            string text = s;

            if (s.Length <= length)
            {
                for (int ix = s.Length; ix < length; ix++)
                {
                    if (rightAligment)
                    {
                        text = " " + text;
                    }
                    else
                    {
                        text += " ";
                    }
                }
            }
            else
            {
                text = s.Substring(0, length);
            }

            return text;
        }
        /// <summary>
        /// Делит строку на фрагменты текста заключенные в ключевые слова.
        /// </summary>
        /// <param name="s">Исходная строка.</param>
        /// <param name="startKey">Ключевая строка начала фрагмента.</param>
        /// <param name="endKey">Ключевая строка конца фрагмента.</param>
        /// <param name="textSelectMethod">Метод деления на фрагменты.</param>
        /// <returns></returns>
        public static TextFragment[] Separate(this String s, string startKey, string endKey, TextSelectMethod textSelectMethod)
        {
            // Контейнер с результатами.
            List<TextFragment> result = new List<TextFragment>();
            if (s == null || s.Trim() == "" || startKey == null || endKey == null || startKey.Trim() == "" || endKey.Trim() == "")
            {
                return result.ToArray();
            }

            bool isFragmentIndex = false;

            #region [ FORWARD ]
            /* ========================================================================== */
            if (textSelectMethod == TextSelectMethod.ToForward)
            {
                // Цикл с посимвольной индексацией.
                for (int ix = 0; ix < s.Length; ix++)
                {
                    bool isStartKeyIndex = (startKey.Length <= (s.Length - ix) && s.Substring(ix, startKey.Length) == startKey);
                    bool isEndKeyIndex = (endKey.Length <= (s.Length - ix) && s.Substring(ix, endKey.Length) == endKey);

                    if (isFragmentIndex)
                    {
                        if (isEndKeyIndex)
                        {
                            isFragmentIndex = false;

                            // Добавляем конечный ключ в список фрагментов и увеличиваем индекс на длину ключа.
                            result.Add(new TextFragment() { Value = endKey, Type = TextType.EndKey });
                            ix += endKey.Length - 1;
                        }
                        else
                        {
                            if (result.Count == 0 || result.Last().Type != TextType.Fragment)
                            {
                                result.Add(new TextFragment() { Value = "", Type = TextType.Fragment });
                            }

                            // Добавляем текущий симвой с индексом ix если тот не вышел за пределы длины базовой строки.
                            if (ix < s.Length)
                            {
                                result[result.Count - 1].Value += s[ix];
                            }
                        }
                    }
                    else
                    {
                        if (isStartKeyIndex)
                        {
                            isFragmentIndex = true;

                            // Добавляем стартовый ключ в список фрагментов и увеличиваем индекс на длину ключа.
                            result.Add(new TextFragment() { Value = startKey, Type = TextType.StartKey });
                            ix += startKey.Length - 1;
                        }
                        else
                        {
                            if (result.Count == 0 || result.Last().Type != TextType.Other)
                            {
                                result.Add(new TextFragment() { Value = "", Type = TextType.Other });
                            }

                            // Добавляем текущий симвой с индексом ix если тот не вышел за пределы длины базовой строки.
                            if (ix < s.Length)
                            {
                                result[result.Count - 1].Value += s[ix];
                            }
                        }
                    }
                }
            }
            /* ========================================================================== */
            #endregion

            #region [ CENTER ]
            /* ========================================================================== */
            if (textSelectMethod == TextSelectMethod.ToCenter)
            {
                // Анализ сначала строки, поиск стартового ключевого слова фрагмента. Цикл с посимвольной индексацией.
                for (int ix = 0; ix < s.Length; ix++)
                {
                    // Проверяем имеется ли впереди стартовый ключ-строка фрагмента.
                    if (startKey.Length <= (s.Length - ix) && s.Substring(ix, startKey.Length) == startKey && !isFragmentIndex)
                    {
                        isFragmentIndex = true;
                        // Добавляем стартовый ключ в список фрагментов и увеличиваем индекс на длину ключа.
                        result.Add(new TextFragment() { Value = startKey, Type = TextType.StartKey });
                        ix += startKey.Length;
                        // Добавляем последний элемент как текущий фрагмент.
                        result.Add(new TextFragment() { Value = "", Type = TextType.Fragment });
                    }

                    // Добавляем последний элемент как текущую строку если контейнер пуст.
                    if (result.Count == 0)
                    {
                        result.Add(new TextFragment() { Value = "", Type = TextType.Other });
                    }
                    // Добавляем текущий симвой с индексом ix если тот не вышел за пределы длины базовой строки.
                    if (ix < s.Length)
                    {
                        result[result.Count - 1].Value += s[ix];
                    }
                }

                if (result.Count > 0 && result.Last().Type == TextType.Fragment)
                {
                    // Анализ сначала строки, поиск конечного ключевого слова фрагмента. Цикл с посимвольной индексацией.
                    for (int ix = result.Last().Value.Length - 1; ix >= 0; ix--)
                    {
                        string fragmentText = result.Last().Value;
                        if (endKey.Length <= (fragmentText.Length - ix) && fragmentText.Substring(ix, endKey.Length) == endKey)
                        {
                            // Последний элемент представленный как фрагмент делим на конечное ключевое слово.
                            TextFragment textEndKey = new TextFragment() { Value = endKey, Type = TextType.EndKey };
                            TextFragment textOther = new TextFragment() { Value = fragmentText.Substring(ix + endKey.Length, fragmentText.Length - (ix + endKey.Length)), Type = TextType.Other };
                            result[result.Count - 1].Value = result[result.Count - 1].Value.Substring(0, ix);

                            // Добавляем текстовые фрагменты как ключевое слово и как остальная часть.
                            result.Add(textEndKey);
                            if (textOther.Value != "")
                            {
                                result.Add(textOther);
                            }
                            // Конечное ключевое слово найдено, выходим из цикла.
                            break;
                        }
                    }
                }
            }
            /* ========================================================================== */
            #endregion

            return result.ToArray();
        }
        /// <summary>
        /// Делит строку на фрагменты текста заключенные в ключевые слова.
        /// </summary>
        /// <param name="s">Исходная строка.</param>
        /// <param name="keys">Список ключевых строк.</param>
        /// <returns></returns>
        public static TextFragment[] Separate(this String s, string[] keys)
        {
            // Контейнер с результатами.
            List<TextFragment> result = new List<TextFragment>();

            if (s == null || s.Trim() == "" || keys == null || keys.Length == 0)
            {
                return result.ToArray();
            }

            bool keyWasFound = false;

            #region [ SPLITTING ]
            /* ========================================================================== */
            // Цикл с посимвольной индексацией.
            for (int ix = 0; ix < s.Length; )
            {
                keyWasFound = false;

                foreach (string key in keys)
                {
                    if (key.Length <= (s.Length - ix) && s.Substring(ix, key.Length) == key)
                    {
                        result.Add(new TextFragment() { Value = key, Type = TextType.Key });
                        ix += key.Length;
                        keyWasFound = true;
                        break;
                    }
                }
                // Добавляем первый фрагмент.
                if (result.Count == 0)
                {
                    result.Add(new TextFragment() { Value = "", Type = TextType.Fragment });
                }

                // Если ключевая строка не найдена то прибавляем символ.
                if (!keyWasFound)
                {
                    if (result.Last().Type != TextType.Fragment)
                    {
                        result.Add(new TextFragment() { Value = "", Type = TextType.Fragment });
                    }
                    result.Last().Value += s[ix];
                    ix++;
                }
            }
            /* ========================================================================== */
            #endregion

            return result.ToArray();
        }
        /// <summary>
        /// Сравинвает строку с символьной маской.
        /// </summary>
        /// <param name="s">Исходная строка.</param>
        /// <param name="mask">Символьная маска которая задается явными символами или 
        /// символом '*' означающий любой набор символов.</param>
        /// <returns></returns>
        public static bool CompareWithMask(this String s, string mask)
        {
            bool result = true;
            TextFragment[] fragments = mask.Separate(new string[] { "*" });

            if (fragments.Length == 0)
            {
                return result;
            }

            bool startOfMaskIsImplict = fragments.First().Type == TextType.Fragment;                        // Начало маски задано явно.
            bool endOfMaskIsImplict = fragments.Last().Type == TextType.Fragment;                           // Конец маски задано явно.
            bool middleOfMaskIsImplict = !(fragments.Where(t => t.Type == TextType.Fragment).Count() >= 2); // Середина маски задано явно.
                 
            int maskIndex = 0;

            // Случай: маска задана полностью явно.
            if (startOfMaskIsImplict && endOfMaskIsImplict && middleOfMaskIsImplict)
            {
                result = (s == mask);
                return result;
            }

            // Случай: маска задана явно сначала.
            if (startOfMaskIsImplict)
            {
                result &= (s.Length >= fragments.First().Value.Length) && s.StartsWith(fragments.First().Value);
            }

            // Случай: маска задана явно в конце.
            if (endOfMaskIsImplict)
            {
                result &= (s.Length >= fragments.Last().Value.Length) && s.EndsWith(fragments.Last().Value);
            }

            // Случай: маска задана в середине.
            if (!middleOfMaskIsImplict)
            {
                bool middleMaskIsOk = false;            // Маска в середине соответствует строке s.
                for (int ix = 0; ix < s.Length; ix++)
                {
                    // Цикл перебора текущего фрагмента до элемента Fargment.
                    while (fragments.Length > maskIndex && fragments[maskIndex].Type == TextType.Key)
                    {
                        maskIndex++;
                    }
                    // Проверяем что текущий фрагмент 
                    if (fragments.Length > maskIndex && (s.Length - ix) >= fragments[maskIndex].Value.Length
                        && s.Substring(ix, fragments[maskIndex].Value.Length) == fragments[maskIndex].Value)
                    {
                        maskIndex++;
                    }
                    // Критерий положительного результата достигнуть 
                    if (fragments.Length <= maskIndex)
                    {
                        middleMaskIsOk = true;
                        break;
                    }
                }
                result &= middleMaskIsOk;
            }

            return result;
        }

        /// <summary>
        /// Размножает текущую строку в указанное число раз.
        /// </summary>
        /// <param name="s">Исходная строка.</param>
        /// <param name="count">Число раз которое нужно повторить символ.</param>
        /// <returns></returns>
        public static string Multiply(this String s, int count)
        {
            if (s == null)
            {
                return s;
            }

            string result = "";

            for (int ix = 0; ix < count; ix++)
            {
                result += s;
            }

            return result;
        }
    }
}
