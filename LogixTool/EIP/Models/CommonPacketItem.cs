using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIP
{
    public class CommonPacketItem
    {
        /// <summary>
        /// Тип Фрагмента.
        /// </summary>
        public CommonPacketItemTypeID TypeID { get; set; }
        /// <summary>
        /// Длина предоставляемых данных.
        /// </summary>
        public UInt16 Length
        {
            get
            {
                return (UInt16)Data.Count;
            }
        }
        /// <summary>
        /// Предоставляемые данные.
        /// </summary>
        public virtual List<byte> Data { get; set; }

        /// <summary>
        /// Создает новый элемент пакета.
        /// </summary>
        public CommonPacketItem()
        {
            this.TypeID = CommonPacketItemTypeID.Address_Null;
            this.Data = new List<byte>();
        }

        /// <summary>
        /// Разбирает последовательность байт в объект со значениями из данной последовательности.
        /// В случае неверной структуры, длины или ошибок возвращает значение null.
        /// </summary>
        /// <param name="bytes">Последовательность байт.</param>
        /// <param name="remainBytes">Остаточная последовательность байт.</param>
        /// <returns></returns>
        public static CommonPacketItem Parse(List<byte> bytes, out List<byte> remainBytes)
        {
            CommonPacketItem commonPacketItem = new CommonPacketItem();
            remainBytes = bytes.ToList();

            if (bytes == null)
            {
                return null;
            }

            if (bytes.Count < 4)
            {
                return null;
            }

            ushort typeValue = (ushort)(bytes[0] | bytes[1] << 8);
            foreach (CommonPacketItemTypeID typeID in Enum.GetValues(typeof(CommonPacketItemTypeID)))
            {
                if ((uint)typeID == typeValue)
                {
                    commonPacketItem.TypeID = typeID;
                    break;
                }
            }

            ushort length = (ushort)(bytes[2] | bytes[3] << 8);

            if (bytes.Count - 4 < length)
            {
                commonPacketItem.Data = bytes.GetRange(4, bytes.Count - 4);
                remainBytes = new List<byte>();
                //return null;
            }
            else
            {
                commonPacketItem.Data = bytes.GetRange(4, length);
                remainBytes = bytes.GetRange(4 + length, bytes.Count - (4 + length));
            }
            return commonPacketItem;
        }

        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
        public virtual byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();

            bytes.Add((byte)(((ushort)this.TypeID) & 0xFF));
            bytes.Add((byte)(((ushort)this.TypeID >> 8) & 0xFF));
            bytes.Add((byte)((this.Length) & 0xFF));
            bytes.Add((byte)((this.Length >> 8) & 0xFF));
            bytes.AddRange(this.Data);

            return bytes.ToArray();
        }
    }
}
