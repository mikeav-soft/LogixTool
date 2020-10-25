using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogixTool.Common;
using LogixTool.Common.Extension;
using LogixTool.EthernetIP.AllenBradley.Models;

namespace LogixTool.EthernetIP.AllenBradley
{
    /// <summary>
    /// Тэг контроллера Allen Breadley.
    /// </summary>
    public class LogixTag : Tree
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        private string _Name;
        /// <summary>
        /// Возвращает или задает значение символьное имя тэга.
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;

                if (this._Name == null)
                {
                    this._Name = "";
                }

                byte? decimalBitNumber; // 

                // Производим преобразование имени тэга в путь стандарта CIP.
                this.SymbolicEPath = this.GetTagEpathFromName(value, out decimalBitNumber);

                // После присовения имени, присваиваем полученный номер бита в атомарном типе данных.
                this.Type.AtomicBitPosition = decimalBitNumber;
            }
        }

        /// <summary>
        /// Путь к объекту согласно спецификации CIP.
        /// </summary>
        public EPath SymbolicEPath { get; private set; }

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
        /// <param name="name"></param>
        public LogixTag(string name)
            : base(name)
        {
            this._Type = new TagDataTypeDefinition(0);

            this.ReadValue = new TagValueReading();
            this.WriteValue = new TagValueWriting();

            this.ReadEnable = true;
            this.WriteEnable = false;

            this.Name = name;
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

                SetChildrenValues(readValueControl);
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
            this.Type.Init();
            this.ReadValue.Report.Init();
            this.WriteValue.Report.Init();
            this.ReadEnable = true;
            this.WriteEnable = false;
            this.OwnerTableItem = null;

            this.Clear();
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

        /// <summary>
        /// Возвращает True в случае если заданные данные содержат корректные данные соответствующие 
        /// текущему значению размерности массива и размера типа данных.
        /// </summary>
        public bool IsValidValueDimension(List<byte[]> data)
        {
            if (this.Type.Size == 0)
            {
                return false;
            }

            if (data == null)
            {
                return false;
            }

            if (data.Count == 0)
            {
                return false;
            }

            if (!this.Type.ArrayDimension.HasValue)
            {
                if (data.Count != 1)
                {
                    return false;
                }

                if (data[0].Length != this.Type.Size)
                {
                    return false;
                }
            }
            else
            {
                if (this.Type.ArrayDimension.Value != data.Count)
                {
                    return false;
                }

                if (data.Any(g => g.Length != this.Type.Size))
                {
                    return false;
                }
            }

            return true;
        }
        /* ================================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Получает объект пути EPath из строкого имени.
        /// </summary>
        /// <param name="name">Строковое значение имени тэга.</param>
        /// <param name="bitNumber">Номер бита который требуется получать из полученного значения.</param>
        /// <returns></returns>
        private EPath GetTagEpathFromName(string name, out byte? bitNumber)
        {
            EPath epath = new EPath();
            bitNumber = null;
            //indexes = null;

            string tagName = name;
            string[] tagParts;

            if (tagName == null || tagName.Trim() == "")
            {
                return null;
            }

            tagName = tagName.Replace(" ", "");

            // Делим имя тэга на именя членов структуры.
            tagParts = tagName.Split(".".ToCharArray());

            for (int ix = 0; ix < tagParts.Length; ix++)
            {
                bool firstPart = (ix == 0);
                bool lastPart = (ix == tagParts.Length - 1);

                string memberName = tagParts[ix];
                if (!memberName.StartsWith("[") && memberName.Contains("[") && memberName.EndsWith("]"))
                {
                    string[] memberNameParts = memberName.Split("[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (memberNameParts.Length == 2)
                    {
                        // Получаем имя члена тэга или самого тэга.
                        epath.Segments.Add(new EPathSegment(memberNameParts[0]));

                        // Получаем индексы массива.
                        string[] textIndexes = memberNameParts[1].Split(",".ToCharArray());
                        UInt16[] numericIndexes = null;

                        // Проверяем что все индексы являются индексами.
                        if (textIndexes.Any(t => !t.IsDigits()))
                        {
                            return null;
                        }

                        // Преобразовываем индексы массива
                        numericIndexes = new UInt16[textIndexes.Length];
                        for (int arrix = 0; arrix < numericIndexes.Length; arrix++)
                        {
                            UInt16 res;
                            if (!UInt16.TryParse(textIndexes[arrix], out res))
                            {
                                return null;
                            }
                            numericIndexes[arrix] = res;
                        }

                        // Преобразовываем их в локальные сегменты.
                        if (numericIndexes.Length > 0)
                            epath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_MemberID, numericIndexes[0]));

                        if (numericIndexes.Length > 1)
                            epath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_MemberID, numericIndexes[1]));

                        if (numericIndexes.Length > 2)
                            epath.Segments.Add(new EPathSegment(EPathSegmentHeader.Local_MemberID, numericIndexes[2]));

                        //if (lastPart)
                        //{
                        //    indexes = numericIndexes;
                        //}
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (lastPart && memberName.IsDigits())
                    {
                        byte bit;
                        if (byte.TryParse(memberName, out bit))
                        {
                            bitNumber = bit;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        epath.Segments.Add(new EPathSegment(memberName));
                    }
                }
            }

            return epath;
        }

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
            if (bytes == null)
            {
                return null;
            }

            // Проверяем что текущий тип данных существует и его код не равен 0.
            if (this.Type == null || this.Type.Code == 0)
            {
                return null;
            }

            // Получаем массив байт по индексу элемента массива.
            byte[] itemValue = bytes.ToArray();

            if (itemValue.Length == 0 || itemValue.Length != this.Type.Size)
            {
                return null;
            }

            // Если в текущем типе данных данного тэга имеется определение структурного номера бита 
            // то извлекаем значение данного бита из текущих данных массива байт.
            if (this.Type.StructureBitPosition != null)
            {
                if (itemValue.Length == 1)
                {
                    byte[] extractedBits = BitExtractor.GetBitRange(itemValue, this.Type.StructureBitPosition.Value, 1);
                    if (extractedBits != null && extractedBits.Length == 1)
                    {
                        if (extractedBits[0] != 0)
                        {
                            itemValue = new byte[] { 0xFF };
                        }
                        else
                        {
                            itemValue = new byte[] { 0x00 };
                        }
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

            // Если в текущем типе данных данного тэга имеется определение атомарного номера бита
            // то извлекаем значение данного бита из текущих данных массива байт.
            if (this.Type.AtomicBitPosition != null)
            {
                if (itemValue.Length > 0 && itemValue.Length <= 4)
                {
                    byte[] extractedBits = BitExtractor.GetBitRange(itemValue, this.Type.AtomicBitPosition.Value, 1);
                    if (extractedBits != null && extractedBits.Length == 1)
                    {
                        if (extractedBits[0] != 0)
                        {
                            itemValue = new byte[] { 0xFF };
                        }
                        else
                        {
                            itemValue = new byte[] { 0x00 };
                        }
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

            // Проверяем является ли данный тип данных структурой.
            switch (this.Type.Family)
            {
                case TagDataTypeFamily.AtomicBool:
                    switch (this.Radix)
                    {
                        case TagValueRadix.Decimal:
                            result = LogixRadixConvertor.GetBoolString(itemValue[0]);
                            break;

                        case TagValueRadix.Hex:
                            result = LogixRadixConvertor.GetHexString(itemValue);
                            break;

                        case TagValueRadix.Binary:
                            result = LogixRadixConvertor.GetBinaryString(itemValue);
                            break;

                        case TagValueRadix.ASCII:
                            result = LogixRadixConvertor.GetASCIIString(itemValue);
                            break;
                    }
                    break;

                case TagDataTypeFamily.AtomicDecimal:
                    switch (this.Radix)
                    {
                        case TagValueRadix.Decimal:
                            if (this.Type.AtomicBitPosition != null)
                            {
                                result = LogixRadixConvertor.GetBoolString(itemValue[0]);
                            }
                            else
                            {
                                result = LogixRadixConvertor.GetNumericString(itemValue);
                            }
                            break;

                        case TagValueRadix.Hex:
                            result = LogixRadixConvertor.GetHexString(itemValue);
                            break;

                        case TagValueRadix.Binary:
                            result = LogixRadixConvertor.GetBinaryString(itemValue);
                            break;

                        case TagValueRadix.ASCII:
                            result = LogixRadixConvertor.GetASCIIString(itemValue);
                            break;
                    }
                    break;
                case TagDataTypeFamily.AtomicLong:
                    switch (this.Radix)
                    {
                        case TagValueRadix.Decimal:
                            result = LogixRadixConvertor.GetNumericString(itemValue);
                            break;

                        case TagValueRadix.Hex:
                            result = LogixRadixConvertor.GetHexString(itemValue);
                            break;

                        case TagValueRadix.Binary:
                            result = LogixRadixConvertor.GetBinaryString(itemValue);
                            break;

                        case TagValueRadix.ASCII:
                            result = LogixRadixConvertor.GetASCIIString(itemValue);
                            break;
                    }
                    break;

                case TagDataTypeFamily.AtomicFloat:
                    switch (this.Radix)
                    {
                        case TagValueRadix.Decimal:
                            result = LogixRadixConvertor.GetFloatString(itemValue);
                            break;

                        case TagValueRadix.Hex:
                            result = LogixRadixConvertor.GetHexString(itemValue);
                            break;

                        case TagValueRadix.Binary:
                            result = LogixRadixConvertor.GetBinaryString(itemValue);
                            break;

                        case TagValueRadix.ASCII:
                            result = LogixRadixConvertor.GetASCIIString(itemValue);
                            break;
                    }
                    break;

                case TagDataTypeFamily.Structure:
                    switch (this.Radix)
                    {
                        case TagValueRadix.Decimal:
                            result = LogixRadixConvertor.GetDecimalString(itemValue);
                            break;

                        case TagValueRadix.Hex:
                            result = LogixRadixConvertor.GetHexString(itemValue);
                            break;

                        case TagValueRadix.Binary:
                            result = LogixRadixConvertor.GetBinaryString(itemValue);
                            break;

                        case TagValueRadix.ASCII:
                            result = LogixRadixConvertor.GetASCIIString(itemValue);
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

        /// <summary>
        /// Устанавливает прочитанные значения дочерним элементам текущей древовидной структуры.
        /// Note: метод является эксперементальным. Был создан для "питания" значений всех эелемнтов.
        /// Делалось для гонки за оптимизацией трафика. На данный момент не имеет особого смысла.
        /// </summary>
        /// <param name="readValueControl"></param>
        private void SetChildrenValues(TagValueReading readValueControl)
        {
            // Так как в данном блоке выделяются данные для дочерних тэгов,
            // Проверяем что данные тэга - родителя существуют.
            // В противном случае никакие данные дочерним тэгам не передаем.
            if (readValueControl == null || readValueControl.Report.Data == null)
            {
                return;
            }

            // Получаем список дочерних элементов.
            IEnumerable<LogixTag> childs = this.GetChilds<LogixTag>();

            // Определяем является ли данный тэг массивом по предопределенной размерности.
            bool isArray = this.Type.ArrayDimension.Max > 0;
            // В случае если текущий элемент является массивом а в дочерних элементах не имеется определения
            // линейных индексов массива либо индексы слишком велики по сравнению с размерностью массива,
            // то считаем что структура построена неверно по отношению к поределения массива.
            if (isArray && childs.Any(c => c.Type.ArrayIndex == null || c.Type.ArrayIndex >= this.Type.ArrayDimension.Max))
            {
                return;
            }

            foreach (LogixTag treeChild in childs)
            {
                // Текущий результат извлечения данных для текущего дочернего элемента.
                bool resultOfOperation = true;
                // Данные дочернего тэга выделенные из пространства байт родителя.
                List<byte[]> data = new List<byte[]>();
                // Смещение в пространстве данных текущего тэга для дочернего тэга.
                UInt32 offset = treeChild.Type.StructureByteOffset;
                // Размер в байтах дочернего тэга.
                UInt16 size = treeChild.Type.Size;
                // Позиция бита (в случае если данный тип тэга является битом 0xC1) 
                // в пространстве данных текущего тэга для дочернего тэга.     
                uint? bitPosition = treeChild.Type.StructureBitPosition;

                // Текущий массив байт для вычислений.
                byte[] bytes;

                // Проверяем является ли текущий тэг массивом.
                if (isArray)
                {
                    #region [ ТЕКУЩИЙ ТЭГ : МАССИВ ]
                    /* ================================================================================================== */
                    // Случай когда текущий тэг является массивом.
                    // Проверяем что индекс масива дочернего элемента не превышает длину данных перед извлечением данных.
                    if (treeChild.Type.ArrayIndex < readValueControl.Report.Data.Count)
                    {
                        bytes = readValueControl.Report.Data[(int)treeChild.Type.ArrayIndex];
                        data.Add(bytes);
                    }
                    else
                    {
                        // TODO: Need Message about unsuccessful data extraction.
                        resultOfOperation = false;
                    }
                    /* ================================================================================================== */
                    #endregion
                }
                else
                {
                    #region [ ТЕКУЩИЙ ТЭГ : ПРОСТОЙ ТЭГ ]
                    /* ================================================================================================== */
                    // Случай когда текущий тэг не является массивом.
                    if (treeChild.Type.Code == 0xC1)
                    {
                        #region [ ТИП : БИТ (0xC1) ]
                        /* ================================================================================================== */
                        // Если текущий тэг является то обязательно должен быть указан номер бита.
                        // в противном случае уходим из выполняемого метода, т.к. действие не имеет смысла.
                        if (bitPosition != null)
                        {

                            // Получаем данные для текущего дочернего элемента исходя из типа данных.
                            bytes = BitExtractor.GetBitRange(this.ReadValue.Report.Data[0], offset * 8, size * 8);

                            if (bytes != null)
                            {
                                // Из полученного байта выделяем только заданный бит.
                                bytes[0] = (byte)(bytes[0] & (0x01 << (int)bitPosition));
                                data.Add(bytes);
                            }
                            else
                            {
                                // TODO: Need Message about unsuccessful data extraction.
                                resultOfOperation = false;
                            }
                        }
                        else
                        {
                            // TODO: Need Message about (Bit value == NULL)!
                            resultOfOperation = false;
                        }
                        /* ================================================================================================== */
                        #endregion
                    }
                    else
                    {
                        #region [ ТИП : ЛЮБОЙ ТИП КРОМЕ БИТА (0xC1) ]
                        /* ================================================================================================== */
                        if (treeChild.Type.AtomicBitPosition != null)
                        {
                            // Случай когда текущий элемент является атомарным битом.
                            // Передаем данные атомарного родителя в тэга атомарного бита.
                            // Дальнейшая работа по извлечению данного бита реализована на уровне получения значения в виде текста.
                            bytes = readValueControl.Report.Data[0];
                        }
                        else
                        {
                            // Получаем данные для текущего дочернего элемента исходя из типа данных.
                            bytes = BitExtractor.GetBitRange(readValueControl.Report.Data[0], offset * 8, size * 8);
                        }

                        if (bytes != null)
                        {
                            data.Add(bytes);
                        }
                        else
                        {
                            // TODO: Need Message about unsuccessful data extraction.
                            resultOfOperation = false;
                        }
                        /* ================================================================================================== */
                        #endregion
                    }
                    /* ================================================================================================== */
                    #endregion
                }

                // Производим создание отчета для дочерних элементов о проведенной операции 
                // считывания на основании данных текущего эелемна - родителя.
                treeChild.ReadValue.BeginEdition();
                treeChild.ReadValue.SetRequestPoint(new DateTime(readValueControl.Report.ServerRequestTimeStamp.Value));
                treeChild.ReadValue.SetResponsePoint(new DateTime(readValueControl.Report.ServerResponseTimeStamp.Value));

                if (readValueControl.Report.IsSuccessful == true && resultOfOperation)
                {
                    // В случае успешно завершенной операции копируем данные и устанавливаем статус.
                    treeChild.ReadValue.SetValueData(data);
                    treeChild.ReadValue.FinalizeEdition(true);
                }
                else
                {
                    // В случае не успешно завершенной операции устанавливаем только статус.
                    treeChild.ReadValue.FinalizeEdition(false);
                }
            }
        }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Преобразовывает текущий объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
