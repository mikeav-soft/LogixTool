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
        /// <summary>
        /// Возвращает или задает отчет о последних завершенных данных операции чтении/записи значения тэга.
        /// </summary>
        public TagValueReport Report
        {
            get { return this.report; }
            set
            {
                this.report = value;
                Event_ReportUpdated();
            }
        }
        /* ======================================================================================== */
        #endregion

        private TagValueReport report;
        private TagValueReport editedReport;                // Текущие редакируемые данные чтения/записи значения тэга.
        private TagDataTypeDefinition type;   // Представляет собой определение типа данных.
        /// <summary>
        /// Создает новый буфер значений тэга.
        /// </summary>
        public TagValueControl(TagDataTypeDefinition type)
        {
            if (type == null)
                throw new ArgumentNullException("Class='TagDataTypeDefinition', Argument='type' can not be Null", "type");

            this.type = type;
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

        #region [ PUBLIC METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Производит сброс состояния текущего отчета.
        /// </summary>
        public virtual void Reset()
        {
            this.Report.Init();
        }
        /* ======================================================================================== */
        #endregion

        #region [ INTERNAL METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Проверяет требуется ли обновление данных для текущего контейнера данных в зависимости от требуемого периода обновления.
        /// В случае нормального периода равного нулю, обновление требуется только один раз до появления
        /// </summary>
        /// <param name="normalUpdateRate"></param>
        /// <param name="retryUpdateRate"></param>
        /// <returns></returns>
        internal virtual bool CheckOnUpdate(uint normalUpdateRate, uint retryUpdateRate)
        {
            // Проверяем имеется ли любая запись о последнем результате.
            if (this.Report.IsSuccessful == null)
            {
                return true;
            }

            uint updateRate;

            // Определяем период обновления операции в зависимости от последнего результата.
            if (this.Report.IsSuccessful == true)
                updateRate = normalUpdateRate;
            else
                updateRate = retryUpdateRate;

            // Определяем разрешение на выполнение операции.
            // Если период времени с последней записи превысил или равен требуемому периоду обновления.
            return (this.Report.ServerRequestTimeStamp == null)
                || (this.Report.ServerRequestTimeStamp > 0
                && normalUpdateRate > 0
                && (((DateTime.Now.Ticks - this.Report.ServerRequestTimeStamp) / 10000) >= updateRate));
        }
        /// <summary>
        /// Проверяет что предоставленные данные существуют и соответствуют определенному типу данных.
        /// </summary>
        /// <param name="data">Последовательность данных.</param>
        /// <returns></returns>
        internal virtual bool CheckData(List<byte[]> data)
        {
            if (this.type.ElementSize == 0)
            {
                return false;
            }

            if (data == null)
            {
                return false;
            }

            int elements = type.ArrayDimension.HasValue ? type.ArrayDimension.Value : 1;

            return elements == data.Count && data.All(e => e != null && e.Length == type.ElementSize);
        }
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
            if (this.type == null || this.type.Code == 0)
            {
                // В случае если тип данных не определен, то прекращаем попытки извлечения бита.
                // Данная ситуация может быть нормальной, т.к. существуют методы получения данных с удаленного устройства
                // не требующие обязательной информации о типе данных тэга (например Simple методы получения).
                return data;
            }

            // Проверяем что поступившие данные соответствуют определенному размеру типа данных.
            if (data.Any(t => t.Length == 0 || t.Length != this.type.ElementSize))
            {
                return null;
            }

            List<byte[]> result = new List<byte[]>();

            switch (this.type.Family)
            {
                case TagDataTypeFamily.AtomicBool:
                    {
                        #region [ POST EXTRACTION OF STRUCTURE BITS ]
                        /* ======================================================================================== */
                        if (this.type.StructureDefinition != null && this.type.StructureDefinition.BitOffset.HasValue)
                        {
                            if (data.Count == 1 && data[0].Length == 1)
                            {
                                byte[] extractedBits = BitExtractor.GetBitRange(data[0], this.type.StructureDefinition.BitOffset.Value, 1);

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
                        if (this.type.AtomicBitDefinition != null)
                        {
                            if (data.Count == 1 && data[0].Length > 0 && data[0].Length <= 4)
                            {
                                byte[] extractedBits = BitExtractor.GetBitRange(data[0], this.type.AtomicBitDefinition.BitOffset.Value, 1);

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
                        if (this.type.BitArrayDefinition != null)
                        {
                            if (data.Count == 1 && data[0].Length == this.type.ElementSize)
                            {
                                byte[] extractedBits = BitExtractor.GetBitRange(data[0], this.type.BitArrayDefinition.BitOffset.Value, 1);

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
