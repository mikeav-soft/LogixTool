using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.EthernetIP
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonPacketFormat
    {
        /// <summary>
        /// Количество элементов в CommonPacketFormat.
        /// </summary>
        public ushort ItemCount
        {
            get
            {
                return (ushort)Items.Count;
            }
        }
        /// <summary>
        /// Контейнер с элементами CommonPacketItem.
        /// </summary>
        public List<CommonPacketItem> Items { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CommonPacketFormat()
        {
            this.Items = new List<CommonPacketItem>();
        }

        /// <summary>
        /// Разбирает последовательность байт в объект со значениями из данной последовательности.
        /// В случае неверной структуры, длины или ошибок возвращает значение null.
        /// </summary>
        /// <param name="bytes">Последовательность байт.</param>
        public static CommonPacketFormat Parse(List<byte> bytes)
        {
            CommonPacketFormat commonPacketFormat = new CommonPacketFormat();

            if (bytes == null || bytes.Count <= 2)
            {
                return null;
            }

            ushort length = (ushort)(bytes[0] | bytes[1] << 8);

            List<byte> itemBytes = bytes.GetRange(2, bytes.Count - 2);
            for (int itemIx = 0; itemIx < length; itemIx++)
            {
                CommonPacketItem item = CommonPacketItem.Parse(itemBytes, out itemBytes);
                commonPacketFormat.Items.Add(item);
            }

            if (itemBytes.Count > 0)
            {
                return null;
            }

            return commonPacketFormat;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(CommonPacketItem item)
        {
            this.Items.Add(item);
        }
        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(this.ItemCount));
            foreach (CommonPacketItem item in this.Items)
            {
                result.AddRange(item.ToBytes());
            }
            return result.ToArray();
        }
    }
}
