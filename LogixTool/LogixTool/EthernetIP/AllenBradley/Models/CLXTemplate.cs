using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogixTool.EthernetIP.AllenBradley.Models
{
    /// <summary>
    /// Представляет собой основное предстваление структурного типа данных в удаленном устройстве. 
    /// </summary>
    public class CLXTemplate
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        private string _Name;
        /// <summary>
        /// Название структуры.
        /// </summary>
        public string Name
        {
            get
            {
                if (this._Name != null)
                {
                    return this._Name.Split(";".ToCharArray())[0];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this._Name = value;
            }
        }

        /// <summary>
        /// Возвращает или задает тип кода данных. 
        /// </summary>
        public CLXSymbolTypeAttribute SymbolTypeAttribute { get; set; }
        /// <summary>
        /// Размер данных который необходимо запросить у удаленного устройства 
        /// для получения определения членов структуры методом (Service Code: "0x4C", Class: "0x6C").
        /// Размерность представлена в кол-ве 32-разрядных слов.
        /// </summary>
        public UInt32 SizeOfMembersDefinition { get; set; }
        /// <summary>
        /// Общий размр хранимых данных структуры (Байт). 
        /// </summary>
        public UInt32 SizeOfStructure { get; set; }
        /// <summary>
        /// Кол-во членов определенных в данной структуре.
        /// </summary>
        public UInt16 MemberCount { get; set; }
        /// <summary>
        /// Контрольная сумма данной структуры.
        /// </summary>
        public UInt16 CRC { get; set; }

        private List<CLXTemplateMember> _Members;
        /// <summary>
        /// Список членов структуры.
        /// </summary>
        public CLXTemplateMember[] Members
        {
            get
            {
                return this._Members.ToArray();
            }
        }
        /* ================================================================================================== */
        #endregion

        #region [ VARIABLES ]
        /* ================================================================================================== */
        /// <summary>
        /// Словарь всех элементов структуры с ключем имени элемента структуры для быстрого поиска по имени. 
        /// </summary>
        private Dictionary<string, CLXTemplateMember> membersByName;
        /// <summary>
        /// Словарь всех элементов структуры с ключем значения смещения положения в байтах в массиве значений
        /// элемента структуры для быстрого поиска по значения смещения. 
        /// </summary>
        private Dictionary<uint, List<CLXTemplateMember>> membersByByteOffset;
        /// <summary>
        /// Переменная указывает на то что необходимо перестроение словаря имен членов.
        /// </summary>
        private bool requiredToRebuildNamedMembers;
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Создает новое описание структуры.
        /// </summary>
        /// <param name="name">Название структуры.</param>
        /// <param name="sizeOfMembersDefinition">Кол-во 32-разрядных слов которые необходимо запросить для получения определения членов структуры.</param>
        /// <param name="sizeOfStructure">Общий размр хранимых данных структуры (Байт). </param>
        /// <param name="memberCount">Кол-во членов определенных в данной структуре.</param>
        /// <param name="crc">Контрольная сумма данной структуры.</param>
        public CLXTemplate(string name, UInt16 typeCode, UInt32 sizeOfMembersDefinition, UInt32 sizeOfStructure, UInt16 memberCount, UInt16 crc)
        {
            this.Name = name;
            this.SymbolTypeAttribute = new CLXSymbolTypeAttribute (typeCode);
            this.SizeOfMembersDefinition = sizeOfMembersDefinition;
            this.SizeOfStructure = sizeOfStructure;
            this.MemberCount = memberCount;
            this.CRC = crc;
            this._Members = new List<CLXTemplateMember>();

            this.requiredToRebuildNamedMembers = true;
            this.membersByName = new Dictionary<string, CLXTemplateMember>();
            this.membersByByteOffset = new Dictionary<uint, List<CLXTemplateMember>>();
        }

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Добавляет новый элемент структуры в контейнер.
        /// </summary>
        /// <param name="m"></param>
        public void AddMember(CLXTemplateMember m)
        {
            this._Members.Add(m);
            m.ParrentTemplate = this;
            this.requiredToRebuildNamedMembers = true;
        }
        /// <summary>
        /// Получает элемент структуры по имени при условии что имя имеет хотя бы один символ.
        /// </summary>
        /// <param name="name">Название элемента структуры.</param>
        /// <param name="member">Элемент структуры.</param>
        /// <returns></returns>
        public bool GetMember(string name, out CLXTemplateMember member)
        {
            if (this.requiredToRebuildNamedMembers)
            {
                RebuildNamedMembers();
            }

            if (this.membersByName.ContainsKey(name))
            {
                member = this.membersByName[name];
                return true;
            }
            else
            {
                member = null;
                return false;
            }
        }
        /// <summary>
        /// Получает элемент структуры по имени при условии что имя имеет хотя бы один символ.
        /// </summary>
        /// <param name="offset">Смещение в байтах элемента структуры.</param>
        /// <param name="members">Элементы структуры.</param>
        /// <returns></returns>
        public bool GetMember(uint offset, out List<CLXTemplateMember> members)
        {
            if (this.requiredToRebuildNamedMembers)
            {
                RebuildNamedMembers();
            }

            if (this.membersByByteOffset.ContainsKey(offset))
            {
                members = this.membersByByteOffset[offset];
                return true;
            }
            else
            {
                members = null;
                return false;
            }
        }
        /// <summary>
        /// Очищает контейнер с элементами структуры.
        /// </summary>
        public void ClearMembers()
        {
            foreach (CLXTemplateMember m in this._Members)
            {
                m.ParrentTemplate = null;
            }

            this._Members.Clear();
            RebuildNamedMembers();

        }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Производит перестроение словаря элементов структуры.
        /// </summary>
        private void RebuildNamedMembers()
        {
            this.membersByName.Clear();

            // Составляем словарь элементов структуры с ключами соответствующим их именам.
            foreach (CLXTemplateMember m in this._Members)
            {
                if (m != null && m.Name != null && m.Name.Trim() != "")
                {
                    if (!this.membersByName.ContainsKey(m.Name))
                    {
                        this.membersByName.Add(m.Name, m);
                    }
                    else
                    {
                        // TODO
                    }
                }
            }

            // Составляем словарь элементов структуры с ключами соответствующим их смещениям в байтах.
            foreach (CLXTemplateMember m in this._Members)
            {
                if (m != null && m.SymbolTypeAttribute.Code!=0x00)
                {
                    uint key = m.Offset;
                    if (!this.membersByByteOffset.ContainsKey(key))
                    {
                        this.membersByByteOffset.Add(key, new List<CLXTemplateMember>());
                    }

                    this.membersByByteOffset[key].Add(m);
                }
            }

            this.requiredToRebuildNamedMembers = false;
        }
    }
}