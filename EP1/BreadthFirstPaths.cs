using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace EP1
{
    public class BreadthFirstPaths
    {
        private bool[] marked;
        private int[] edgeTo;
        private readonly int s;

        public BreadthFirstPaths(Frequentador[] g, int s)
        {
            marked = new bool[g.Length];
            edgeTo = new int[g.Length];
            this.s = s;
            bfp(g, s);
        }

        private void bfp(Frequentador[] g, int s)
        {
            Queue<int> queue = new Queue<int>();
            marked[s] = true;
            queue.Enqueue(s);

            while (queue.Count != 0)
            {
                int v = queue.Dequeue();

                if (g[v] == null)
                    return;

                if (g[v].Adjacentes == null)
                    return;

                foreach (Frequentador w in g[v].Adjacentes)
                    if (!marked[w.Index])
                    {
                        edgeTo[w.Index] = v;
                        marked[w.Index] = true;
                        queue.Enqueue(w.Index);
                        bfp(g, w.Index);
                    }
            }
        }

        public bool HasPathTo(int v)
        {
            return marked[v];
        }

        public int GetDistance(int v)
        {
            int count = 0;
            if (!HasPathTo(v)) return -1;
            Stack<int> path = new Stack<int>();
            for (int x = v; x != s; x = edgeTo[x])
            {
                path.Push(x);
                count++;
            }
                
            path.Push(s);

            return count;
        }

        public Stack<int> PathTo(int v)
        {
            if (!HasPathTo(v)) return null;
            Stack<int> path = new Stack<int>();
            for (int x = v; x != s; x = edgeTo[x])
                path.Push(x);
            path.Push(s);
            return path;
        }
    }
}
