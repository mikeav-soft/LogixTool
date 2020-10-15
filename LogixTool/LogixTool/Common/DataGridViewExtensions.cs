using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace LogixTool.Common
{
    /// <summary>
    /// Расширения для DataGridView классам который он использует.
    /// </summary>
    public static class DataGridViewExtensions
    {
        /// <summary>
        /// Установливает значение по порядковому номеру Item элемента DataGridViewComboBoxCell.
        /// </summary>
        /// <param name="cell">Текущая ячейка типа DataGridViewComboBoxCell.</param>
        /// <param name="itemIndex">Номер индекса Item.</param>
        public static void SetComboBoxCellIndex(this DataGridViewCell cell, int itemIndex)
        {
            if (cell != null && cell is DataGridViewComboBoxCell && itemIndex < (cell as DataGridViewComboBoxCell).Items.Count)
            {
                (cell as DataGridViewComboBoxCell).Value = (cell as DataGridViewComboBoxCell).Items[itemIndex];
            }
        }
        /// <summary>
        /// Получает значение порядкового номера Item элемента DataGridViewComboBoxCell.
        /// </summary>
        /// <param name="cell">Текущая ячейка типа DataGridViewComboBoxCell.</param>
        /// <returns>Возвращает индекс элемента.</returns>
        public static int? GetComboBoxCellIndex(this DataGridViewCell cell)
        {
            int? result = null;

            if (cell != null && cell.Value != null && cell is DataGridViewComboBoxCell)
            {
                DataGridViewComboBoxCell comboBoxCell = (cell as DataGridViewComboBoxCell);

                for (int ix = 0; ix < comboBoxCell.Items.Count; ix++)
                {
                    if (comboBoxCell.Value.ToString() == (cell as DataGridViewComboBoxCell).Items[ix].ToString())
                    {
                        return ix;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Получает текстовое значение Item элемента DataGridViewComboBoxCell.
        /// </summary>
        /// <param name="cell">Текущая ячейка типа DataGridViewComboBoxCell.</param>
        /// <returns>Возвращает текстовное значение ячейки.</returns>
        public static string GetComboBoxCellValue(this DataGridViewCell cell)
        {
            string result = null;

            if (cell != null && cell.Value != null && cell is DataGridViewComboBoxCell)
            {
                result = cell.Value.ToString();

            }
            return result;
        }
        /// <summary>
        /// Устанавливает значение Item элемента DataGridViewComboBoxCell из текста.
        /// </summary>
        /// <param name="cell">Текущая ячейка типа DataGridViewComboBoxCell.</param>
        /// <param name="value">Текстовое значение Item.</param>
        /// <returns>При успешной операции возвращает True.</returns>
        public static bool SetComboBoxCellValue(this DataGridViewCell cell, string value)
        {
            // Проверяем входные параметры.
            if (value == null || cell == null || !(cell is DataGridViewComboBoxCell))
            {
                return false;
            }
            // Перебирам все значение Items до тех пор пока не найдем равное заданному тексту значение.
            // Если такое значение имеется то устанавливаем Item элемент по найденному индексу.
            DataGridViewComboBoxCell comboBoxCell = (cell as DataGridViewComboBoxCell);
            for (int ix = 0; ix < comboBoxCell.Items.Count; ix++)
            {
                if (value == (cell as DataGridViewComboBoxCell).Items[ix].ToString())
                {
                    comboBoxCell.Value = comboBoxCell.Items[ix];
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Совершает перемещение выделенных строк вверхи или вниз по таблице.
        /// </summary>
        /// <param name="dataGridView">Текщая таблица DataGridView.</param>
        /// <param name="moveDown">При значении True производит перемещение вниз по таблице. В противном случае вверх.</param>
        public static void MoveSelectedRows(this DataGridView dataGridView, bool moveDown)
        {
            // Проверяем входные параметры.
            if (dataGridView == null)
            {
                return;
            }

            // Преобразовываем коллекцию строк таблицы в список. 
            List<DataGridViewRow> rowCollection = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.IsNewRow)
                {
                    rowCollection.Add(row);
                }
            }

            // Операция перемещения была проделана.
            bool applyOperation = false;

            if (moveDown)
            {
                // Перемещает выделенные строки Вниз.
                if (rowCollection.Count > 0 && !rowCollection.Last().Selected)
                {
                    for (int ix = rowCollection.Count - 1; ix >= 0; ix--)
                    {
                        if (rowCollection[ix].Selected)
                        {
                            DataGridViewRow tempRow;
                            tempRow = rowCollection[ix + 1];
                            rowCollection[ix + 1] = rowCollection[ix];
                            rowCollection[ix] = tempRow;
                            applyOperation = true;
                        }
                    }
                }
            }
            else
            {
                // Перемещает выделенные строки Вверх.
                if (rowCollection.Count > 0 && !rowCollection.First().Selected)
                {
                    for (int ix = 0; ix < rowCollection.Count; ix++)
                    {
                        if (rowCollection[ix].Selected)
                        {
                            DataGridViewRow tempRow;
                            tempRow = rowCollection[ix - 1];
                            rowCollection[ix - 1] = rowCollection[ix];
                            rowCollection[ix] = tempRow;
                            applyOperation = true;
                        }
                    }
                }
            }

            // Если операция перемещения выделенных строк была упешной, то имеет смысл ее применять.
            // Очищаем старый порядок строк запоминаем какие из строк были выделены и добавляем новый порядок.
            // Снова ставим выделение на ранее выделенных строках (выделение сбрасывается при новом добавлении).
            if (applyOperation)
            {
                List<bool> selectedRowIndexes = new List<bool>();
                for (int ix = 0; ix < rowCollection.Count; ix++)
                {
                    selectedRowIndexes.Add(rowCollection[ix].Selected);
                }

                dataGridView.Rows.Clear();
                dataGridView.Rows.AddRange(rowCollection.ToArray());

                for (int ix = 0; ix < rowCollection.Count; ix++)
                {
                    rowCollection[ix].Selected = selectedRowIndexes[ix];
                }
            }
        }
        /// <summary>
        /// Вствляет или заменяет строки в таблице DataGridView c заданной выделенной строки текст
        /// из заданного текста который разделен символом "\t" обозначающим разделение мезжу ячейками и "\r\n" между строками.
        /// </summary>
        /// <param name="dataGridView">Текщая таблица DataGridView.</param>
        /// <param name="text">Вставляемый текст.</param>
        public static void AddRowsFromText(this DataGridView dataGridView, string text)
        {
            // Проверяем входные параметры.
            if (dataGridView == null || text == null)
            {
                return;
            }
            // Получаем кол-во колонок в таблице.
            int columnCount = dataGridView.ColumnCount;
            int currentRowIndex = 0;

            // В случае отсутствия колонок операция неудачна.
            if (columnCount <= 0)
            {
                return;
            }

            // Устанавливаем из выбранных строк индекс первой с начала которой будем производить вставку.
            if (dataGridView.SelectedRows.Count > 0)
            {
                currentRowIndex = dataGridView.SelectedRows[0].Index;
                foreach (DataGridViewRow row in dataGridView.SelectedRows)
                {
                    if (row.Index < currentRowIndex)
                    {
                        currentRowIndex = row.Index;
                    }
                }
            }

            string [] rowParts = text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // Преобразовываем входной текст в массивы построчно.
            foreach (string rowPart in rowParts)
            {
                string[] textRowParts = rowPart.Split("\t".ToCharArray());
                if (textRowParts.Length <= columnCount)
                {
                    if (dataGridView.Rows[currentRowIndex].IsNewRow || dataGridView.RowCount <= currentRowIndex)
                    {
                        dataGridView.Rows.Add();
                    }

                    // Заполняем значения строки в таблице dataGridView.
                    for (int colIndex = 0; colIndex < textRowParts.Length; colIndex++)
                    {
                        DataGridViewCell cell = dataGridView.Rows[currentRowIndex].Cells[colIndex];
                        if (cell.GetType() == typeof(DataGridViewTextBoxCell))
                        {
                            cell.Value = textRowParts[colIndex];
                        }
                        else if (cell.GetType() == typeof(DataGridViewComboBoxCell))
                        {
                            if (!cell.SetComboBoxCellValue(textRowParts[colIndex]))
                            {
                                cell.SetComboBoxCellIndex(0);
                            }
                        }
                    }
                    currentRowIndex++;
                }
            }
        }
        /// <summary>
        /// Преобразовывает в текст выделенные строки из таблицы DataGridView в виде текста который 
        /// разделен символом "\t" обозначающим разделение мезжу ячейками и "\r\n" между строками.
        /// </summary>
        /// <param name="dataGridView">Текщая таблица DataGridView.</param>
        /// <returns>Возвращаемый текст.</returns>
        public static string GetTextFromSelectedRows(this DataGridView dataGridView)
        {
            // Проверяем входные параметры.
            if (dataGridView == null)
            {
                return null;
            }

            // Результат операции в виде строки.
            string result = "";

            // Цикла перебора строк таблиц.
            // Синтезируем строки с разделителем ""\r\n"".
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                // За исключением новой строки синтезируем ячейки с разделителем "\t".
                if (!row.IsNewRow)
                {
                    if (result != "")
                    {
                        result += "\r\n";
                    }

                    string rowText = "";
                    // Цикла перебора ячеек таблиц.
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (rowText != "")
                        {
                            rowText += "\t";
                        }

                        if (cell.Value != null)
                        {
                            rowText += cell.Value.ToString();
                        }
                    }
                    result += rowText;
                }
            }

            return result;
        }
        /// <summary>
        /// Получает текстовое значение из выбранных ячеек.
        /// </summary>
        /// <param name="dataGridView">Текщая таблица DataGridView.</param>
        /// <returns></returns>
        public static string GetTextFromSelectedCells(this DataGridView dataGridView)
        {
            // Проверяем входные параметры.
            if (dataGridView == null)
            {
                return null;
            }

            // Результат операции в виде строки.
            string result = "";

            int columnCount = dataGridView.ColumnCount;

            List<DataGridViewCell> cells = dataGridView.SelectedCells.ToList()
                .OrderBy(y => y.RowIndex * columnCount + y.ColumnIndex).ToList();

            DataGridViewCell[,] table = new DataGridViewCell[(cells.Last().RowIndex - cells.First().RowIndex) + 1, columnCount];

            foreach (DataGridViewCell c in cells)
            {
                table[c.RowIndex - cells.First().RowIndex, c.ColumnIndex] = c;
            }

            return result;
        }
        /// <summary>
        /// Преобразовывает данную коллекцию ячеек в список.
        /// В случае равенства коллекции Null, будет также возвращен результат Null.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static List<DataGridViewCell> ToList(this DataGridViewCellCollection collection)
        {
            // Проверяем входные параметры.
            if (collection == null)
            {
                return null;
            }
            // Проеобразовываем коллекцию.
            List<DataGridViewCell> result = new List<DataGridViewCell>();
            foreach(DataGridViewCell cell in collection)
            {
                result.Add(cell);
            }

            return result;
        }
        /// <summary>
        /// Преобразовывает данную коллекцию выделенных ячеек в список.
        /// В случае равенства коллекции Null, будет также возвращен результат Null.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static List<DataGridViewCell> ToList(this DataGridViewSelectedCellCollection collection)
        {
            // Проверяем входные параметры.
            if (collection == null)
            {
                return null;
            }
            // Проеобразовываем коллекцию.
            List<DataGridViewCell> result = new List<DataGridViewCell>();
            foreach (DataGridViewCell cell in collection)
            {
                result.Add(cell);
            }

            return result;
        }
        /// <summary>
        /// Преобразовывает данную коллекцию строк в список.
        /// В случае равенства коллекции Null, будет также возвращен результат Null.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static List<DataGridViewRow> ToList(this DataGridViewRowCollection collection)
        {
            // Проверяем входные параметры.
            if (collection == null)
            {
                return null;
            }
            // Проеобразовываем коллекцию.
            List<DataGridViewRow> result = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in collection)
            {
                result.Add(row);
            }

            return result;
        }
        /// <summary>
        /// Преобразовывает данную коллекцию выделенных строк в список.
        /// В случае равенства коллекции Null, будет также возвращен результат Null.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static List<DataGridViewRow> ToList(this DataGridViewSelectedRowCollection collection)
        {
            // Проверяем входные параметры.
            if (collection == null)
            {
                return null;
            }
            // Проеобразовываем коллекцию.
            List<DataGridViewRow> result = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in collection)
            {
                result.Add(row);
            }

            return result;
        }
        /// <summary>
        /// Получает коллекцию прямоугольников соответствующая области заголовков колонок, где индекс
        /// коллекции соответствует индексу колонки.
        /// </summary>
        /// <param name="dataGridView">Текщая таблица DataGridView.</param>
        /// <returns></returns>
        public static List<Rectangle> GetDisplayedColumnHeaderBounds (this DataGridView dataGridView)
        {
            // Список с прямоугольными областями.
            List<Rectangle> rectangles = new List<Rectangle>();
            // Текущая координата X.
            int currX = dataGridView.RowHeadersWidth;

            // Перебираем по очереди все колонки таблицы.
            for (int ix = 0; ix < dataGridView.ColumnCount; ix++)
            {
                DataGridViewColumn column = dataGridView.Columns[ix];

                int width = column.Visible ? column.Width : 0;
                Rectangle rext = new Rectangle(currX, 0, width, dataGridView.ColumnHeadersHeight);
                rectangles.Add(rext);

                currX += width;
            }

            return rectangles;
        }
    }
}
