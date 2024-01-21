using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


namespace CodingGame.Practice.ThereIsNoSpoon
{
    /**
    * Don't let the machines win. You are humanity's last hope...
    **/
    class Map
    {
        int _width;
        int _height;
        List<List<Node>> _lines;
        
        public Map()
        {
            _width = 0;
            _height = 0;
            _lines = new List<List<Node>>();
        }

        public int Width { get => _width; set => _width = value; }
        public int Height { get => _height; set => _height = value; }
        public List<List<Node>> Lines { get => _lines; set => _lines = value; }

        public Tuple<int, int> GetFirstNode()
        {
            for (var h = 0; h < _height; h++)
            {
                for (var l = 0; l < _width; l++)
                {
                    Node node = _lines[h][l];
                    if (!node.IsChecked && node.IsNode == true)
                    {
                        node.IsChecked = true;
                        return new Tuple<int, int>(l, h);
                    }
                }
            }

            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> GetTheRightNode(Tuple<int, int> node)
        {
            int l = node.Item1 + 1;
            int h = node.Item2;

            if (l < _width)
            {
                for (int i = l; i < _width; i++)
                while (l < _width)
                {
                    if (_lines[h][l].IsNode)
                        return new Tuple<int, int>(l, h);
                    
                    l++;
                }
            }
            
            return new Tuple<int, int>(-1, -1);
        }

        public Tuple<int, int> GetTheBottomNode(Tuple<int, int> node)
        {
            int l = node.Item1;
            int h = node.Item2 + 1;

            if (h < _height)
            {
                while (h < _height)
                {
                    if (_lines[h][l].IsNode)
                        return new Tuple<int, int>(l, h);

                    h++;
                }
            }
            
            return new Tuple<int, int>(-1, -1);
        }

        public bool IsThereUncheckedNodes()
        {
            foreach (List<Node> nodes in _lines)
            {
                if (nodes.Any(x => !x.IsChecked))
                    return true;
            }

            return false;
        }
    }

    class Node
    {
        bool _isNode;
        bool _isChecked;

        public Node(char c)
        {
            if (c == '0')
            {
                _isNode = true;
                _isChecked = false;
            }
            else
            {
                _isNode = false;
                _isChecked = true;
            }
        }

        public bool IsNode { get => _isNode; set => _isNode = value; }
        public bool IsChecked { get => _isChecked; set => _isChecked = value; }
    }

    class Player
    {
        static void Main(string[] args)
        {
            Map m = new Map();
            Tuple<int, int> startingCoordonate = new Tuple<int, int>(0, 0);

            m.Width = int.Parse(Console.ReadLine()); // the number of cells on the X axis
            m.Height = int.Parse(Console.ReadLine()); // the number of cells on the Y axis
            
            for (int i = 0; i < m.Height; i++)
            {
                List<Node> nodes = new List<Node>();
                string line = Console.ReadLine(); // width characters, each either 0 or .
                for (var j = 0; j < line.Length; j++)
                {
                    nodes.Add(new Node(line.Substring(j, 1).ToCharArray()[0]));                
                }
                    
                m.Lines.Add(nodes);
            }

            StringBuilder sb = new StringBuilder();
            foreach (List<Node> nodes in m.Lines)
            {
                foreach (Node n in nodes)
                {
                    if (n.IsNode)
                        sb.Append(" 0 ");
                    else
                        sb.Append(" . ");
                }
                sb.AppendLine();
            }
            Console.Error.WriteLine(sb);

            while (m.IsThereUncheckedNodes())
            {
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                var node = m.GetFirstNode();
                var rightNode = m.GetTheRightNode(node);
                var bottomNode = m.GetTheBottomNode(node);

                if (rightNode.Item1 != -1)
                    startingCoordonate = rightNode;
                else if (bottomNode.Item1 != -1)
                    startingCoordonate = bottomNode;

                // Three coordinates: a node, its right neighbor, its bottom neighbor
                Console.WriteLine($"{node.Item1} {node.Item2} {rightNode.Item1} {rightNode.Item2} {bottomNode.Item1} {bottomNode.Item2}");
            }
        }
    }
}
