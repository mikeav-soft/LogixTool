using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LogixTool.Logix
{
    /// <summary>
    /// Класс описывающий мультиязычный текст.
    /// </summary>
    public class LangText
    {
        /// <summary>
        /// Текст, где ключ есть кратокое обозначение языка, а значение само содержание текста.
        /// </summary>
        public Dictionary<string, string> Texts { get; set; }
        /// <summary>
        /// Показывает что имеется содержание текста.
        /// </summary>
        public bool HasContent
        {
            get
            {
                return this.Texts != null && this.Texts.Count > 0;
            }
        }

        /// <summary>
        /// Создает новый объект мультязычного текста.
        /// </summary>
        public LangText()
        {
            this.Texts = new Dictionary<string, string>();
        }
        /// <summary>
        /// Создает новый объект мультязычного текста из XML элемента.
        /// </summary>
        /// <param name="xtexts">Text XElement.</param>
        public LangText(XElement xtexts)
            : this()
        {
            string[] validNames = { "Description", "Comment", "RevisionNote", "AdditionHelpText" };
            string lang, text;

            if (xtexts != null)
            {
                string currName = xtexts.Name.LocalName;
                if (validNames.Contains(currName))
                {
                    if (xtexts.HasElements)
                    {
                        foreach (XElement xcurr in xtexts.Elements("Localized" + currName))
                        {
                            lang = xcurr.Attribute("Lang").GetXValue("");
                            text = xcurr.GetXValue("");

                            text = text.Replace("\r", " ");
                            text = text.Replace("\n", " ");

                            if (!this.Texts.ContainsKey(lang))
                            {
                                this.Texts.Add(lang, text);
                            }
                        }
                    }
                    else
                    {
                        lang = "";
                        text = xtexts.GetXValue("");

                        text = text.Replace("\r", " ");
                        text = text.Replace("\n", " ");

                        if (!this.Texts.ContainsKey(lang))
                        {
                            this.Texts.Add(lang, text);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Пытается получить текст из заданного языка, возвращает true в случае удачи.
        /// </summary>
        /// <param name="lang">Код языка.</param>
        /// <param name="text">Результат: искомый текст.</param>
        /// <returns></returns>
        public bool TryGetText (string lang, out string text)
        {
            text = null;
            if (this.Texts!=null && lang != null && this.Texts.ContainsKey(lang))
            {
                text = this.Texts[lang]; 
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Создает копию данного обьъекта.
        /// </summary>
        /// <returns></returns>
        public LangText Copy()
        {
            LangText copiedLangText = (LangText)this.MemberwiseClone();
            Dictionary<string, string> texts = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> pair in this.Texts)
            {
                texts.Add(pair.Key, pair.Value);
            }

            copiedLangText.Texts = texts;

            return copiedLangText;
        }
        /// <summary>
        /// Преобразет текущий объект в текст.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.Texts != null && this.Texts.Count > 0)
            {
                return this.Texts.Values.ElementAt(0);
            }
            else
            {
                return "";
            }
        }
    }
}
