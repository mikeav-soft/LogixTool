using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogixTool.EthernetIP.AllenBradley.Models
{
    /// <summary>
    /// Представляет собой основное предстваление тэга в удаленном устройстве. 
    /// </summary>
    public class CLXTag
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// Возвращает или задает имя программы к которой принадлежит текущий тэг. В случае если значение равно Null тэг считается глобальным.
        /// </summary>
        public string ProgramName { get; set; }
        /// <summary>
        /// Возвращает или задает название тэга.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Возвращает или задает текущий Instance в удаленном устройстве.
        /// </summary>
        public UInt32 Instance { get; set; }
        /// <summary>
        /// Возвращает или задает тип кода данных. 
        /// </summary>
        public CLXSymbolTypeAttribute SymbolTypeAttribute { get; set; }

        /// <summary>
        /// Возвращает или задает размерность "0" массива.
        /// Если значение равно 0, то данная размерность не существует.
        /// </summary>
        public UInt32 ArrayDim0 { get; set; }
        /// <summary>
        /// Возвращает или задает размерность "1" массива.
        /// Если значение равно 0, то данная размерность не существует.
        /// </summary>
        public UInt32 ArrayDim1 { get; set; }
        /// <summary>
        /// Возвращает или задает размерность "2" массива.
        /// Если значение равно 0, то данная размерность не существует.
        /// </summary>
        public UInt32 ArrayDim2 { get; set; }

        /// <summary>
        /// Возвращает кол-во размерностей массива данного тэга.
        /// При значении 0 данный тэг не является масиивом.
        /// </summary>
        public byte ArrayRank
        {
            get
            {
                if (this.ArrayDim0 == 0)
                {
                    return 0;
                }
                else if (this.ArrayDim1 == 0)
                {
                    return 1;
                }
                else if (this.ArrayDim2 == 0)
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }
        }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instance"></param>
        /// <param name="symbolTypeAttribute"></param>
        /// <param name="arrayDim0"></param>
        /// <param name="arrayDim1"></param>
        /// <param name="arrayDim2"></param>
        public CLXTag(string program, string name, UInt32 instance, UInt16 symbolTypeAttribute, UInt32 arrayDim0, UInt32 arrayDim1, UInt32 arrayDim2)
        {
            this.ProgramName = program;
            this.Name = name;
            this.Instance = instance;
            this.SymbolTypeAttribute = new CLXSymbolTypeAttribute(symbolTypeAttribute);
            this.ArrayDim0 = arrayDim0;
            this.ArrayDim1 = arrayDim1;
            this.ArrayDim2 = arrayDim2;
        }
    }
}
