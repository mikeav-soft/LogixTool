using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogixTool.Common;

namespace LogixTool.Logix.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class TagMember : DataTypeMember
    {
        #region [ PROPERTIES ]
        /* ============================================================================== */
        /// <summary>
        /// Семейство типа данных данного тэга.
        /// </summary>
        public DataTypeFamily Family
        {
            get
            {
                DataTypeFamily family = DataTypeFamily.NULL;
                LogixL5X root = this.GetRoot<LogixL5X>();

                if (root != null && root.DataTypes != null)
                {
                    DataType d;
                    if (root.DataTypes.TryGetValue(this.DataType, out d))
                    {
                        return d.Family;
                    }
                }

                return family;
            }
        }
        /// <summary>
        /// Комментарий тэга.
        /// </summary>
        public LangText Comment
        {
            get
            {
                List<Tree> members = new List<Tree>();
                Tree member = this;
                for (int ix = 0; ix < 100 && member != null; ix++)
                {

                    if (member is Tag)
                    {
                        if (((Tag)member).Comments.Values.Count > 0)
                        {
                            string key = String.Join(".", members.Select(n => n.ID).ToArray());
                            key = "." + key.Replace(".[", "[");
                            if (((Tag)member).Comments.ContainsKey(key))
                            {
                                return ((Tag)member).Comments[key];
                            }
                        }
                    }
                    members.Insert(0, member);
                    member = member.Parrent;
                }
                return null;
            }
        }
        /// <summary>
        /// Указывает номер индекса в массиве, если данный элемент есть элемент массива.
        /// Индекс может являться как целочисленным (int) так и тэгом (TagMember).
        /// </summary>
        public ArrayIndex ArrayIndex { get; set; }
        /// <summary>
        /// Возвращает значение текущего объекта.
        /// </summary>
        public List<byte> Value
        {
            get
            {
                // Получаем коренной элемент тэга данного дерева.
                List<TagMember> tagMembers = this.GetRootBranch<TagMember>();
                // Проверка состояния коренного элемента тэга.
                if (tagMembers == null || tagMembers.Count <= 0 || !(tagMembers[0] is Tag))
                {
                    return null;
                }


                int byteOffset = 0;             // Вычисляемое смещение в пространстве байт для искомого элемента.
                int size = 0;                   // Вычисляемый размер в байтах искомого элемента.
                byte? bitOffset = null;         // Вычисляемое дополнительное смещение в битах для искомого эелемента.

                Tag tag = null;                 // Коренной элемент дерева данной структуры.
                DataType currDataType = null;   // Текущий элемент 

                for (int ix = 0; ix < tagMembers.Count; ix++)
                {
                    object objMember = tagMembers[ix];

                    bool isFirstIndex = ix == 0;
                    bool isLastIndex = ix == tagMembers.Count - 1;
                    

                    // Проверяем что текущий элемент не равен Null.
                    if (objMember == null)
                    {
                        return null;
                    }

                    // Для каждого элемента вычисляем смещение.
                    if (isFirstIndex)
                    {
                        #region [ Index = 0; класс "Tag" ]
                        /* ============================================================================== */
                        // Проверяем что первый элемент именно Tag.
                        if (objMember.GetType() != typeof(Tag))
                        {
                            return null;
                        }
                        // Привеодим к типу Tag.
                        tag = (Tag)objMember;

                        // Получаем определение типа данных для текущего элемента
                        // для использования в следующей итерации.
                        currDataType = tag.GetDataTypeDefinition();
                        /* ============================================================================== */
                        #endregion
                    }
                    else
                    {
                        // Привеодим к типу TagMember.
                        TagMember member = (TagMember)objMember;

                        if (member.ArrayIndex == null)
                        {
                            #region [ Index > 0; класс "TagMember" ]
                            /* ============================================================================== */
                            // Получаем член типа данных структуры который определяет данный элемент.
                            DataTypeMember currDataTypeMember = currDataType.GetChild<DataTypeMember>(member.ID);
                            if (currDataTypeMember == null)
                            {
                                return null;
                            }

                            // Получаем смещение в байтах для текущего члена типа данных.
                            uint? memberByteOffset = currDataTypeMember.ByteOffset;
                            if (memberByteOffset == null)
                            {
                                return null;
                            }

                            // Получаем смещение в битах если таковое присутствет.
                            bitOffset = currDataTypeMember.BitOffset;

                            // Вычисляем смещение для текущего элемента массива.
                            byteOffset += (int)memberByteOffset;

                            // Получаем определение типа данных для текущего элемента
                            // для использования в следующей итерации.
                            currDataType = member.GetDataTypeDefinition();
                            /* ============================================================================== */
                            #endregion
                        }
                        else
                        {
                            #region [ Index > 0; элемент массива ]
                            /* ============================================================================== */
                            if (member.ArrayIndex.IsExplict)
                            {
                                // Получаем размер для текущего типа данных.
                                // Проверяем что для текущего типа данных существует значение размера структуры и оно больше чем 0.
                                if (currDataType.Size == null)
                                {
                                    return null;
                                }  
                                int dataTypeSize = (int)currDataType.Size;
                                if (dataTypeSize == 0)
                                {
                                    return null;
                                }

                                // Вычисляем смещение для текущего элемента массива.
                                byteOffset += (int)member.ArrayIndex.LinearPosition * dataTypeSize;
                            }
                            else
                            {
                                return null;
                            }

                            // Получаем определение типа данных для текущего элемента
                            // для использования в следующей итерации.
                            currDataType = member.GetDataTypeDefinition();
                            /* ============================================================================== */
                            #endregion
                        }
                    }

                    // Текущий индекс является последним.
                    // Получаем из текущего типа данных размер данных.
                    if (isLastIndex)
                    {
                        // Привеодим к типу TagMember.
                        TagMember member = (TagMember)objMember;

                        // Получаем размер для текущего типа данных.
                        if (currDataType == null || currDataType.Size == null)
                        {
                            return null;
                        }

                        // Вычисляем размер получаемых данных в байтах.
                        if (member.Dimension!=null && member.ArrayIndex == null)
                        {
                            // Выбран элемент как массив.
                            size = (int)currDataType.Size * member.Dimension.Length;
                        }
                        else
                        {
                            size = (int)currDataType.Size;
                        }

                        // Проверяем что у тэга есть общий массив значений.
                        if (tag == null || tag.SourceMemorySpace == null)
                        {
                            return null;
                        }

                        // Проверяем что расчитанная длина необходимых данных которые необходимо получить и смещение
                        // в сумме меньше или равно длине общего массива значений.
                        if (tag.SourceMemorySpace.Count < byteOffset + size)
                        {
                            return null;
                        }

                        return tag.SourceMemorySpace.GetRange(byteOffset, size);
                    }
                }



                return null;
            }
        }


        /// <summary>
        /// Источник данных для текущего тэга.
        /// Равен нулю для всех членов кроме коренного тэга.
        /// </summary>
        protected List<byte> SourceMemorySpace { get; set; }
        /* ============================================================================== */
        #endregion

        /// <summary>
        /// Инициализирует новый структурный элемент тэга.
        /// </summary>
        public TagMember(string name)
            : base(name)
        {
            this.ArrayIndex = null;
            this.SourceMemorySpace = null;
        }
        /// <summary>
        /// Создает новый структурный элемент тэга на основе DataTypeMember.
        /// </summary>
        /// <param name="member"></param>
        public TagMember(DataTypeMember member)
            : this("")
        {
            this.DataType = member.DataType;
            this.Description = member.Description;
            this.Dimension = member.Dimension;
            this.ExternalAccess = member.ExternalAccess;
            this.ID = member.ID;
            this.Radix = member.Radix;
        }

        #region [ PUBLIC METHODS METHODS ]
        /* ============================================================================== */
        /// <summary>
        /// Построить следующие дочерние элементы для данного тэга.
        /// </summary>
        /// <param name="dataTypes">Все типы данных для построения.</param>
        public void BuildNext()
        {
            LogixL5X root = this.GetRoot<LogixL5X>();

            if (root == null || root.DataTypes == null)
            {
                return;
            }


            if (this.Dimension != null && this.Dimension.Length>0)
            {
                foreach (string index in this.Dimension.GetIndexSequence())
                {
                    TagMember tag = new TagMember("[" + index + "]");
                    tag.DataType = this.DataType;
                    this.Add(tag);
                }
            }
            else
            {
                DataType dataType;
                if (root.DataTypes.TryGetValue(this.DataType, out dataType))
                {
                    foreach (DataTypeMember dataTypeMember in dataType.Childrens.Values)
                    {
                        if (dataTypeMember.GetType() == typeof(DataTypeMember))
                        {
                            TagMember tagMember = new TagMember((DataTypeMember)dataTypeMember);
                            this.Add(tagMember);
                        }
                        else if (dataTypeMember.GetType() == typeof(DataTypeAddonParameter))
                        {
                            TagAddonParameter tagParameter = new TagAddonParameter((DataTypeAddonParameter)dataTypeMember);
                            this.Add(tagParameter);
                        }
                    }
                }

            }
        }
        /// <summary>
        /// Построить следующий дочерний элемент для данного тэга по заданному имени.
        /// </summary>
        /// <param name="name">Имя запрашегоемого тэга для построения.</param>
        public TagMember BuildNext(string name)
        {
            LogixL5X root = this.GetRoot<LogixL5X>();
            if (name == null || root == null || root.DataTypes == null)
            {
                return null;
            }

            TagMember nextTagMember = null;     // результат следующего построенного тэга.

            // Случай если следующий элемент является массивом.
            if (this.Dimension != null && this.Dimension.Length > 0)
            {
                if (name.StartsWith("[") && name.EndsWith("]"))
                {
                    TagMember tag = new TagMember(name);
                    tag.DataType = this.DataType;
                    nextTagMember = tag;
                    this.Add(tag);
                }
            }
            // Случай если следующий элемент является структурным тэгом.
            else
            {
                DataType dataType;
                DataTypeMember dataTypeMember;

                // Ищем DataType и DataTypeMember.
                if (root.DataTypes.TryGetValue(this.DataType, out dataType))
                {
                    dataTypeMember = dataType.GetChild<DataTypeMember>(name);
                    if (dataTypeMember != null)
                    {
                        if (dataTypeMember.GetType() == typeof(DataTypeMember))
                        {
                            TagMember tagMember = new TagMember((DataTypeMember)dataTypeMember);
                            this.Add(tagMember);
                            nextTagMember = tagMember;
                        }
                        else if (dataTypeMember.GetType() == typeof(DataTypeAddonParameter))
                        {
                            TagAddonParameter tagParameter = new TagAddonParameter((DataTypeAddonParameter)dataTypeMember);
                            this.Add(tagParameter);
                            nextTagMember = tagParameter;
                        }
                    }
                }
            }
            return nextTagMember;
        }
        /// <summary>
        /// Копирует поля в новый объект, где связи пусты.
        /// </summary>
        /// <returns></returns>
        public new TagMember Clone()
        {
            TagMember tagMember = (TagMember)this.MemberwiseClone();
            tagMember.Childrens = new Dictionary<string, Tree>();
            tagMember.Parrent = null;
            tagMember.CrossRefference = new List<CrossReferenceItem>();
            return tagMember;
        }
        /// <summary>
        /// Преобразование объекта в строку.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<TagMember> tagMembers = this.GetRootBranch<TagMember>();
            string fullName = String.Join(".", tagMembers.Select(n => n.ID));
            fullName = fullName.Replace(".[", "[");
            return fullName;
        }
        /* ============================================================================== */
        #endregion
    }
}
