using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LogixTool.Common;

namespace LogixTool.Logix.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class DataType : LogixTree
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        private string MESSAGE_HEADER
        {
            get
            {
                return "Data Type";
            }
        }

        /// <summary>
        /// Семейство типов.
        /// </summary>
        public DataTypeFamily Family { get; set; }
        /// <summary>
        /// Описание элемента.
        /// </summary>
        public LangText Description { get; set; }

        /// <summary>
        /// Размер текущего типа данных в байтах.
        /// </summary>
        public int? Size { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public MemoryCell MembersCell { get; set; }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Инициализирует новый элемент DataType.
        /// </summary>
        public DataType()
            : base("")
        {
            this.Family = DataTypeFamily.NULL;
            this.MembersCell = null;
        }
        /// <summary>
        /// Создает новый элемент на основе XML элемента DataType.
        /// </summary>
        /// <param name="xDataType"></param>
        public DataType(XElement xDataType)
            : this()
        {
            if (xDataType.ExistAs("DataType"))
            {
                // Преобразование Name.
                this.ID = xDataType.Attribute("Name").GetXValue("");
                // Преобразование Description.
                this.Description = new LangText(xDataType.Element("Description"));

                // Преобразование Family и Class.
                string family = xDataType.Attribute("Family").GetXValue("");
                string dataTypeClass = xDataType.Attribute("Class").GetXValue("");
                bool mapped = xDataType.Attribute("Mapped").GetXValue("")=="true";


                if (dataTypeClass == "Atomic")
                {
                    this.Family = DataTypeFamily.ATOMIC;
                }
                else if (dataTypeClass == "Predefined")
                {
                    if (family == "StringFamily")
                    {
                        this.Family = DataTypeFamily.STRING;
                    }
                    else
                    {
                        this.Family = DataTypeFamily.PREDEFINED;
                    }
                }
                else if (dataTypeClass == "User")
                {
                    if (family == "StringFamily")
                    {
                        this.Family = DataTypeFamily.STRING;
                    }
                    else if (family == "NoFamily")
                    {
                        this.Family = DataTypeFamily.USER_DEFINED;
                    }
                }
                else if (dataTypeClass == "Module")
                {
                    this.Family = DataTypeFamily.MODULE_DEFINED;
                }

                // Чтение размера типа данных в байтах из атрибута.
                if ((this.Family == DataTypeFamily.ATOMIC || this.Family == DataTypeFamily.PREDEFINED))
                {
                    if (xDataType.Attribute("Size") != null && xDataType.Attribute("Size").GetXValue(null).IsDigits())
                    {
                        this.Size = Convert.ToInt32(xDataType.Attribute("Size").GetXValue(null));
                    }
                    else
                    {
                        LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"DataType\", Name='" + this.ID + "', not contains Size value"));
                    }
                }

                // В случае если имеется предварительная разметка то создаем распределение памяти.
                if (mapped)
                {
                    this.MembersCell = new MemoryCell(32);
                }
                else if (this.Family == DataTypeFamily.PREDEFINED || this.Family == DataTypeFamily.ATOMIC)
                {
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"DataType\", Name='" + this.ID + "', not mapped"));
                }

                // Построение дочерних элементов DataTypeMember.
                XElement xMembers = xDataType.Element("Members");
                if (xMembers != null)
                {
                    foreach (XElement xmember in xMembers.Elements("Member").Where(d => d.Attribute("Hidden").GetXValue("") != "true"))
                    {
                        DataTypeMember member = new DataTypeMember(xmember);
                        this.Add(member);

                        // В случае если имеется предварительная разметка, то извлекаем атрибуты и помещам в память.
                        if (mapped)
                        {
                            uint row = Convert.ToUInt32(xmember.Attribute("Row").GetXValue(null));
                            uint pos = Convert.ToUInt32(xmember.Attribute("Pos").GetXValue(null));
                            uint length = Convert.ToUInt32(xmember.Attribute("Length").GetXValue(null));
                            this.MembersCell.Add(member.ID, member, length, row, pos);
                        }
                    }
                }
                else
                {
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"DataType\", Name='" + this.ID + "', not contains XML Element \"Members\""));
                }
            }
            else if (xDataType.ExistAs("AddOnInstructionDefinition"))
            {
                // Преобразование Name.
                this.ID = xDataType.Attribute("Name").GetXValue("");
                // Преобразование Description.
                this.Description = new LangText(xDataType.Element("Description"));
                // Преобразование Family.
                this.Family = DataTypeFamily.ADDON_INSTANCE;

                // Построение дочерних элементов DataTypeAddonParameter из параметров инструкции.
                XElement xMembers = xDataType.Element("Parameters");
                if (xMembers != null)
                {
                    foreach (XElement xparameter in xMembers.Elements("Parameter"))
                    {
                        this.Add(new DataTypeAddonParameter(xparameter));
                    }
                }
                else
                {
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"AddOnInstructionDefinition\", Name='" + this.ID + "', not contains XML Element \"Parameters\""));
                }
                // Построение дочерних элементов DataTypeAddonParameter из локальных тэгов инструкции.
                XElement xLocalTags = xDataType.Element("LocalTags");
                if (xLocalTags != null)
                {
                    foreach (XElement xlocaltag in xLocalTags.Elements("LocalTag"))
                    {
                        this.Add(new DataTypeAddonParameter(xlocaltag));
                    }
                }
                else
                {
                    LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "XML Element \"AddOnInstructionDefinition\", Name='" + this.ID + "', not contains XML Element \"LocalTags\""));
                }
            }
            else
            {
                LogixL5X.Event_ExternalMessage(this, new MessageEventArgs(this, MessageEventArgsType.Warning, MESSAGE_HEADER, "Source XML Element is not a \"DataType\" or \"AddOnInstructionDefinition\" or is Null."));
                return;
            }
        }

        /// <summary>
        /// Рекурсивный метод: Определяет размер типа данных.
        /// </summary>
        /// <param name="dataType"></param>
        public void DefineMemoryCellAndSize(Dictionary<string, DataType> allDataTypes)
        {
            MemoryCell memory = new MemoryCell(32);

            // Проверка начальных условий.
            if (this.MembersCell != null)// || this.Family == DataTypeFamily.ATOMIC)
            {
                return;
            }

            // Получаем членов для текущего типа данных.
            // В случае типов данных Add-On берем все члены кроме ParameterUsage.INOUT.
            if (this.Family == DataTypeFamily.ADDON_INSTANCE)
            {
                #region [ Рассчет ADD-ON ]
                /* ================================================================================ */
                List<DataTypeAddonParameter> members = this.GetChilds<DataTypeAddonParameter>().Where(d => d.Usage != ParameterUsage.InOut).ToList();

                uint boolPos = 0;
                uint boolRow = 0;

                // Перебираем в цикле все члены типов данных.
                for (int ix = 0; ix < members.Count; ix++)
                {
                    // Базовые типы данных.
                    if (members[ix].DataType.ToUpper() == "BOOL")
                    {
                        // Если член типа данных массив то начинаем заполнение с новой строки.
                        if (members[ix].Dimension != null)
                        {
                            memory.AddToEnd(members[ix].ID, members[ix], (uint)members[ix].Dimension.Length, true, 0);
                        }
                        else
                        {
                            // В первом случае начинаем заполнение всех BOOL элементов в первом ряду #0 пока не закончится ряд.
                            // после того как ряд завершен начинаем искать последний пустой ряд.
                            if (boolPos < 32)
                            {
                                boolPos++;
                                memory.Add(members[ix].ID, members[ix], 1, boolRow, boolPos);
                            }
                            else
                            {
                                // Берем последнюю свободную координату.
                                boolPos = memory.NextFreeCoord.Pos;
                                boolRow = memory.NextFreeCoord.Row;

                                // Если есть в последнем ряду заполнения то берем следующий ряд.
                                if (boolPos > 0)
                                {
                                    boolPos = 0;
                                    boolRow++;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Структурные типы данных. Уходим на рекурсивный рассчет.
                        DataType nextDataType;
                        if (allDataTypes.TryGetValue(members[ix].DataType, out nextDataType))
                        {
                            nextDataType.DefineMemoryCellAndSize(allDataTypes);
                            if (nextDataType.Size == null)
                            {
                                return;
                            }
                            int bytes = (int)nextDataType.Size;


                            // Если член типа данных массив то начинаем заполнение с новой строки.
                            if (members[ix].Dimension != null)
                            {
                                bytes = bytes * members[ix].Dimension.Length;
                                memory.AddToEnd(members[ix].ID, members[ix], (uint)(bytes * 8), true, 0);
                            }
                            else
                            {
                                // Если член типа данных "SINT" 
                                // то начинаем заполнение с последней позиции где следующая позиция кратна 8 битам.
                                if (nextDataType.ID == "SINT")
                                {
                                    if (memory.NextFreeCoord.Row != boolRow)
                                    {
                                        memory.AddToEnd(members[ix].ID, members[ix], (uint)(bytes * 8), 8);
                                    }
                                    else
                                    {
                                        memory.AddToEnd(members[ix].ID, members[ix], (uint)(bytes * 8), true, 0);
                                    }
                                }
                                // Если член типа данных "INT" 
                                // то начинаем заполнение с последней позиции где следующая позиция кратна 16 битам.
                                else if (nextDataType.ID == "INT")
                                {
                                    if (memory.NextFreeCoord.Row != boolRow)
                                    {
                                        memory.AddToEnd(members[ix].ID, members[ix], (uint)(bytes * 8), 16);
                                    }
                                    else
                                    {
                                        memory.AddToEnd(members[ix].ID, members[ix], (uint)(bytes * 8), true, 0);
                                    }
                                }
                                // Если член типа данных структуры или 32-х разрядные числа то 
                                // начинаем заполнение с последней позиции где следующая позиция кратна 16 битам.
                                else
                                {
                                    memory.AddToEnd(members[ix].ID, members[ix], (uint)(bytes * 8), true, 0);
                                }
                            }
                        }
                        else
                        {
                            // EXC. Тип данных не найден.
                            return;
                        }
                    }
                }
                /* ================================================================================ */
                #endregion
            }
            else
            {
                #region [ Рассчет UDT и остальных типов ]
                /* ================================================================================ */
                List<DataTypeMember> members = this.GetChilds<DataTypeMember>();

                // Перебираем в цикле все члены типов данных.
                for (int ix = 0; ix < members.Count; ix++)
                {
                    // Базовые типы данных.
                    if (members[ix].DataType.ToUpper() == "BOOL")
                    {
                        // Если член типа данных массив то начинаем заполнение с новой строки.
                        if (members[ix].Dimension != null)
                        {
                            memory.AddToEnd(members[ix].ID, members[ix], (uint)members[ix].Dimension.Length, true, 0);
                        }
                        else
                        {
                            // В первом случае начинаем заполнение всех BOOL элементов в первом ряду #0 пока не закончится ряд.
                            // после того как ряд завершен начинаем заполнение в порядке следования.
                            memory.AddToEnd(members[ix].ID, members[ix], 1, false, 0);
                        }
                    }
                    else
                    {
                        // Структурные типы данных. Уходим на рекурсивный рассчет.
                        DataType nextDataType;
                        if (allDataTypes.TryGetValue(members[ix].DataType, out nextDataType))
                        {
                            nextDataType.DefineMemoryCellAndSize(allDataTypes);
                            if (nextDataType.Size == null)
                            {
                                return;
                            }

                            int bytes = (int)nextDataType.Size;

                            // Если член типа данных массив то начинаем заполнение с новой строки.
                            if (members[ix].Dimension != null)
                            {
                                bytes = bytes * members[ix].Dimension.Length;
                                memory.AddToEnd(members[ix].ID, members[ix], (uint)(bytes * 8), true, 0);
                            }
                            else
                            {
                                // Если член типа данных "SINT" 
                                // то начинаем заполнение с последней позиции где следующая позиция кратна 8 битам.
                                if (nextDataType.ID == "SINT")
                                {
                                    memory.AddToEnd(members[ix].ID, members[ix], (uint)(bytes * 8), 8);
                                }

                                // Если член типа данных "INT" 
                                // то начинаем заполнение с последней позиции где следующая позиция кратна 16 битам.
                                else if (nextDataType.ID == "INT")
                                {
                                    memory.AddToEnd(members[ix].ID, members[ix], (uint)(bytes * 8), 16);
                                }

                                // Если член типа данных структуры или 32-х разрядные числа то 
                                // начинаем заполнение с последней позиции где следующая позиция кратна 16 битам.
                                else
                                {
                                    memory.AddToEnd(members[ix].ID, members[ix], (uint)(bytes * 8), true, 0);
                                }
                            }
                        }
                        else
                        {
                            // EXC. Тип данных не найден.
                            return;
                        }
                    }
                }
                /* ================================================================================ */
                #endregion
            }

            // Присвоение размерности
            if (this.Family != DataTypeFamily.PREDEFINED && this.Family != DataTypeFamily.ATOMIC)
            {
                this.Size = memory.RowCount * 4;
            }

            // Присвоение таблицы распределения памяти.
            this.MembersCell = memory;
        }
    }
}
