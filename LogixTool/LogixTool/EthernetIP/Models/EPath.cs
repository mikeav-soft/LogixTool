using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.EthernetIP
{
    /// <summary>
    /// Представляет собой маршрут до объекта CIP.
    /// </summary>
    public class EPath
    {
        /// <summary>
        /// Возвращает Размер маршрута выраженного в 16-ти битных словах.
        /// </summary>
        public byte Size
        {
            get
            {
                byte count = (byte)this.Segments.SelectMany(t => t.ToBytes()).Count();
                if ((count & 0x01) == 1)
                {
                    return (byte)(count/2 + 1);
                }
                else
                {
                    return (byte)(count / 2);
                }
            }
        }
        /// <summary>
        /// Возвращает или задает Сегменты пути.
        /// </summary>
        public List<EPathSegment> Segments { get; set; }

        /// <summary>
        /// Создат маршрут до объекта CIP.
        /// </summary>
        public EPath()
        {
            this.Segments = new List<EPathSegment>();
        }

        /// <summary>
        /// Преобразовывает данный объект в масив Байт.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes(EPathToByteMethod method)
        {
            // Формируем результат.
            List<byte> result = new List<byte>();

            // Размер байт в пакете.
            if (method == EPathToByteMethod.Complete)
            {
                result.Add(this.Size);
            }

            // Преобразовываем сегменты в последовательность байт.
            List<byte> value = new List<byte>();
            foreach (EPathSegment segment in this.Segments)
            {
                value.AddRange(segment.ToBytes());
            }

            result.AddRange(value);
            return result.ToArray();
        }
    }
}
