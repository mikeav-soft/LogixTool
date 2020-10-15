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
    public class TagAddonParameter : TagMember
    {
        /// <summary>
        /// Требуемый параметр.
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// Отображение параметра.
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// Использование параметра.
        /// </summary>
        public ParameterUsage Usage { get; set; }

        /// <summary>
        /// Инициализирует новый структурный элемент тэга (Add-On parameter).
        /// </summary>
        public TagAddonParameter(string name)
            : base(name)
        {
            this.Required = false;
            this.Visible = false;
            this.Usage = ParameterUsage.Null;
        }
        /// <summary>
        /// Создает новый структурный элемент тэга (Add-On parameter) на основе DataTypeParameter.
        /// </summary>
        /// <param name="member"></param>
        public TagAddonParameter(DataTypeAddonParameter member)
            : this("")
        {
            this.DataType = member.DataType;
            this.Description = member.Description;
            this.Dimension = member.Dimension;
            this.ExternalAccess = member.ExternalAccess;
            this.ID = member.ID;
            this.Radix = member.Radix;

            this.Required = member.Required;
            this.Visible = member.Visible;
            this.Usage = member.Usage;
        }

        /// <summary>
        /// Копирует поля в новый объект, где связи пусты.
        /// </summary>
        /// <returns></returns>
        public new TagAddonParameter Clone()
        {
            TagAddonParameter parameter = new TagAddonParameter(this.ID);
            parameter.Childrens = new Dictionary<string, Tree>();
            parameter.Parrent = null;
            parameter.CrossRefference = new List<CrossReferenceItem>();

            return parameter;
        }
    }
}
