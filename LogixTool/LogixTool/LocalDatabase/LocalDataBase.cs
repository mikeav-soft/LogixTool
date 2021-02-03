using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Forms;
using System.IO;
using LogixTool.Common;
using LogixTool.Common.Extension;

namespace LogixTool.LocalDatabase
{
    /// <summary>
    /// Центролизованная база данных настроек приложения.
    /// </summary>
    public class Storage
    {
        /// <summary>
        /// Константа. Название корневой папки локальной базы данных.
        /// </summary>
        public const string DIR_ROOT = "data";
        /// <summary>
        /// Директория локальной базы данных.
        /// </summary>
        public string StorageDirectory
        {
            get
            {
                return Environment.CurrentDirectory + "\\" + DIR_ROOT;
            }
        }

        #region [ EVENTS ]
        /* ======================================================================================== */
        /// <summary>
        /// Возникает при появлении каких либо сообщений при работе ядра.
        /// </summary>
        public static event MessageEvent Messages;
        /// <summary>
        /// Возникает при появлении каких либо сообщений при работе ядра.
        /// </summary>
        public event MessageEvent Message;
        /// <summary>
        /// Вызывает событие с сообщением.
        /// </summary>
        /// <param name="e"></param>
        private void Event_Message(MessageEventArgs e)
        {
            MessageEventArgs messageEventArgs = e;
            string messageHeader = "[Local Database]." + messageEventArgs.Header;
            messageEventArgs.Header = messageHeader;

            if (this.Message != null)
            {
                this.Message(this, messageEventArgs);
            }

            if (Messages != null)
            {
                Messages(this, messageEventArgs);
            }
        }
        /* ======================================================================================== */
        #endregion

        #region [ PUBLIC METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Сохраняет данные *.xml в хранилище.
        /// </summary>
        /// <param name="owner">Владелец данных. Обычно приложение или сервис.</param>
        /// <param name="type">Тип данных.</param>
        /// <param name="id">Иденификатор данных.</param>
        /// <param name="xelem">Фрагмент *.xml данных.</param>
        /// <returns></returns>
        public bool Put(StoreOwner owner, StoreType type ,string id, XElement xelem)
        {
            string header = "PutData";

            // Получаем атрибуты элементов перечислений.
            OwnerAttribute ownerAttribute = GetStoreOwnerTextAttribute(owner);
            TypeAttribute typeAttribute = GetStoreTypeTextAttribute(type);
            string curr_id = "";

            if (id != null && id.Trim() != "")
            {
                curr_id = id;
            }

            // Проверяем входные параметры.
            if (xelem == null || ownerAttribute == null || typeAttribute == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Incorrect input parameters."));
                return false;
            }

            // Получаем специальное имя файла.
            string filename = GetFileName(type, curr_id);

            // Формируем корневые xml элементы
            XElement xstore = new XElement("storage");
            xstore.Add(new XAttribute("Owner", ownerAttribute.Name));
            xstore.Add(new XAttribute("Type", typeAttribute.Name));
            xstore.Add(new XAttribute("ID", curr_id));
            xstore.Add(new XAttribute("Saved", DateTime.Now.Ticks.ToString()));

            XElement xdata = new XElement("data");
            xstore.Add(xdata);
            xdata.Add(xelem);

            // Проверяем создана ли специфическая директория для файла, если нет то создаем ее.
            DirectoryInfo di = new DirectoryInfo(StorageDirectory + "\\" + ownerAttribute.Name);
            if (!di.Exists)
            {
                try
                {
                    di.Create();
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + curr_id + ". Folder was created: " + di.FullName + "."));
                }
                catch
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + curr_id + ". Imposible to create folder: " + di.FullName + "."));
                    return false;
                }
            }

            // Сохраняем данные в xml файл.
            string fileName = di.FullName + "\\" + filename;

            try
            {
                XDocument xdoc = new XDocument();
                xdoc.Add(xstore);
                xdoc.Save(fileName);

                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Info, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + curr_id + ". XML file was created: " + fileName + "."));
                return true;
            }
            catch
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + curr_id + ". Imposible to create XML file: " + fileName + "."));
                return false;
            }
        }
        /// <summary>
        /// Получает данные из хранилища по конкретному идентификатору.
        /// </summary>
        /// <param name="owner">Владелец данных. Обычно приложение или сервис.</param>
        /// <param name="type">>Тип данных.</param>
        /// <param name="curr_id">Иденификатор данных.</param>
        /// <param name="info">Полученная информация из хранилища.</param>
        /// <returns></returns>
        public bool Get(StoreOwner owner, StoreType type, string id, out StorageItemInfo info)
        {
            string header = "GetData";
            string curr_id = "";

            if (id != null && id.Trim() != "")
            {
                curr_id = id;
            }

            info = new StorageItemInfo(owner, type, curr_id, null, null, null);

            // Получаем атрибуты элементов перечислений.
            OwnerAttribute ownerAttribute = GetStoreOwnerTextAttribute(owner);
            TypeAttribute typeAttribute = GetStoreTypeTextAttribute(type);


            // Проверяем входные параметры.
            if (ownerAttribute == null || typeAttribute == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Incorrect input parameters."));
                return false;
            }

            // Получаем специальное имя файла.
            string filename = GetFileName(type, curr_id);

            // Проверяем существует ли данный файл и проверяем его содержимое.
            FileInfo fi = new FileInfo(StorageDirectory + "\\" + ownerAttribute.Name + "\\" + filename);

            if (fi.Exists)
            {
                try
                {
                    XDocument xdoc = XDocument.Load(fi.FullName);
                    if (xdoc.Root.ExistAs("storage") && xdoc.Root.Element("data") != null)
                    {
                        string readedId = xdoc.Root.Attribute("ID").GetXValue(null);
                        string readedTicks = xdoc.Root.Attribute("Saved").GetXValue(null);
                        string readedOwner = xdoc.Root.Attribute("Owner").GetXValue(null);
                        string readedType = xdoc.Root.Attribute("Type").GetXValue(null);

                        if (readedOwner != ownerAttribute.Name)
                        {
                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + curr_id + ". Incorrect file structure: " + fi.FullName + ". Requested Owner = " + ownerAttribute.Name + "<> Readed Owner = " + readedOwner));
                            return false;
                        }

                        if (readedType != typeAttribute.Name)
                        {
                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + curr_id + ". Incorrect file structure: " + fi.FullName + ". Requested Type = " + typeAttribute.Name + "<> Readed Type = " + readedType));
                            return false;
                        }

                        if (readedTicks == null || !readedTicks.All(c => Char.IsDigit(c)))
                        {
                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + curr_id + ". Incorrect file structure: " + fi.FullName + ". Readed DateTime Stamp is not correct."));
                            return false;
                        }

                        if (curr_id != readedId)
                        {
                            Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + curr_id + ". Incorrect file structure: " + fi.FullName + ". Requested ID = " + curr_id + "<> Readed ID = " + readedId));
                            return false;
                        }

                        info = new StorageItemInfo(owner, type, curr_id, new DateTime(Convert.ToInt64(readedTicks)), true, xdoc.Root.Element("data"));
                        return true;
                    }
                    else
                    {
                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + curr_id + ". Incorrect file structure: " + fi.FullName + "."));
                        return false;
                    }
                }
                catch
                {
                    Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + curr_id + ". Can not to read file: " + fi.FullName + "."));
                    return false;
                }
            }
            else
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Warning, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + curr_id + ". File not found: " + fi.FullName + "."));
                return false;
            }
        }
        /// <summary>
        /// Получает все данные из хранилища касаемые данного владельца с заданным типом.
        /// </summary>
        /// <param name="owner">Владелец данных. Обычно приложение или сервис.</param>
        /// <param name="type">Тип данных.</param>
        /// <param name="infos">Полученная информация из хранилища.</param>
        public void Get(StoreOwner owner, StoreType type, out List<StorageItemInfo> infos)
        {
            string header = "GetData";

            // Получаем атрибуты элементов перечислений.
            OwnerAttribute ownerAttribute = GetStoreOwnerTextAttribute(owner);
            TypeAttribute typeAttribute = GetStoreTypeTextAttribute(type);
            infos = new List<StorageItemInfo>();

            // Проверяем входные параметры.
            if (ownerAttribute == null || typeAttribute == null)
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Incorrect input parameters."));
                return;
            }

            // Получаем специальное имя каталога и входящие в него файлы.
            DirectoryInfo di = new DirectoryInfo(StorageDirectory + "\\" + ownerAttribute.Name);

            if (di.Exists)
            {
                foreach (FileInfo fi in di.GetFiles("*.xml"))
                {
                    bool result = true;
                    string filename = fi.FullName;

                    try
                    {
                        XDocument xdoc = XDocument.Load(fi.FullName);
                        if (xdoc.Root.ExistAs("storage") && xdoc.Root.Element("data") != null)
                        {
                            string readedId = xdoc.Root.Attribute("ID").GetXValue(null);
                            string readedTicks = xdoc.Root.Attribute("Saved").GetXValue(null);
                            string readedOwner = xdoc.Root.Attribute("Owner").GetXValue(null);
                            string readedType = xdoc.Root.Attribute("Type").GetXValue(null);

                            if (readedOwner != ownerAttribute.Name)
                            {
                                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + ". Incorrect file structure: " + fi.FullName + ". Requested Owner = " + ownerAttribute.Name + "<> Readed Owner = " + readedOwner));
                                result = false;
                            }

                            if (readedType != typeAttribute.Name)
                            {
                                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + ". Incorrect file structure: " + fi.FullName + ". Requested Type = " + typeAttribute.Name + "<> Readed Type = " + readedType));
                                result = false;
                            }

                            if (readedTicks == null || !readedTicks.All(c => Char.IsDigit(c)))
                            {
                                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + ". Incorrect file structure: " + fi.FullName + ". Readed DateTime Stamp is not correct."));
                                result = false;
                            }

                            if (result)
                            {
                                StorageItemInfo info = new StorageItemInfo(owner, type, readedId, new DateTime(Convert.ToInt64(readedTicks)), true, xdoc.Root.Element("data"));
                                infos.Add(info);
                            }
                        }
                    }
                    catch
                    {
                        Event_Message(new MessageEventArgs(this, MessageEventArgsType.Error, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + ". Can not to read file: " + fi.FullName + "."));
                    }
                }
            }
            else
            {
                Event_Message(new MessageEventArgs(this, MessageEventArgsType.Warning, header, "Object:" + ownerAttribute.Name + "::" + typeAttribute.Name + "::" + ". Folder not found: " + di.FullName + "."));
            }
        }
        /* ======================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ======================================================================================== */
        /// <summary>
        /// Получает имя файла из данных параметров.
        /// </summary>
        /// <param name="owner">Тип данных.</param>
        /// <param name="id">Иденификатор данных.</param>
        /// <returns>Результат: Имя файла.</returns>
        private static string GetFileName(StoreType type, string id)
        {
            // Получение текстового атрибута.
            TypeAttribute typeAttribute = GetStoreTypeTextAttribute(type);

            if (typeAttribute == null)
            {
                return null;
            }

            if (id == null || id.Trim() == "")
            {
                return typeAttribute.Name + ".xml";
            }
            else
            {
                return typeAttribute.Name + "." + id + ".xml";
            }
        }
        /// <summary>
        /// Получает отрибут перечисления владельца данных StoreOwner.
        /// </summary>
        /// <param name="owner">Владелец данных.</param>
        /// <returns></returns>
        private static OwnerAttribute GetStoreOwnerTextAttribute(StoreOwner owner)
        {
            // Получение текстового атрибута.
            OwnerAttribute[] attributes = typeof(StoreOwner).GetField(owner.ToString())
                .GetCustomAttributes(typeof(OwnerAttribute), false)
                .Where(o => o is OwnerAttribute)
                .Select(t => (OwnerAttribute)t).ToArray();

            if (attributes.Length == 1)
            {
                return attributes[0];
            }

            return null;
        }
        /// <summary>
        /// Получает отрибут перечисления типа данных StoreType.
        /// </summary>
        /// <param name="type">Тип данных.</param>
        /// <returns></returns>
        private static TypeAttribute GetStoreTypeTextAttribute(StoreType type)
        {
            // Получение текстового атрибута.
            TypeAttribute[] attributes = typeof(StoreType).GetField(type.ToString())
                .GetCustomAttributes(typeof(TypeAttribute), false)
                .Where(o => o is TypeAttribute)
                .Select(t => (TypeAttribute)t).ToArray();

            if (attributes.Length == 1)
            {
                return attributes[0];
            }

            return null;
        }
        /* ======================================================================================== */
        #endregion
    }
}
