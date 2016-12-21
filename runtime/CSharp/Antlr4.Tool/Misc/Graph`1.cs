/*
 * [The "BSD license"]
 *  Copyright (c) 2012 Terence Parr
 *  Copyright (c) 2012 Sam Harwell
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *  1. Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 *  3. The name of the author may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 *  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 *  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 *  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 *  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 *  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 *  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 *  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace Antlr4.Misc
{
    using System.Collections.Generic;

    /** A generic graph with edges; Each node as a single Object payload.
     *  This is only used to topologically sort a list of file dependencies
     *  at the moment.
     */
    public class Graph<T>
    {

        public class Node
        {
            internal T payload;
            internal IList<Node> edges; // points at which nodes?

            public Node(T payload)
            {
                this.payload = payload;
            }

            public virtual void AddEdge(Node n)
            {
                if (edges == null)
                    edges = new List<Node>();
                if (!edges.Contains(n))
                    edges.Add(n);
            }

            public override string ToString()
            {
                return payload.ToString();
            }
        }

        /** Map from node payload to node containing it */
        protected IDictionary<T, Node> nodes = new LinkedHashMap<T, Node>();

        public virtual void AddEdge(T a, T b)
        {
            //System.out.println("add edge "+a+" to "+b);
            Node a_node = GetNode(a);
            Node b_node = GetNode(b);
            a_node.AddEdge(b_node);
        }

        protected virtual Node GetNode(T a)
        {
            Node existing;
            if (nodes.TryGetValue(a, out existing) && existing != null)
                return existing;

            Node n = new Node(a);
            nodes[a] = n;
            return n;
        }

        /** DFS-based topological sort.  A valid sort is the reverse of
         *  the post-order DFA traversal.  Amazingly simple but true.
         *  For sorting, I'm not following convention here since ANTLR
         *  needs the opposite.  Here's what I assume for sorting:
         *
         *    If there exists an edge u -&gt; v then u depends on v and v
         *    must happen before u.
         *
         *  So if this gives nonreversed postorder traversal, I get the order
         *  I want.
         */
        public virtual IList<T> Sort()
        {
            ISet<Node> visited = new OrderedHashSet<Node>();
            List<T> sorted = new List<T>();
            while (visited.Count < nodes.Count)
            {
                // pick any unvisited node, n
                Node n = null;
                foreach (Node tNode in nodes.Values)
                {
                    n = tNode;
                    if (!visited.Contains(n))
                        break;
                }
                if (n != null)
                { // if at least one unvisited
                    DFS(n, visited, sorted);
                }
            }
            return sorted;
        }

        public void DFS(Node n, ISet<Node> visited, List<T> sorted)
        {
            if (visited.Contains(n))
                return;

            visited.Add(n);
            if (n.edges != null)
            {
                foreach (Node target in n.edges)
                {
                    DFS(target, visited, sorted);
                }
            }

            sorted.Add(n.payload);
        }
    }
}
