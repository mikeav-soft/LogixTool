using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIP.EthernetIP
{
    /// <summary>
    /// Создает элемент узла пути типа Segment EPATH.
    /// </summary>
    public class EPathSegment
    {
        /// <summary>
        /// Заголовок сегмента пути.
        /// </summary>
        public EPathSegmentHeader SegmentHeader { get; set; }
        /// <summary>
        /// Значение сегмента.
        /// </summary>
        public List<byte> Value { get; set; }

        /// <summary>
        /// Создает новый элемент узла EPATH.
        /// </summary>
        public EPathSegment()
        {
            this.SegmentHeader = EPathSegmentHeader.Port_Identifier_0;
            this.Value = new List<byte>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        public EPathSegment(EPathSegmentHeader header, UInt32 value)
            : this()
        {
            // Производим коррекцию в зависимости от заданного значения.
            this.SegmentHeader = header;

            // Читаем предложенное байтовое значение заголовка сегмента.
            byte segmentHeaderValue = (byte)header;

            // ___________ 1. PORT SEGMENT.
            if ((segmentHeaderValue & 0xE0) == 0x00)
            {
                this.Value.Add((byte)(value & (0xFF)));
            }
            // ___________ 2. LOCAL SEGMENT.
            else if ((segmentHeaderValue & 0xE0) == 0x20)
            {
                if (value <= 0xFF)
                {
                    segmentHeaderValue = (byte)(segmentHeaderValue & 0xFC);
                    this.Value.Add((byte)(value & (0xFF)));
                }
                else if (value <= 0xFFFF)
                {
                    segmentHeaderValue = (byte)((segmentHeaderValue & 0xFC) | 0x01);
                    this.Value.Add(0x00);
                    this.Value.AddRange(BitConverter.GetBytes((UInt16)(value & (0xFFFF))));
                }
                else if (value <= 0xFFFFFFFF)
                {
                    segmentHeaderValue = (byte)((segmentHeaderValue & 0xFC) | 0x02);
                    this.Value.Add(0x00);
                    this.Value.AddRange(BitConverter.GetBytes(value));
                }

                string enumName = Enum.GetName(typeof(EPathSegmentHeader), segmentHeaderValue);
                if (enumName != null)
                {
                    this.SegmentHeader = (EPathSegmentHeader)Enum.Parse(typeof(EPathSegmentHeader), enumName);
                }
            }
            // ___________ x. OTHER.
            else
            {
                this.Value.AddRange(BitConverter.GetBytes(value));
            }
        }


        /// <summary>
        /// Создает узел EPATH с символьным типом из заданной строки.
        /// </summary>
        /// <param name="text">Текстовое значения типа Data_Extented.</param>
        public EPathSegment(string text)
            : this()
        {
            this.SegmentHeader = EPathSegmentHeader.Data_Extented;
            if (text == null || text.Length == 0)
            {
                this.Value.Add(0x00);
                return;
            }

            // Преобразовываем строку в последовательность байт.
            byte[] textInBytes = text.ToCharArray().Select(t => (byte)t).ToArray();
            // Добавляем длину последовательности.
            this.Value.Add((byte)(textInBytes.Length & 0xFF));
            // Добавляем последовательность.
            this.Value.AddRange(textInBytes);
            // Если в последовательности не хватает одного байта, то добавляем завершающий нулевой байт.
            if ((this.Value.Count & 0x01) == 0)
            {
                this.Value.Add(0x00);
            }
        }

        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();
            result.Add((byte)this.SegmentHeader);
            result.AddRange(this.Value);

            return result.ToArray();
        }
    }
}
