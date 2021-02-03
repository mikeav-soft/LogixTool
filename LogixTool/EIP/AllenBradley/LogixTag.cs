using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EIP.AllenBradley.Models;

namespace EIP.AllenBradley
{
    /// <summary>
    /// Тэг контроллера Allen Breadley.
    /// </summary>
    public class LogixTag
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// Путь к объекту согласно спецификации CIP.
        /// </summary>
        public EPath SymbolicEPath { get; set; }

        private TagDataTypeDefinition _Type;
        /// <summary>
        /// Возвращает или задает тип данных значения тэга.
        /// Представляет собой подструктуру которая не должа быть равной Null.
        /// </summary>
        public TagDataTypeDefinition Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (_Type != null && value != null)
                {
                    _Type.CopyFrom(value);
                }
            }
        }

        /// <summary>
        /// Возвращает или задает разрешение на выполнение операции.
        /// </summary>
        public bool ReadEnable { get; set; }
        /// <summary>
        /// Возвращает или задает разрешение на выполнение операции.
        /// </summary>
        public bool WriteEnable { get; set; }

        /// <summary>
        /// Возвращает или задает вид системы исчисления значения.
        /// Система исчисления необходима для работы с текстовым форматом значения тэга.
        /// </summary>
        public TagValueRadix Radix { get; set; }

        /// <summary>
        /// Возвращает структуру обслуживания чтения значения тэга.
        /// </summary>
        public TagValueReading ReadValue { get; private set; }
        /// <summary>
        /// Возвращает структуру обслуживания записи значения тэга.
        /// </summary>
        public TagValueWriting WriteValue { get; private set; }

        /// <summary>
        /// Возвращает или задает ссылку на элемент таблицы используемый при групповом чтении.
        /// Устаналивается/снимается классом CLXCustomTagMemoryTable при добавлении/добавлении тэга соответственно.
        /// </summary>
        public CLXCustomTagMemoryItem OwnerTableItem { get; internal set; }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Создает новый тэг контроллера Allen Breadley.
        /// </summary>
        public LogixTag()
        {
            this._Type = new TagDataTypeDefinition(0);

            this.ReadValue = new TagValueReading(this._Type);
            this.WriteValue = new TagValueWriting(this._Type);

            this.ReadEnable = true;
            this.WriteEnable = false;

            this.OwnerTableItem = null;

            this.ReadValue.ReportUpdated += ReadValue_ReportUpdated;
        }

        #region [ EVENT SUBSCRIPTIONS ]
        /* ================================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ReadValue_ReportUpdated(object sender, EventArgs e)
        {
            if (sender != null && sender.GetType() == typeof(TagValueReading))
            {
                // Получаем объект контроля над чтением значения тэга.
                TagValueReading readValueControl = (TagValueReading)sender;
            }
        }
        /* ================================================================================================== */
        #endregion

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Сбрасывает состояние тэга.
        /// </summary>
        public virtual void InitState()
        {
            this.SymbolicEPath = null;
            this.Type.Init();
            this.ReadValue.Report.Init();
            this.WriteValue.Report.Init();
            this.ReadEnable = true;
            this.WriteEnable = false;
            this.OwnerTableItem = null;
        }
        /// <summary>
        /// Возвращает текущее полученное значение тэга в строковом формате соответствующее текущему значению свойства Radix
        /// данного тэга.
        /// </summary>
        /// <returns></returns>
        public string GetReadedValueText()
        {
            return this.GetValueText(this.ReadValue.Report.Data, " ");
        }
        /// <summary>
        /// Возвращает текущее заданное значение тэга для записи в строковом формате соответствующее текущему значению свойства Radix
        /// данного тэга.
        /// </summary>
        /// <returns></returns>
        public string GetWritedValueText()
        {
            return this.GetValueText(this.WriteValue.RequestedData, " ");
        }
        /// <summary>
        /// Устанавливает текущее заданное значение тэга для записи из строкового формата соответствующее текущему значению свойства Radix
        /// данного тэга.
        /// </summary>
        /// <returns></returns>
        public bool SetWritedValueText(string s, int index)
        {
            // Получаем текущее значение для записи.
            List<byte[]> value = this.WriteValue.RequestedData;

            // Проверяем значение для записи и входные параметры.
            if (value == null || value.Count <= index)
            {
                return false;
            }

            // Преобразовываем текущий текст в последовательность байт.
            byte[] bytes = this.SetValueText(s);

            // Проверяем значение после преобразования строки.
            if (bytes == null)
            {
                return false;
            }

            value[index] = bytes;
            this.WriteValue.RequestedData = value;

            return true;
        }
        /* ================================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Преобразовывает последовательность байт для текущего тэга с его определенным типом данных 
        /// а также определенной системой исчисления в строковое представление.
        /// </summary>
        /// <param name="bytes">Последовательность байт для преобразования.</param>
        /// <returns></returns>
        private string GetValueText(byte[] bytes)
        {
            string result = null;

            // Проверяем что текущая последовательность байт не равна Null.
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            // Проверяем что текущий тип данных существует и его код не равен 0.
            if (this.Type == null || this.Type.Code == 0)
            {
                return null;
            }

            //// Проверяем что поступившие данные соответствуют определенному размеру типа данных.
            //if (bytes.Length != this.Type.Size)
            //{
            //    return null;
            //}

            // Проверяем является ли данный тип данных структурой.
            switch (this.Type.Family)
            {
                case TagDataTypeFamily.AtomicBool:
                    switch (this.Radix)
                    {
                        case TagValueRadix.Decimal:
                            result = LogixRadixConvertor.GetNumericString(bytes);
                            break;

                        case TagValueRadix.Hex:
                            result = LogixRadixConvertor.GetHexString(bytes);
                            break;

                        case TagValueRadix.Binary:
                            result = LogixRadixConvertor.GetBinaryString(bytes);
                            break;

                        case TagValueRadix.ASCII:
                            result = LogixRadixConvertor.GetASCIIString(bytes);
                            break;
                    }
                    break;

                case TagDataTypeFamily.AtomicDecimal:
                    switch (this.Radix)
                    {
                        case TagValueRadix.Decimal:
                            result = LogixRadixConvertor.GetNumericString(bytes);
                            break;

                        case TagValueRadix.Hex:
                            result = LogixRadixConvertor.GetHexString(bytes);
                            break;

                        case TagValueRadix.Binary:
                            result = LogixRadixConvertor.GetBinaryString(bytes);
                            break;

                        case TagValueRadix.ASCII:
                            result = LogixRadixConvertor.GetASCIIString(bytes);
                            break;
                    }
                    break;

                case TagDataTypeFamily.AtomicBoolArray:
                    switch (this.Radix)
                    {
                        case TagValueRadix.Decimal:
                            result = LogixRadixConvertor.GetNumericString(bytes);
                            break;

                        case TagValueRadix.Hex:
                            result = LogixRadixConvertor.GetHexString(bytes);
                            break;

                        case TagValueRadix.Binary:
                            result = LogixRadixConvertor.GetBinaryString(bytes);
                            break;

                        case TagValueRadix.ASCII:
                            result = LogixRadixConvertor.GetASCIIString(bytes);
                            break;
                    }
                    break;

                case TagDataTypeFamily.AtomicFloat:
                    switch (this.Radix)
                    {
                        case TagValueRadix.Decimal:
                            result = LogixRadixConvertor.GetFloatString(bytes);
                            break;

                        case TagValueRadix.Hex:
                            result = LogixRadixConvertor.GetHexString(bytes);
                            break;

                        case TagValueRadix.Binary:
                            result = LogixRadixConvertor.GetBinaryString(bytes);
                            break;

                        case TagValueRadix.ASCII:
                            result = LogixRadixConvertor.GetASCIIString(bytes);
                            break;
                    }
                    break;

                case TagDataTypeFamily.Structure:
                    switch (this.Radix)
                    {
                        case TagValueRadix.Decimal:
                            result = LogixRadixConvertor.GetDecimalString(bytes);
                            break;

                        case TagValueRadix.Hex:
                            result = LogixRadixConvertor.GetHexString(bytes);
                            break;

                        case TagValueRadix.Binary:
                            result = LogixRadixConvertor.GetBinaryString(bytes);
                            break;

                        case TagValueRadix.ASCII:
                            result = LogixRadixConvertor.GetASCIIString(bytes);
                            break;
                    }
                    break;


                default:
                    break;
            }

            return result;
        }
        /// <summary>
        /// Преобразовывает последовательность байт в виде представления массива для текущего тэга с его определенным типом данных 
        /// а также определенной системой исчисления в строковое представление.
        /// </summary>
        /// <param name="value">Последовательность байт в виде представления массива для преобразования.</param>
        /// <param name="separator">Строковый разделитель между элементами массива.</param>
        /// <returns></returns>
        private string GetValueText(List<byte[]> value, string separator)
        {
            if (value == null || value.Count == 0)
            {
                return null;
            }

            if (this.Type.ArrayDimension.HasValue)
            {
                List<string> result = new List<string>();
                for (int ix = 0; ix < value.Count; ix++)
                {
                    string v = GetValueText(value[ix]);

                    if (v == null)
                    {
                        v = "";
                    }

                    result.Add(v);
                }

                return String.Join(separator, result);
            }
            else
            {
                return GetValueText(value[0]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private byte[] SetValueText(string s)
        {
            // Проверяем что входная строка имеет значение.
            if (s == null || s.Trim() == "")
            {
                return null;
            }

            // Проверяем что текущий тип данных существует и его код не равен 0.
            if (this.Type == null || this.Type.Code == 0)
            {
                return null;
            }

            byte[] bytes = null;

            // Проверяем является ли данный тип данных структурой.
            if (this.Type.Family != TagDataTypeFamily.Structure)
            {
                switch (this.Radix)
                {
                    case TagValueRadix.Decimal:
                        {
                            if (this.Type.Family == TagDataTypeFamily.AtomicBool)
                            {
                                byte? boolValue = LogixRadixConvertor.FromBoolString(s);
                                if (boolValue != null)
                                {
                                    bytes = new byte[] { (byte)boolValue };
                                }

                            }
                            else if (this.Type.Family == TagDataTypeFamily.AtomicFloat)
                            {
                                bytes = LogixRadixConvertor.FromFloatString(s);
                            }
                            else
                            {
                                bytes = LogixRadixConvertor.FromNumericString(s, this.Type.Size);
                            }
                        }
                        break;

                    case TagValueRadix.Hex:
                        bytes = LogixRadixConvertor.FromHexString(s, this.Type.Size);
                        break;

                    case TagValueRadix.Binary:
                        bytes = LogixRadixConvertor.FromBinaryString(s, this.Type.Size);
                        break;

                    case TagValueRadix.ASCII:
                        bytes = LogixRadixConvertor.FromASCIIString(s, this.Type.Size);
                        break;
                }
            }

            return bytes;
        }
        /* ================================================================================================== */
        #endregion

    }
}
