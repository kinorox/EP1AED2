using System;
using System.Collections.Generic;
using System.Text;

namespace EP1
{
    public class DepthFirstSearch
    {
        private bool[] marked;
        private int count;

        public DepthFirstSearch(Frequentador[] g, int s)
        {
            marked = new bool[g.Length];
            dfs(g, s);
        }

        private void dfs(Frequentador[] g, int v)
        {
            marked[v] = true;
            count++;
            foreach (Frequentador w in g[v].Adjacentes)
                if(!marked[w.Index]) dfs(g, w.Index);
        }

        public bool isMarked(int w)
        {
            return marked[w];
        }

        public int Count()
        {
            return count;
        }
    }
}
