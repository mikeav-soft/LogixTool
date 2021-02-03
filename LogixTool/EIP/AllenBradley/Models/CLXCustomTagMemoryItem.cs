using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EIP.AllenBradley.Models
{
    /// <summary>
    /// Элемент привязки области памяти возвращаемой контроллером с тэгом контроллера.
    /// </summary>
    public class CLXCustomTagMemoryItem
    {
        #region [ PROPERTIES ]
        /* ======================================================================================== */
        /// <summary>
        /// Идентификационный номер в таблице.
        /// </summary>
        public UInt16 ID { get; set; }
        /// <summary>
        /// Ассоциированный с текущей областью памяти тэг.
        /// </summary>
        public LogixTag Tag { get; set; }
        /// <summary>
        /// Возвращает размер занимаемой области в байтах.
        /// Идентификатор (2 байта) и значение тэга.
        /// </summary>
        public int ExpectedSizeOfResponse
        {
            get
            {
                // Возращаемый ожидаемый размер: 2 байта поля ID + сам размер тэга чтения.
                return 2 + this.Tag.Type.ExpectedTotalSize;
            }
        }
        /// <summary>
        /// Возвращает или задает ссылку на объект используемой таблицы.
        /// </summary>
        public CLXCustomTagMemoryTable ParrentTable { get; set; }
        /* ======================================================================================== */
        #endregion

        /// <summary>
        /// Создает новый элемент привязки области памяти возвращаемой контроллером с тэгом контроллера.
        /// </summary>
        /// <param name="id">Идентификатор тэга.</param>
        /// <param name="tag">Тэг контроллера.</param>
        /// <param name="parrentTable">Родительская таблица.</param>
        public CLXCustomTagMemoryItem(UInt16 id, LogixTag tag)
        {
            this.ID = id;
            this.Tag = tag;
            this.ParrentTable = null;
        }

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Устанавливает значение тэга из массива пересылаемых байт где:
        /// * байты 0...1 - условный идентификатор тэга в таблице контроллера.
        /// * байты 2...n - значение тэга.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool GetTagValueFromBytes(List<byte> bytes)
        {
            if (bytes == null || bytes.Count != this.ExpectedSizeOfResponse)
            {
                this.Tag.ReadValue.FinalizeEdition(false);
                return false;
            }

            // Проверяем что текущий ID значения соответствует ID тэга.
            UInt16 id = BitConverter.ToUInt16(bytes.ToArray(), 0);
            if (id != this.ID)
            {
                this.Tag.ReadValue.FinalizeEdition(false);
                return false;
            }

            // Получаем последовательность байт соостветствующее только значению (2 байта ID убираем).
            List<byte> value = bytes.GetRange(2, this.ExpectedSizeOfResponse - 2);
            // Получаем значение в байтах для одного элемента (массива или атомарного элемента).
            int itemSize = this.Tag.Type.Size;

            // Результат соответствующий значению тэга.
            List<byte[]> result = new List<byte[]>();

            // Делим последовательность байт на фрагменты.
            for (int ix = 0; ix < value.Count; ix += itemSize)
            {
                result.Add(value.GetRange(ix, itemSize).ToArray());
            }

            this.Tag.ReadValue.SetValueData(result);
            this.Tag.ReadValue.FinalizeEdition(true);

            return true;
        }
        /* ======================================================================================== */
        #endregion
    }
}
