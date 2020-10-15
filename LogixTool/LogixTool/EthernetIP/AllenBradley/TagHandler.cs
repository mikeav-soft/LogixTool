using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogixTool.Common;
using LogixTool.EthernetIP.AllenBradley.Models;

namespace LogixTool.EthernetIP.AllenBradley
{
    /// <summary>
    /// Держатель тэга контроллера Allen Breadley для работы в поточной задаче.
    /// </summary>
    public class TagHandler : LogixTag
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// Возвращает или задает период требуемого интервала чтения значения тэга (милисекунд).
        /// </summary>
        public UInt32 ReadUpdateRate { get; set; }
        /// <summary>
        /// Возвращает или задает период требуемого интервала записи значения тэга (милисекунд).
        /// </summary>
        public UInt32 WriteUpdateRate { get; set; }

        /// <summary>
        /// Возвращает True в случае необходимости обновления значения тэга.
        /// </summary>
        public bool NeedToRead
        {
            get
            {
                if (!this.ReadEnable)
                {
                    return false;
                }

                // Последнее чтение не произвдеено.
                if (this.ReadValue.Report.IsSuccessful == null)
                {
                    return true;
                }

                // Последнее чтение завершено удачно.
                if (this.ReadValue.Report.IsSuccessful == true)
                {
                    return this.ReadValue.Report.ServerRequestTimeStamp == null
                        || ((this.ReadValue.Report.ServerRequestTimeStamp > 0
                        && (((DateTime.Now.Ticks - this.ReadValue.Report.ServerRequestTimeStamp) / 10000) >= this.ReadUpdateRate)
                            && this.ReadUpdateRate > 0));
                }

                // Последнее чтение завершено неудачно.
                if (this.ReadValue.Report.IsSuccessful == false)
                {
                    return this.ReadValue.Report.ServerRequestTimeStamp == null ||
                        (((DateTime.Now.Ticks - this.ReadValue.Report.ServerRequestTimeStamp) / 10000) >= 5000);
                }

                return false;
            }
        }
        /// <summary>
        /// Возвращает True в случае необходимости обновления значения тэга.
        /// </summary>
        public bool NeedToWrite
        {
            get
            {
                if (!this.WriteEnable)
                {
                    return false;
                }

                if (this.WriteValue.Report.IsSuccessful == null)
                {
                    return true;
                }

                if (this.WriteValue.Report.IsSuccessful == true)
                {
                    return this.WriteValue.Report.ServerRequestTimeStamp == null
                        || ((this.WriteValue.Report.ServerRequestTimeStamp > 0
                        && ((DateTime.Now.Ticks - this.WriteValue.Report.ServerRequestTimeStamp) / 10000) >= this.WriteUpdateRate));
                }

                if (this.WriteValue.Report.IsSuccessful == false)
                {
                    return this.WriteValue.Report.ServerRequestTimeStamp == null ||
                        (((DateTime.Now.Ticks - this.WriteValue.Report.ServerRequestTimeStamp) / 10000) >= 5000);
                }

                return false;
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
        public TagTask OwnerTask { get; internal set; }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Создает новый тэг контроллера Allen Breadley.
        /// </summary>
        /// <param name="name"></param>
        public TagHandler(string name)
            : base(name)
        {
            this.ReadUpdateRate = 1000;
            this.WriteUpdateRate = 1000;

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
            this.Clear();
        }
        /* ================================================================================================== */
        #endregion
    }
}
