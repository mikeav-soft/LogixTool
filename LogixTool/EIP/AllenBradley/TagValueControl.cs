using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EIP.AllenBradley
{
    /// <summary>
    /// Представляет собой класс описывающий двойной буфер действия чтения/записи значения тэга 
    /// реализоавнный с целью непрерывного цикличного обновления значения объекта путем копирования
    /// радектируемого объекта по завершению в завершенный во избежании некотроллируемой перезаписи объекта
    /// внешним потоком.
    /// </summary>
    public class TagValueControl
    {
        #region [ PROPERTIES ]
        /* ======================================================================================== */
        private TagValueReport _Report;
        /// <summary>
        /// Возвращает или задает отчет о последних завершенных данных операции чтении/записи значения тэга.
        /// </summary>
        public TagValueReport Report
        {
            get
            {
                return this._Report;
            }
            set
            {
                this._Report = value;
                Event_ReportUpdated();
            }
        }
        /* ======================================================================================== */
        #endregion

        private TagValueReport editedReport;                // Текущие редакируемые данные чтения/записи значения тэга.
        private TagDataTypeDefinition dataTypeDefinition;   // Представляет собой определение типа данных.
        /// <summary>
        /// Создает новый буфер значений тэга.
        /// </summary>
        public TagValueControl(TagDataTypeDefinition type)
        {
            this.dataTypeDefinition = type;
            this.Report = new TagValueReport();
            this.editedReport = new TagValueReport();
        }

        #region [ EVENTS ]
        /* ======================================================================================== */
        /// <summary>
        /// Возникает при изменении значения отчета о последних завершенных данных операциях чтении/записи значения тэга.
        /// </summary>
        public event EventHandler ReportUpdated;
        /// <summary>
        /// Вызывает событие при изменении значения отчета о последних завершенных данных операциях чтении/записи значения тэга.
        /// </summary>
        private void Event_ReportUpdated()
        {
            if (this.ReportUpdated != null)
            {
                this.ReportUpdated(this, null);
            }
        }
        /* ======================================================================================== */
        #endregion

        #region [ INTERNAL METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Производит инициализацию переменных внутри редактируемого отчета перед началом работы с ним.
        /// </summary>
        internal virtual void BeginEdition()
        {
            if (this.editedReport == null)
            {
                this.editedReport = new TagValueReport();
            }
            this.editedReport.Init();
        }
        /// <summary>
        /// Устанавливает текущую временную отметку в редактируемый отчет запроса к удаленному устройству (контроллеру).
        /// </summary>
        internal virtual void SetRequestPoint()
        {
            SetRequestPoint(DateTime.Now);
        }
        /// <summary>
        /// Устанавливает заданную временную отметку в редактируемый отчет запроса к удаленному устройству (контроллеру).
        /// </summary>
        /// <param name="dt">Значение времени.</param>
        internal virtual void SetRequestPoint(DateTime dt)
        {
            this.editedReport.ServerRequestTimeStamp = dt.Ticks;
            this.editedReport.ServerResponseTimeStamp = null;

            if (this.Report.ServerRequestTimeStamp != null)
            {
                this.editedReport.ActualUpdateRate = (this.editedReport.ServerRequestTimeStamp - this.Report.ServerRequestTimeStamp) / 10000;
            }
            else
            {
                this.editedReport.ActualUpdateRate = null;
            }
        }
        /// <summary>
        /// Устанавливает текущую временную отметку в редактируемый отчет ответа от удаленного устройства (контроллера).
        /// </summary>
        internal virtual void SetResponsePoint()
        {
            this.SetResponsePoint(DateTime.Now);
        }
        /// <summary>
        /// Устанавливает заданную временную отметку в редактируемый отчет ответа от удаленного устройства (контроллера).
        /// </summary>
        /// <param name="dt">Значение времени.</param>
        internal virtual void SetResponsePoint(DateTime dt)
        {
            this.editedReport.ServerResponseTimeStamp = dt.Ticks;

            if (this.editedReport.ServerResponseTimeStamp != null)
            {
                this.editedReport.ServerReplyTime = (this.editedReport.ServerResponseTimeStamp - this.editedReport.ServerRequestTimeStamp) / 10000;
            }
            else
            {
                this.editedReport.ServerReplyTime = null;
            }
        }
        /// <summary>
        /// Устанавливает новое значение в редактируемый отчет в виде массива байт и делает отметку о том, 
        /// является ли новое значение отличным от предыдущего значения.
        /// </summary>
        /// <param name="data">Данные в виде массива байт.</param>
        internal virtual void SetValueData(List<byte[]> data)
        {
            data = PostDataProcessing(data);
            this.editedReport.ValueChanged = this.CheckValuesOnDifference(this.Report.Data, data);
            this.editedReport.Data = data;
        }
        /// <summary>
        /// Производит завершение процесса редактирования отчета с установкой значения об успешности оперции.
        /// После завершения выполнения данного метода редактируемый элемент становится доступный в поле
        /// Completed готовый к чтению.
        /// </summary>
        /// <param name="isSuccessful">Значение успешности операции.</param>
        internal virtual void FinalizeEdition(bool isSuccessful)
        {
            this.editedReport.IsSuccessful = isSuccessful;
            this.Report = this.editedReport.Copy();
        }
        /* ======================================================================================== */
        #endregion

        #region [ PROTECTED METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Проверяет два значения на различие между собой.
        /// Если значения с массивом байт отличаются каким-либо образом то возвращается True.
        /// </summary>
        /// <param name="firstValue">Первое значение для сравнения.</param>
        /// <param name="secondValue">Второе значение для сравнения.</param>
        /// <returns></returns>
        protected bool CheckValuesOnDifference(List<byte[]> firstValue, List<byte[]> secondValue)
        {
            // Проверяем равны ли Null два сравниваемых значения.
            if (firstValue != null && secondValue != null)
            {
                // При неравенстве двух значений Null проводим дальнейший анализ.
                // Преверяем равно ли кол-во элементов в значениях.
                if (secondValue.Count != firstValue.Count)
                {
                    // Кол-во элементво массива отличается.
                    // Считаем что значение было изменено.
                    return true;
                }
                else
                {
                    // Производим поэлементное сравнение.
                    for (int arrIx = 0; arrIx < firstValue.Count; arrIx++)
                    {
                        // Если у каких либо соответствующих сравниваемых элементов массива отличается размерность то считаем что
                        // значение было изменено.
                        if (firstValue[arrIx].Length != secondValue[arrIx].Length)
                        {
                            return true;
                        }
                        else
                        {
                            // Если у каких либо соответствующих сравниваемых элементов имеется отличие в равенстве
                            // Null то считаем что значение было изменено.
                            if ((firstValue[arrIx] != null && secondValue[arrIx] == null)
                                || (firstValue[arrIx] == null && secondValue[arrIx] != null))
                            {
                                return true;
                            }

                            // Если у каких либо соответствующих сравниваемых элементов имеется различие в
                            // значениях то считаем что значение было изменено.
                            for (int elemIx = 0; elemIx < firstValue[arrIx].Length; elemIx++)
                            {
                                if (firstValue[arrIx][elemIx] != secondValue[arrIx][elemIx])
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Если имеется отличия значений Null между двумя значениями 
                // то считаем что значение изменилось.
                if ((firstValue == null && secondValue != null)
                    || (firstValue != null && secondValue == null))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Производит пост обработку полученных данных.
        /// Цель данной проверки заключается в проверке на соответствие поступающих данных и типа данных.
        /// Также при особых случаях происходит выделение битового значения в случае если:
        /// 1. Бит данных находится в структуре.
        /// 2. Бит данных указан пользователем как составная часть атомарного числа.
        /// 3. Бит данных является элементом битового массива.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected List<byte[]> PostDataProcessing(List<byte[]> data)
        {
            // Проверяем что текущая последовательность байт не равна Null.
            if (data == null || data.Count == 0)
            {
                return null;
            }

            // Проверяем что текущий тип данных существует и его код не равен 0.
            if (this.dataTypeDefinition == null || this.dataTypeDefinition.Code == 0)
            {
                // В случае если тип данных не определен, то прекращаем попытки извлечения бита.
                // Данная ситуация может быть нормальной, т.к. существуют методы получения данных с удаленного устройства
                // не требующие обязательной информации о типе данных тэга (например Simple методы получения).
                return data;
            }

            // Проверяем что поступившие данные соответствуют определенному размеру типа данных.
            if (data.Any(t => t.Length == 0 || t.Length != this.dataTypeDefinition.Size))
            {
                return null;
            }

            List<byte[]> result = new List<byte[]>();

            switch (this.dataTypeDefinition.Family)
            {
                case TagDataTypeFamily.AtomicBool:
                    {
                        #region [ POST EXTRACTION OF STRUCTURE BITS ]
                        /* ======================================================================================== */
                        if (this.dataTypeDefinition.StructureBitPosition != null)
                        {
                            if (data.Count == 1 && data[0].Length == 1)
                            {
                                byte[] extractedBits = BitExtractor.GetBitRange(data[0], this.dataTypeDefinition.StructureBitPosition.Value, 1);

                                if (extractedBits != null && extractedBits.Length == 1)
                                {
                                    result.Add(new byte[] { 0x00 });
                                    if (extractedBits[0] != 0) result[0][0] = 0x01;
                                    return result;
                                }
                                else
                                {
                                    // TODO: Add message.
                                    return null;
                                }
                            }
                            else
                            {
                                // TODO: Add message.
                                return null;
                            }
                        }
                        /* ======================================================================================== */
                        #endregion
                    }
                    break;

                case TagDataTypeFamily.AtomicInteger:
                    {
                        #region [ POST EXTRACTION OF ATOMIC NUMERIC BITS ]
                        /* ======================================================================================== */
                        if (this.dataTypeDefinition.AtomicBitPosition != null)
                        {
                            if (data.Count == 1 && data[0].Length > 0 && data[0].Length <= 4)
                            {
                                byte[] extractedBits = BitExtractor.GetBitRange(data[0], this.dataTypeDefinition.AtomicBitPosition.Value, 1);

                                if (extractedBits != null && extractedBits.Length == 1)
                                {
                                    result.Add(new byte[] { 0x00 });
                                    if (extractedBits[0] != 0) result[0][0] = 0x01;
                                    return result;
                                }
                                else
                                {
                                    // TODO: Add message.
                                    return null;
                                }
                            }
                            else
                            {
                                // TODO: Add message.
                                return null;
                            }

                        }
                        /* ======================================================================================== */
                        #endregion
                    }
                    break;

                case TagDataTypeFamily.AtomicBoolArray:
                    {
                        #region [ POST EXTRACTION OF ATOMIC ARRAY BITS ]
                        /* ======================================================================================== */
                        if (this.dataTypeDefinition.BitArrayDWordOffset != null && this.dataTypeDefinition.BitArrayDWordBitPosition != null)
                        {
                            if (data.Count > this.dataTypeDefinition.BitArrayDWordOffset.Value && data[0].Length == 4)
                            {
                                byte[] extractedBits = BitExtractor.GetBitRange(data[(int)this.dataTypeDefinition.BitArrayDWordOffset.Value],
                                    this.dataTypeDefinition.BitArrayDWordBitPosition.Value, 1);

                                if (extractedBits != null && extractedBits.Length == 1)
                                {
                                    result.Add(new byte[] { 0x00 });
                                    if (extractedBits[0] != 0) result[0][0] = 0x01;
                                    return result;
                                }
                                else
                                {
                                    // TODO: Add message.
                                    return null;
                                }
                            }
                            else
                            {
                                // TODO: Add message.
                                return null;
                            }
                        }
                        /* ======================================================================================== */
                        #endregion
                    }
                    break;
            }

            // В том случае если необходимость извлечения бита не определена,
            // то возвращаем данные без изменения.
            return data;
        }
        /* ======================================================================================== */
        #endregion
    }
}
