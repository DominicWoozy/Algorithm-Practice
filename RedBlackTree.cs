using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTree
{
    /// <summary>
    /// 红黑树(左倾)，一种自平衡树，也是2-3-4树的一种，树中存在2、3节点，并且在插入和删除时会产生临时的4节点。
    /// 规则：节点颜色非红即黑，根节点为黑，两个红节点不能相连，从根节点到叶子节点的每条路径上的黑节点数量相同。
    /// 无红节点相连的黑节点为2节点，空节点为黑节点，左侧连一个红节点的黑节点为3节点，左右都是红节点的黑节点为4节点。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class RedBlackTree<T>
    {
        private int _count;
        private Comparer<T> _comparer;
        private Node _root;

        public RedBlackTree()
        {
            _root = null;
            _comparer = Comparer<T>.Default;
        }

        public RedBlackTree(Comparer<T> comparer)
        {
            _root = null;
            this._comparer = comparer;
        }

        //---------------------------------------------------------
        //操作方法
        //---------------------------------------------------------

        public int Count => _count;

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

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("参数不得为空值");
            _root = InternalPut(_root, item);
            _root.isRed = false;
            _count++;
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach(T item in items)
                Add(item);
        }

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

        public void RemoveMin()
        {
            if (_root == null) 
                throw new NullReferenceException("树为空");
            if (!IsRed(_root.left) && !IsRed(_root.right))
                _root.isRed = true;
            _root = InternalRemoveMin(_root);
            if (_root != null) _root.isRed = false;
            _count--;
        }

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
            if (!IsRed(_root.left) && !IsRed(_root.right))
                _root.isRed = true;
            _root = InternalRemoveMax(_root);
            if (_root != null) _root.isRed = false;
            _count--;
        }

        private static Node InternalRemoveMax(Node h)
        {
            if (IsRed(h.left))
                h = RotateRight(h);
            if (h.right == null)
                return null;
            if (!IsRed(h.right) && !IsRed(h.right.left))
                h = MoveRedRight(h);
            h.right = InternalRemoveMax(h.right);
            return Balance(h);
        }

        public void Remove(T item)
        {
            if (item == null)
                throw new ArgumentNullException("参数不得为空值");
            if (!Contains(item)) return;

            if (!IsRed(_root.left) && !IsRed(_root.right))
                _root.isRed = true;

            _root = InternalRemove(_root, item);
            if (_root != null) _root.isRed = false;
        }

        private Node InternalRemove(Node node, T item)
        {
            if (_comparer.Compare(item, node.item) < 0)
            {
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

        public void PreOrderTravesal(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("方法参数不得为空。");
            InternalPreOrderTravesal(_root, action);
        }

        private static void InternalPreOrderTravesal(Node node, Action<T> action)
        {
            if (node == null) return;
            action.Invoke(node.item);
            InternalPreOrderTravesal(node.left, action);
            InternalPreOrderTravesal(node.right, action);
        }

        public void InOrderTravesal(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("方法参数不得为空。");
            InternalInOrderTravesal(_root, action);
        }

        private static void InternalInOrderTravesal(Node node, Action<T> action)
        {
            if (node == null) return;
            InternalInOrderTravesal(node.left, action);
            action.Invoke(node.item);
            InternalInOrderTravesal(node.right, action);
        }

        public void PostOrderTravesal(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("方法参数不得为空。");
            InternalPostOrderTravesal(_root, action);
        }

        private static void InternalPostOrderTravesal(Node node, Action<T> action)
        {
            if (node == null) return;
            InternalPostOrderTravesal(node.left, action);
            InternalPostOrderTravesal(node.right, action);
            action.Invoke(node.item);
        }

        public void LevelOrderTravesal(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("方法参数不得为空。");
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
        }
        
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
                        line.Add($"{node.item, -4}");
                        queue1.Enqueue(node.left);
                        queue1.Enqueue(node.right);
                        flag = true;
                    }
                }
                if(flag) table.Add(line);
            }
            int width =  8 << table.Count;
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
        /// 
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
        /// 平衡节点，消除出现的4节点。
        /// </summary>
        /// <param name="node"></param>
        /// <returns>平衡后的节点</returns>
        private static Node Balance(Node node)
        {
            if (IsRed(node.right)) node = RotateLeft(node);
            if (IsRed(node.left) && IsRed(node.left.left)) node = RotateRight(node);
            if (IsRed(node.left) && IsRed(node.right)) FlipColors(node);
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
