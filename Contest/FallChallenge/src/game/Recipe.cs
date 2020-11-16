using System;
using System.Linq;
using System.Text;

namespace CodingGame.Contest.FallChallenge.src.game
{
    public class Recipe
    {
        int[] _delta;

        public static readonly string[] CHARS = new string[] { "A", "B", "C", "D" };

        public int[] Delta { get => _delta; set => _delta = value; }

        public Recipe(int a, int b, int c, int d) 
        {
            _delta = new int[] { a, b, c, d };
        }

        public Recipe() 
        {
            _delta = new int[] { 0, 0, 0, 0 };
        }

        public Recipe(Recipe other) 
        {
            new Recipe(other.Delta[0], other.Delta[1], other.Delta[2], other.Delta[3]);
        }

        public override String ToString() 
        {
            StringBuilder sb = new StringBuilder();
            if (_delta.All(b => b == 0)) 
            {
                return "∅";
            }

            if (_delta.Any(b => b > 0)) 
            {
                sb.Append("+");
                for (int i = 0; i < 4; ++i) 
                {
                    for (int k = 0; k < _delta[i]; ++k) 
                    {
                        sb.Append(CHARS[i]);
                    }
                }
            }

            if (_delta.Any(b => b < 0)) 
            {
                if (_delta.Any(b => b > 0)) 
                {
                    sb.Append('\n');
                }
                sb.Append("-");
                for (int i = 0; i < 4; ++i) 
                {
                    for (int k = _delta[i]; k < 0; ++k) 
                    {
                        sb.Append(CHARS[i]);
                    }
                }
            }

            return sb.ToString();
        }

        public String ToPlayerString() 
        {
            return String.Format("%d %d %d %d", _delta[0], _delta[1], _delta[2], _delta[3]);
        }

        public void Add(int idx, int x) 
        {
            _delta[idx] += x;
        }

        public int GetTotal() 
        {
            return _delta.Sum();
        }

        public int GetTotalLoss() 
        {
            return -_delta
                .Where(i => i < 0)
                .Sum();
        }

        public int GetTotalGain() 
        {
            return _delta
                .Where(i => i > 0)
                .Sum();
        }

        public override int GetHashCode()
        {
            return _delta.GetHashCode();
        }

        public override bool Equals(Object obj) {
            if (this == obj) {
                return true;
            }

            if (!(obj.GetType() == typeof(Recipe))) {
                return false;
            }

            Recipe other = (Recipe)obj;

            return _delta.Equals(other.Delta);
        }
    }
}
