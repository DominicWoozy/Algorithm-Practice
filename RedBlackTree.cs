using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTree
{
    /// <summary>
    /// 红黑树(左倾)，一种自平衡二叉树，也是2-3-4树的一种，树中存在2、3节点，并且在插入和删除时会产生临时的4节点。
    /// 规则：节点颜色非红即黑，根节点为黑，两个红节点不能相连，从根节点到叶子节点的每条路径上的黑节点数量相同。
    /// 无红节点相连的黑节点为2节点，空节点为黑节点，左侧连一个红节点的黑节点为3节点，左右都是红节点的黑节点为4节点。
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    class RedBlackTree<T> 
    {
        private int _count;
        private IComparer<T> _comparer;
        private Node _root;
        private bool isSafe = true;

        /// <summary>
        /// 创建红黑树。
        /// </summary>
        public RedBlackTree()
        {
            _root = null;
            _comparer = Comparer<T>.Default;
        }

        /// <summary>
        /// 以指定比较方式创建红黑树。
        /// </summary>
        /// <param name="comparer"></param>
        public RedBlackTree(IComparer<T> comparer)
        {
            _root = null;
            this._comparer = comparer;
        }

        //---------------------------------------------------------
        //操作方法
        //---------------------------------------------------------

        public int Count => _count;

        /// <summary>
        /// 判断是否已经存在某值。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return InternalContains(_root, item);
        }

        private bool InternalContains(Node node, T item)
        {
            if (node == null) return false;
            int compare = _comparer.Compare(item, node.item);
            if (compare == 0) return true;
            else if (compare < 0) return InternalContains(node.left, item);
            else return InternalContains(node.right, item);
        }

        /// <summary>
        /// 添加元素。
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("参数不得为空值。");
            if (!isSafe)
                throw new InvalidOperationException("不得同时操作多个方法。");
            isSafe = false;
            _root = InternalPut(_root, item);
            _root.isRed = false;
            _count++;
            isSafe = true;
        }

        /// <summary>
        /// 批量添加元素。
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<T> items)
        {
            foreach(T item in items)
                Add(item);
        }

        /// <summary>
        /// 内部插入方法，使用递归便于恢复平衡性。若存在便更新原节点。
        /// </summary>
        /// <param name="node"></param>
        /// <param name="item"></param>
        /// <returns>平衡后的节点</returns>
        private Node InternalPut(Node node, T item)
        {
            if (node == null)
                return new Node(item);
            int compare = _comparer.Compare(item, node.item);
            if (compare > 0) node.right = InternalPut(node.right, item);
            else if (compare < 0) node.left = InternalPut(node.left, item);
            else node.item = item;
            if (IsRed(node.right) && !IsRed(node.left)) 
                node = RotateLeft(node);
            if (IsRed(node.left) && IsRed(node.left.left)) 
                node = RotateRight(node);
            if (IsRed(node.left) && IsRed(node.right)) 
                FlipColors(node);
            return node;
        }

        /// <summary>
        /// 删除整棵树的最小值。
        /// </summary>
        public void RemoveMin()
        {
            if (_root == null) 
                throw new NullReferenceException("树为空");
            if (!isSafe)
                throw new InvalidOperationException("不得同时操作多个方法。");
            isSafe = false;
            if (!IsRed(_root.left) && !IsRed(_root.right))
                _root.isRed = true;
            _root = InternalRemoveMin(_root);
            if (_root != null) _root.isRed = false;
            _count--;
            isSafe = true;
        }

        /// <summary>
        /// 内部方法，删除某节点下的最小值。可用作提供前驱节点。
        /// </summary>
        /// <param name="node"></param>
        /// <returns>节点</returns>
        private static Node InternalRemoveMin(Node node)
        {
            if (node.left == null)
                return null;
            if (!IsRed(node.left) && !IsRed(node.left.left))
                node = MoveRedLeft(node);
            node.left = InternalRemoveMin(node.left);
            return Balance(node);
        }

        public void RemoveMax()
        {
            if (_root == null)
                throw new NullReferenceException("树为空");
            if (!isSafe)
                throw new InvalidOperationException("不得同时操作多个方法。");
            isSafe = false;
            if (!IsRed(_root.left) && !IsRed(_root.right))
                _root.isRed = true;
            _root = InternalRemoveMax(_root);
            if (_root != null) _root.isRed = false;
            _count--;
            isSafe = true;
        }

        /// <summary>
        /// 内部方法，删除某节点下的最大值。可用作提供后继节点。
        /// </summary>
        /// <param name="node"></param>
        /// <returns>节点</returns>
        private static Node InternalRemoveMax(Node node)
        {
            if (IsRed(node.left))
                node = RotateRight(node);
            if (node.right == null)
                return null;
            if (!IsRed(node.right) && !IsRed(node.right.left))
                node = MoveRedRight(node);
            node.right = InternalRemoveMax(node.right);
            return Balance(node);
        }

        /// <summary>
        /// 删除元素。
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            if (item == null)
                throw new ArgumentNullException("参数不得为空值");
            if (!isSafe)
                throw new InvalidOperationException("不得同时操作多个方法。");
            isSafe = false;

            if (!Contains(item)) return;

            if (!IsRed(_root.left) && !IsRed(_root.right))
                _root.isRed = true;

            _root = InternalRemove(_root, item);
            if (_root != null) _root.isRed = false;
            isSafe = true;
        }

        /// <summary>
        /// 内部删除方法，为多种情况恢复平衡。
        /// </summary>
        /// <param name="node"></param>
        /// <param name="item"></param>
        /// <returns>节点</returns>
        private Node InternalRemove(Node node, T item)
        {
            if (_comparer.Compare(item, node.item) < 0) 
            {
                //若小于删除元素且左节点和左子节点都是黑节点，则更改颜色打破平衡。
                if (!IsRed(node.left) && !IsRed(node.left.left))
                    node = MoveRedLeft(node);
                node.left = InternalRemove(node.left, item);
            }
            else
            {
                if (IsRed(node.left))
                    node = RotateRight(node);
                if ((_comparer.Compare(item, node.item) == 0) && (node.right == null))
                    return null;
                if (!IsRed(node.right) && !IsRed(node.right.left))
                    node = MoveRedRight(node);
                if (_comparer.Compare(item, node.item) == 0)
                {
                    //先用此节点的前驱节点取代此节点，再将前驱节点删除以保持有序。
                    Node x = InternalMin(node.right);
                    node.item = x.item;
                    node.right = InternalRemoveMin(node.right);
                }
                else
                    node.right = InternalRemove(node.right, item);
            }
            return Balance(node);
        }

        private static Node InternalMin(Node node)
        {
            if (node.left == null) return node;
            else return InternalMin(node.left);
        }


        //---------------------------------------------------------
        //遍历操作
        //---------------------------------------------------------

        /// <summary>
        /// 先序遍历，使用委托方法增加遍历的可用性。
        /// </summary>
        /// <param name="action">任务</param>
        public void PreOrderTravesal(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("方法参数不得为空。");
            if (!isSafe)
                throw new InvalidOperationException("不得同时操作多个方法。");
            isSafe = false;
            InternalPreOrderTravesal(_root, action);
            isSafe = true;
        }

        private static void InternalPreOrderTravesal(Node node, Action<T> action)
        {
            if (node == null) return;
            action.Invoke(node.item);
            InternalPreOrderTravesal(node.left, action);
            InternalPreOrderTravesal(node.right, action);
        }

        /// <summary>
        /// 中序遍历，也是按排序输出，使用委托方法增加遍历的可用性。
        /// </summary>
        /// <param name="action">任务</param>
        public void InOrderTravesal(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("方法参数不得为空。");
            if (!isSafe)
                throw new InvalidOperationException("不得同时操作多个方法。");
            isSafe = false;
            InternalInOrderTravesal(_root, action);
            isSafe = true;
        }

        private static void InternalInOrderTravesal(Node node, Action<T> action)
        {
            if (node == null) return;
            InternalInOrderTravesal(node.left, action);
            action.Invoke(node.item);
            InternalInOrderTravesal(node.right, action);
        }

        /// <summary>
        /// 后序遍历，使用委托方法增加遍历的可用性。
        /// </summary>
        /// <param name="action">任务</param>
        public void PostOrderTravesal(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("方法参数不得为空。");
            if (!isSafe)
                throw new InvalidOperationException("不得同时操作多个方法。");
            isSafe = false;
            InternalPostOrderTravesal(_root, action);
            isSafe = true;
        }

        private static void InternalPostOrderTravesal(Node node, Action<T> action)
        {
            if (node == null) return;
            InternalPostOrderTravesal(node.left, action);
            InternalPostOrderTravesal(node.right, action);
            action.Invoke(node.item);
        }

        /// <summary>
        /// 层次遍历，使用委托方法增加遍历的可用性。
        /// </summary>
        /// <param name="action">任务</param>
        public void LevelOrderTravesal(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("方法参数不得为空。");
            if (!isSafe)
                throw new InvalidOperationException("不得同时操作多个方法。");
            isSafe = false;
            Queue<Node> quene = new Queue<Node>();
            quene.Enqueue(_root);
            while(quene.Count != 0)
            {
                Node node = quene.Dequeue();
                if (node == null) continue;
                action.Invoke(node.item);
                quene.Enqueue(node.left);
                quene.Enqueue(node.right);
            }
            isSafe = true;
        }
        
        /// <summary>
        /// 打印树的完整结构。
        /// </summary>
        public void Print()
        {
            if (_root == null) return;
            List<List<string>> table = new List<List<string>>();
            Queue<Node> queue1 = new Queue<Node>();
            Queue<Node> queue2 = new Queue<Node>();
            queue1.Enqueue(_root);
            bool flag = true;
            while (flag)
            {
                List<string> line = new List<string>();
                Queue<Node> temp = queue1;
                queue1 = queue2;
                queue2 = temp;
                flag = false;
                while (queue2.Count != 0)
                {
                    Node node = queue2.Dequeue();
                    if (node == null)
                    {
                        line.Add("    ");
                        queue1.Enqueue(null);
                        queue1.Enqueue(null);
                    }
                    else
                    {
                        if(node.isRed)
                            line.Add($"/\\R/{node.item, -4}");
                        else
                            line.Add($"{node.item,-4}");
                        queue1.Enqueue(node.left);
                        queue1.Enqueue(node.right);
                        flag = true;
                    }
                }
                if(flag) table.Add(line);
            }
            int width =  8 << table.Count;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("The structure of the RedBlackBST:");
            Console.ResetColor();
            foreach (var line in table)
            {
                Console.WriteLine();
                width >>= 1;
                string blank = "";
                for (int j = 0; j < width - 4; j++)
                    blank += " ";
                for (int j = 0; j < width / 2 - 4; j++)
                    Console.Write(" ");
                foreach (var cell in line)
                {
                    if (cell.StartsWith("/\\R/"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(cell.Substring(4) + blank);
                        Console.ResetColor();
                    }
                    else
                        Console.Write(cell + blank);
                }
            }
            Console.WriteLine();
        }


        //---------------------------------------------------------
        //辅助函数
        //---------------------------------------------------------

        /// <summary>
        /// 若当前节点左边有两个连续的红节点，则向右转。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static Node RotateRight(Node node)
        {
            Node x = node.left;
            node.left = x.right;
            x.right = node;
            x.isRed = x.right.isRed;
            x.right.isRed = true;
            return x;
        }

        /// <summary>
        /// 左旋，破坏平衡以形成临时的4节点。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static Node RotateLeft(Node node)
        {
            Node x = node.right;
            node.right = x.left;
            x.left = node;
            x.isRed = x.left.isRed;
            x.left.isRed = true;
            return x;
        }

        /// <summary>
        /// 将4节点的颜色翻转。
        /// </summary>
        /// <param name="node"></param>
        private static void FlipColors(Node node)
        {
            node.isRed = !node.isRed;
            node.left.isRed = !node.left.isRed;
            node.right.isRed = !node.right.isRed;
        }

        /// <summary>
        /// 改变颜色，打破2节点，形成临时的4节点。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static Node MoveRedLeft(Node node)
        {
            FlipColors(node);
            if (IsRed(node.right.left))
            {
                node.right = RotateRight(node.right);
                node = RotateLeft(node);
                FlipColors(node);
            }
            return node;
        }

        private static Node MoveRedRight(Node node)
        {
            FlipColors(node);
            if (IsRed(node.left.left))
            {
                node = RotateRight(node);
                FlipColors(node);
            }
            return node;
        }

        /// <summary>
        /// 恢复平衡，消除出现的4节点。
        /// </summary>
        /// <param name="node"></param>
        /// <returns>平衡后的节点</returns>
        private static Node Balance(Node node)
        {
            if (IsRed(node.right)) 
                node = RotateLeft(node);
            if (IsRed(node.left) && IsRed(node.left.left)) 
                node = RotateRight(node);
            if (IsRed(node.left) && IsRed(node.right)) 
                FlipColors(node);
            return node;
        }

        /// <summary>
        /// 判断节点是否为红节点。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool IsRed(Node node)
        {
            return (node != null && node.isRed);
        }


        /// <summary>
        /// 红黑树节点。
        /// </summary>
        class Node
        {
            public T item;
            public bool isRed;
            public Node left;
            public Node right;

            public Node(T item)
            {
                this.item = item;
                isRed = true;
            }

            public Node(T item, bool isRed)
            {
                this.item = item;
                this.isRed = isRed;
            }
        }

    }
}
