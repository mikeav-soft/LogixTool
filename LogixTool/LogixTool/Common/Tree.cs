using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogixTool.Common
{
    /// <summary>
    /// Представляет собой элемент структуры дерева.
    /// </summary>
    public class Tree
    {
        #region [ PROPERTIES ]
        /* ================================================================================================== */
        /// <summary>
        /// Идентификатор элемента.
        /// </summary>
        public string ID { get; protected set; }
        /// <summary>
        /// Родительский элемент дерева.
        /// </summary>
        public Tree Parrent { get; set; }
        /// <summary>
        /// Дочерние эелементы дерева.
        /// </summary>
        public Dictionary<string, Tree> Childrens { get; set; }
        /// <summary>
        /// Возвращает значение True если данный элемент содержит дочерние элементы.
        /// </summary>
        public bool HasChildrens
        {
            get
            {
                return (Childrens != null && Childrens.Count > 0);
            }
        }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// Create of new Tree Element.
        /// </summary>
        /// <param name="id"></param>
        public Tree(string id)
        {
            this.ID = id;
            this.Parrent = null;
            this.Childrens = new Dictionary<string, Tree>();
        }

        #region [ PUBLIC METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// Клонирует данный объект в новый, за исключением связей с сосдними элементами.
        /// </summary>
        /// <returns></returns>
        public virtual Tree Clone()
        {
            Tree tree = (Tree)this.MemberwiseClone();
            tree.Childrens = new Dictionary<string, Tree>();
            tree.Parrent = null;
            return tree;
        }
        /// <summary>
        /// Изменяет идентификатор элемента дерева.
        /// При успешно выполненной операции возвращает True.
        /// </summary>
        /// <param name="id">Новое значение идентификатора элемента дерева.</param>
        /// <returns></returns>
        public bool ChangeId(string id)
        {
            string curName = this.ID;
            string newName = id;

            if (this.Parrent == null)
            {
                // У элемента нет родителя.
                this.ID = newName;
                return true;
            }
            else
            {
                // У элемента имеется родитель.
                Tree parentTree = (Tree)this.Parrent;
                if (parentTree.Childrens.ContainsKey(curName))
                {
                    // Текущее имя имеется у родителя в словаре.
                    if (!parentTree.Childrens.ContainsKey(newName))
                    {
                        // Новоое имя не существует у родителя в словаре.
                        List<Tree> childrens = parentTree.Childrens.Values.ToList();
                        this.Childrens.Clear();
                        this.ID = newName;

                        foreach (Tree child in childrens)
                        {
                            parentTree.Add(child);
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (!parentTree.Childrens.ContainsKey(newName))
                    {
                        // Текущее имя не имеется у родителя в словаре.
                        this.ID = newName;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Производит слияние текущего элемента дерева и нисходящей структуры с новым элементом дерева.
        /// В случае равенства идентификаторов ID в параллельных структурах добавления и замены данного элемента происхордить не будет.
        /// </summary>
        /// <param name="tree">Элемент дерева которым будет достраиватся текущий элемент.</param>
        /// <returns></returns>
        public void Merge(Tree tree)
        {
            if (tree == null)
            {
                return;
            }

            // Текущий элемент дерева для потенциального добавления в базовое дерево.
            Tree currTree = tree;

            // Получает текущий идентификатор дерева для контроля существования.
            string currId = currTree.ID;

            // Проверяем есть ли такой дочерний элемент 
            // с идентификатором добавляемого элемента.
            if (!this.Childrens.ContainsKey(currId))
            {
                // Если такого элемента нет то добавляем текущий 
                // элемент дерева как дочерние в базовый элемент.
                this.Add(currTree);
                return;
            }
            else
            {
                // Если такой элемент существует то проверяем есть ли 
                // у добавляемого элемента дочерние эелементы чтобы их получить 
                // для дальнейшего добавления в дерево.
                if (!currTree.HasChildrens)
                {
                    // Если дочерних элементов нет то выходим из цикла.
                    return;
                }

                // Для каждого из элементов добавляемого дерева проводим рекурсивную операцию
                // опускаясь на один уровень далее по дереву.
                foreach (Tree t in currTree.Childrens.Values)
                {
                    this.Childrens[currId].Merge(t);
                }
                return;
            }
        }
        /// <summary>
        /// Добавляет новый доченрний элемент.
        /// </summary>
        /// <param name="t">Новый дочерний элемент.</param>
        /// <returns></returns>
        public bool Add(Tree t)
        {
            if (t == null || t.ID == null || this.Childrens == null)
            {
                return false;
            }
            else
            {
                string key = t.ID;

                if (!this.Childrens.ContainsKey(key))
                {
                    this.Childrens.Add(key, t);
                    t.Parrent = this;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Получает корневой элемент дерева.
        /// </summary>
        /// <returns></returns>
        public Tree GetRoot()
        {
            Tree tree = this;
            while (tree.Parrent != null)
            {
                tree = tree.Parrent;
            }
            return tree;
        }
        /// <summary>
        /// Получает самый верхний корневой элемент дерева с заданным типом.
        /// </summary>
        /// <returns></returns>
        public G GetRoot<G>() where G : Tree
        {
            Tree tree = this;
            while (tree.Parrent != null)
            {
                tree = tree.Parrent;
            }

            if (tree is G)
            {
                object obj = tree;
                return (G)obj;
            }
            else
            {
                return default(G);
            }
        }

        /// <summary>
        /// Получает родительский элемент с заданным типом.
        /// </summary>
        /// <typeparam name="G">Тип искомого элемента.</typeparam>
        /// <returns></returns>
        public G GetParrent<G>() where G : Tree
        {
            if (this.Parrent != null && this.Parrent is G)
            {
                object parrent = this.Parrent;
                return (G)parrent;
            }
            else
            {
                return default(G);
            }
        }
        /// <summary>
        /// Получает родительский элемент.
        /// </summary>
        /// <returns></returns>
        public Tree GetParrent()
        {
            return this.Parrent;
        }

        /// <summary>
        /// Получает все дочерние элементы с заданным типом.
        /// </summary>
        /// <typeparam name="G">Тип дочернего элемента.</typeparam>
        /// <returns></returns>
        public List<G> GetChilds<G>() where G : Tree
        {
            List<G> childs = new List<G>();
            foreach (Tree t in this.Childrens.Values)
            {
                if (t is G)
                {
                    object tree = t;
                    childs.Add((G)tree);
                }
            }
            return childs;
        }

        /// <summary>
        /// Получает дочерний элемент с заданным иминем и типом.
        /// </summary>
        /// <typeparam name="T">Тип дочернего элемента.</typeparam>
        /// <param name="id">Имя дочернего элемента.</param>
        /// <returns></returns>
        public G GetChild<G>(string id) where G : Tree
        {
            if (id != null && this.Childrens.ContainsKey(id))
            {
                object tree = this.Childrens[id];
                if (tree is G)
                {
                    return (G)tree;
                }
            }

            return default(G);
        }
        /// <summary>
        /// Получает дочерний элемент с заданным иминем.
        /// </summary>
        /// <param name="id">Имя дочернего элемента.</param>
        /// <returns></returns>
        public Tree GetChild(string id)
        {
            if (id != null && this.Childrens.ContainsKey(id))
            {
                return this.Childrens[id];
            }
            return null;
        }
        /// <summary>
        /// Получает дочерние элементы по заданному пути из имен.
        /// </summary>
        /// <param name="ids">Путь из имен</param>
        /// <returns></returns>
        public Tree GetChild(string[] ids)
        {
            Tree t = this;

            if (ids == null || ids.Length == 0)
            {
                return null;
            }

            foreach (string name in ids)
            {
                if (t == null || t.Childrens == null)
                {
                    return null;
                }

                if (!t.Childrens.TryGetValue(name, out t))
                {
                    return null;
                }
            }

            return t;
        }

        /// <summary>
        /// Удаляет дочерний элемент с заданным именем.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Remove(string id)
        {
            if (id == null || this.Childrens == null || !this.Childrens.ContainsKey(id))
            {
                return false;
            }

            return this.Childrens.Remove(id);
        }

        /// <summary>
        /// Очищает все дочерние элементы.
        /// </summary>
        public void Clear()
        {
            if (this.Childrens != null)
            {
                foreach (Tree t in this.Childrens.Values)
                {
                    t.Parrent = null;
                }

                this.Childrens.Clear();
            }
        }
        /// <summary>
        /// Получает ветвь (последовательность связных элементов) от текущего до головного элемента.
        /// </summary>
        /// <returns></returns>
        public List<Tree> GetRootBranch()
        {
            List<Tree> path = new List<Tree>();
            Tree currTreeElement = this;

            while (currTreeElement != null)
            {
                path.Add(currTreeElement);
                currTreeElement = currTreeElement.Parrent;
            }

            path.Reverse();
            return path;
        }
        /// <summary>
        /// Получает ветвь (последовательность связных элементов) от текущего до головного элемента.
        /// </summary>
        /// <returns></returns>
        public List<G> GetRootBranch<G>() where G : Tree
        {
            List<G> path = new List<G>();
            Tree currTreeElement = this;

            while (currTreeElement != null)
            {
                if (currTreeElement is G)
                {
                    object t = currTreeElement;
                    path.Add((G)t);
                }
                currTreeElement = currTreeElement.Parrent;
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public List<Tree> FindByLines(Func<Tree, bool> select)
        {
            return FindByLines((t, n) => select.Invoke(t));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public List<Tree> FindByLines(Func<Tree, int, bool> select)
        {
            return FindByLines(select, (c, f, n) => c);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public List<Tree> FindByLines(Func<Tree, int, bool> select, Func<IEnumerable<Tree>, Tree, int, IEnumerable<Tree>> next)
        {
            List<Tree> result = new List<Tree>();
            FindByLinesNext(select, next, result, 0);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public List<Tree> FindByArcs(Func<Tree, bool> select)
        {
            return FindByArcs((t, n) => select.Invoke(t));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public List<Tree> FindByArcs(Func<Tree, int, bool> select)
        {
            return FindByArcs(select, (c, f, n) => c);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public List<Tree> FindByArcs(Func<Tree, int, bool> select, Func<IEnumerable<Tree>, List<Tree>, int, IEnumerable<Tree>> next)
        {
            List<Tree> result = new List<Tree>();
            FindByArcsNext(select, next, result, 0);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public List<Tree> FindParrents(Func<Tree, bool> select)
        {
            return FindParrents((t, n) => select.Invoke(t));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public List<Tree> FindParrents(Func<Tree, int, bool> select)
        {
            return FindParrents(select, (c, f, n) => true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public List<Tree> FindParrents(Func<Tree, int, bool> select, Func<Tree, Tree, int, bool> next)
        {
            List<Tree> result = new List<Tree>();
            FindParrentsNext(select, next, result, 0);
            return result;
        }
        /* ================================================================================================== */
        #endregion

        #region [ PRIVATE METHODS ]
        /* ================================================================================================== */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="select">Критерий поиска элемента. 
        /// [Текущий элемент для проверки; Текущий номер итерации]</param>
        /// <param name="next">Критерий перехода поиска к следующей итерации.
        /// [Последующие элементы для поиска; Текущий найденыйэлент дерева; Текущий номер итерации]</param>
        /// <param name="result">Список с результатом поиска по дереву.</param>
        /// <param name="iteration">Текущий номер итерации.</param>
        private void FindByLinesNext(Func<Tree, int, bool> select, Func<IEnumerable<Tree>, Tree, int, IEnumerable<Tree>> next, List<Tree> result, int iteration)
        {
            // Проверка входных параметров.
            if (select == null || result == null)
            {
                return;
            }

            Tree foundTree = null;

            // Добавляем текущий элемент в список в случае если условия поиска удовлетворены.
            if (select.Invoke(this, iteration))
            {
                foundTree = this;
                result.Add(foundTree);
            }

            if (this.Childrens != null && this.Childrens.Count > 0)
            {
                IEnumerable<Tree> nextChilds = next.Invoke(this.Childrens.Values, foundTree, iteration);

                if (nextChilds != null)
                {
                    foreach (Tree t in nextChilds)
                    {
                        t.FindByLinesNext(select, next, result, iteration + 1);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="select"></param>
        /// <param name="next"></param>
        /// <param name="result"></param>
        /// <param name="iteration"></param>
        private void FindByArcsNext(Func<Tree, int, bool> select, Func<IEnumerable<Tree>, List<Tree>, int, IEnumerable<Tree>> next, List<Tree> result, int iteration)
        {
            // Проверка входных параметров.
            if (select == null || result == null)
            {
                return;
            }

            List<Tree> foundTrees = new List<Tree>();

            // Добавляем текущий элемент в список в случае если условия поиска удовлетворены.
            if (select.Invoke(this, iteration) && iteration == 0)
            {
                Tree tree = this;
                foundTrees.Add(tree);
                result.Add(tree);
            }

            if (this.Childrens != null && this.Childrens.Count > 0)
            {
                foreach (Tree child in this.Childrens.Values)
                {
                    Tree tree = child;

                    if (select.Invoke(tree, iteration))
                    {
                        result.Add(tree);
                        foundTrees.Add(tree);
                    }
                }

                IEnumerable<Tree> nextChilds = next.Invoke(this.Childrens.Values, foundTrees, iteration);

                if (nextChilds != null)
                {
                    foreach (Tree t in nextChilds)
                    {
                        t.FindByArcsNext(select, next, result, iteration + 1);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="select">Критерий выбора текущего элемента, где первый аргумент Текущий элемент дерева, второй аргумент номер итерации поиска.</param>
        /// <param name="next">Критерий выбора следующих элементов для дальнейшего поиска.</param>
        /// <param name="result"></param>
        /// <param name="iteration"></param>
        private void FindParrentsNext(Func<Tree, int, bool> select, Func<Tree, Tree, int, bool> next, List<Tree> result, int iteration)
        {
            // Проверка входных параметров.
            if (select == null || result == null)
            {
                return;
            }

            Tree foundTree = null;

            // Добавляем текущий элемент в список в случае если условия поиска удовлетворены.
            if (select.Invoke(this, iteration))
            {
                foundTree = this;
                result.Add(foundTree);
            }

            if (this.Parrent != null && next.Invoke(this.Parrent, foundTree, iteration))
            {
                this.Parrent.FindParrentsNext(select, next, result, iteration + 1);
            }
        }
        /* ================================================================================================== */
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="ic"></param>
        /// <param name="head"></param>
        public static void TestNumericTreeBuilding(int ix, int ic, Tree head)
        {
            if (ix <= 0)
            {
                return;
            }

            List<Tree> newChilds = new List<Tree>();

            for (int cix = 0; cix < ic; cix++)
            {
                newChilds.Add(new Tree(head.ID + cix.ToString()));
            }

            foreach (Tree t in newChilds)
            {
                head.Add(t);
                TestNumericTreeBuilding(ix - 1, ic, t);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
