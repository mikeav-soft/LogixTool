using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogixTool.EthernetIP.AllenBradley.Models
{
    /// <summary>
    /// Представляет собой основное предстваление члена структурного типа данных объекта CLXTemplate в удаленном устройстве. 
    /// </summary>
    public class CLXTemplateMember
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// Возвращает или задает имя текущего члена структуры.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Возвращает размерность массива данных.
        /// Если значение равно Null, то это означает что элемент не является массивом.
        /// </summary>
        public UInt16? ArrayDimension
        {
            get
            {
                if (this.SymbolTypeAttribute.Code != 0xC1)
                {
                    return positionDefinition;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Возвращает позициию данного бита в слове, в случае если данный тип данных является битом (Code=0xC1).
        /// </summary>
        public UInt16? BitPosition
        {
            get
            {
                if (this.SymbolTypeAttribute.Code == 0xC1)
                {
                    return positionDefinition;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Возвращает или задает код типа данных.
        /// </summary>
        public CLXSymbolTypeAttribute SymbolTypeAttribute { get; set; }
        /// <summary>
        /// Возвращает или задает смещение в байтах расположения данной структуры.
        /// </summary>
        public UInt32 Offset { get; set; }
        /// <summary>
        /// Родительский элемент заголовка структуры типа данных.
        /// </summary>
        public CLXTemplate ParrentTemplate { get; set; }
        /// <summary>
        /// Возвращает элемент структуры с типом (BYTE, 0xC2), в случае если данный элемент является битом (BOOL, 0xC1),
        /// где размечен (расположен) данный элемент.
        /// </summary>
        public CLXTemplateMember CorrespondedHiddenMember
        {
            get
            {
                // Проверяем что у данного элемента имеется ссылка на структуру.
                if (this.ParrentTemplate == null)
                {
                    return null;
                }

                // Проверяем что данный элемент является битом (BOOL, 0xC1).
                if (this.SymbolTypeAttribute.Code != 0xC1)
                {
                    return null;
                }

                // Получаем все элементы структуры с таким же смещением как и у данного элемента.
                List<CLXTemplateMember> findedMembers;
                if (!this.ParrentTemplate.GetMember(this.Offset, out findedMembers))
                {
                    return null;
                }

                // Получаем все найденные элементы имя которых начинается с "ZZZZZZZZZZ" и не является текущим элементом.
                findedMembers = findedMembers.Where(f => f.Name.StartsWith("ZZZZZZZZZZ") && f != this).ToList();

                // Проверяем что найденный элемент только один.
                if (findedMembers.Count != 1)
                {
                    return null;
                }

                return findedMembers.First();
            }
        }
        /* ================================================================================================== */
        #endregion

        public UInt16 positionDefinition;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrayDimension"></param>
        /// <param name="typeCode"></param>
        /// <param name="offset"></param>
        public CLXTemplateMember(string name, UInt16 positionDefinition, UInt16 typeCode, UInt32 offset)
        {
            this.Name = name;
            this.positionDefinition = positionDefinition;
            this.SymbolTypeAttribute = new CLXSymbolTypeAttribute (typeCode);
            this.Offset = offset;
            this.ParrentTemplate = null;
        }
    }
}
