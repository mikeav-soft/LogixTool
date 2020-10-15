using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LogixTool.Common
{
    /// <summary>
    /// Класс содержащий статические методы для открытия и сохранения *.csv файлов.
    /// </summary>
    public class CsvFile
    {
        /// <summary>
        /// Открывает текстовый *.csv файл с символами - разделителями и преобразовывает в виде списка строковых элементов.
        /// </summary>
        /// <param name="filepath">Полный путь к текстовому файлу.</param>
        /// <param name="separator">Символ разделитель элементов.</param>
        /// <param name="items">Результат: список прочитанных элементов, где элемент списка - ряд, а элемент массива элемента списка - значение столбца.</param>
        /// <returns></returns>
        public static bool Open(string filepath, char separator, out List<string []> items)
        {
            items = new List<string[]>();

            if (filepath == null)
            {
                return false;
            }

            try
            {
                StreamReader sr = new StreamReader(filepath);
                while(!sr.EndOfStream)
                {
                    string text = sr.ReadLine();
                    string[] parts = text.Split(new char[] { separator });
                    items.Add(parts);
                }
                sr.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Записывает список строковых элементов в текстовый *.csv файл с символами - разделителями.
        /// </summary>
        /// <param name="filepath">Полный путь к текстовому файлу.</param>
        /// <param name="separator">Символ разделитель элементов.</param>
        /// <param name="items">Результат: список прочитанных элементов, где элемент списка - ряд, а элемент массива элемента списка - значение столбца.</param>
        /// <returns></returns>
        public static bool Save(string filepath, char separator, List<string[]> items)
        {
            if (filepath == null || (items!=null && items.Any(i=>i==null)))
            {
                return false;
            }

            try
            {
                StreamWriter sw = new StreamWriter(filepath);
                if (items!=null)
                {
                    foreach(string[] item in items)
                    {
                        sw.WriteLine(String.Join(separator.ToString(), item));
                    }
                }

                sw.Flush();
                sw.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
