using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EIP.AllenBradley.Models;

namespace EIP.AllenBradley
{
    /// <summary>
    /// Держатель тэга контроллера Allen Breadley для работы в поточной задаче.
    /// </summary>
    public class LogixTagHandler : LogixTag
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// Возвращает или задает значение символьное имя тэга.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Возвращает или задает период требуемого интервала чтения значения тэга (милисекунд).
        /// </summary>
        public UInt32 ReadUpdateRate { get; set; }
        /// <summary>
        /// Возвращает или задает период требуемого интервала записи значения тэга (милисекунд).
        /// </summary>
        public UInt32 WriteUpdateRate { get; set; }

        /// <summary>
        /// Возвращает или задает разрешение на выполнение операции.
        /// </summary>
        public bool ReadEnable { get; set; }
        /// <summary>
        /// Возвращает или задает разрешение на выполнение операции.
        /// </summary>
        public bool WriteEnable { get; set; }
        /// <summary>
        /// Возвращает True в случае необходимости обновления значения тэга.
        /// </summary>
        public bool NeedToRead
        {
            get
            {
                // Проверяем имеется ли разрешение на чтение.
                if (!this.ReadEnable)
                {
                    return false;
                }

                return this.ReadValue.CheckOnUpdate(this.ReadUpdateRate, 5000);
            }
        }
        /// <summary>
        /// Возвращает True в случае необходимости обновления значения тэга.
        /// </summary>
        public bool NeedToWrite
        {
            get
            {
                // Проверяем имеется ли разрешение на запись.
                if (!this.WriteEnable)
                {
                    return false;
                }

                return this.WriteValue.CheckOnUpdate(this.WriteUpdateRate, 5000);
            }
        }

        /// <summary>
        /// Возвращает или задает метод применяемый для чтения значения тэга.
        /// </summary>
        public TagReadMethod ReadMethod { get; set; }
        /// <summary>
        /// Возвращает или задает метод применяемый для записи значения тэга.
        /// </summary>
        public TagWriteMethod WriteMethod { get; set; }
        /// <summary>
        /// Возвращает или задает ссылку на родительский объект используемый данный тэг.
        /// Устаналивается/снимается классом TagTask при добавлении/добавлении тэга соответственно. 
        /// </summary>
        public LogixTask OwnerTask { get; internal set; }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Создает новый тэг контроллера Allen Breadley.
        /// </summary>
        /// <param name="name"></param>
        public LogixTagHandler(string name)
            : base()
        {
            if (name == null)
            {
                throw new ArgumentNullException("name", "Constructor 'LogixTagHandler()': Argument 'name' is NULL");
            }

            this.ReadEnable = true;
            this.WriteEnable = false;

            this.Name = name;
            this.ReadUpdateRate = 1000;
            this.WriteUpdateRate = 0;

            this.ReadMethod = TagReadMethod.Table;
            this.WriteMethod = TagWriteMethod.Simple;

            this.OwnerTask = null;
        }

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Сбрасывает состояние тэга.
        /// </summary>
        public override void InitState()
        {
            base.InitState();
            this.OwnerTableItem = null;
            this.ReadEnable = true;
            this.WriteEnable = false;
        }
        /// <summary>
        /// Преобразовывает текущий объект в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
        /* ================================================================================================== */
        #endregion
    }
}
