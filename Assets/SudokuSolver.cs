using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SudokuSolver : MonoBehaviour
{
    Stack<GameState> history;
    public GameObject canvas;
    Node[] nodes;
    SudokuBoard c_board;
    Superpositions c_superpositions;

    // Start is called before the first frame update
    void Start()
    {
        InitNodes();

        InitBoardState();

        this.c_board = gameObject.GetComponent<SudokuBoard>();
        if (this.c_board != null)
        {
            this.c_board.Init(canvas);
        }

        this.c_superpositions = gameObject.GetComponent<Superpositions>();
        if (this.c_superpositions != null)
        {
            this.c_superpositions.Init(canvas);
            this.c_superpositions.RegisterOnclick((i, v) =>
            {
                SetCell(i, v);
                var counter = 0;
                while (history.Count > 0 && history.Count < 81 && !history.Peek().IsDone() && counter < 81 * 81)
                {
                    var st = this.history.Pop();
                    var n = st.NextState();
                    if (n != null)
                    {
                        this.history.Push(st);
                        this.history.Push(n);
                        UpdateComponents();
                    }
                    counter++;
                }
                UpdateComponents();
            });
        }

        UpdateComponents();
    }

    void SetCell(int i, int v)
    {
        history.Push(new GameState(history.Peek(), nodes[i], v));
    }

    void InitNodes()
    {
        this.nodes = new Node[81];
        for (int i = 0; i < 81; i++)
        {
            var n = new Node();
            n.id = i;
            n.neighbors = new List<Node>();
            nodes[i] = n;
        }
        for (int i = 0; i < 81; i++)
        {
            for (int k = 0; k < 81; k++)
            {
                if (i != k && IsNeighbor(k, i) > 0)
                {
                    nodes[k].neighbors.Add(nodes[i]);
                }
            }
        }

        // For explanation https://www.desmos.com/calculator/rrucuhps2t
        int IsNeighbor(int candidate, int origin)
        {
            int Clamp(int k) { return k == 0 ? 1 : 0; };
            return
                Clamp((candidate / 3 - origin / 3) % 3) * //neighborhood
                Clamp(candidate / 27 - origin / 27) + // neighborhood mask
                Clamp(candidate / 9 - origin / 9) + //row
                Clamp((candidate - origin) % 9);  //column
        }
    }

    void InitBoardState()
    {
        this.history = new Stack<GameState>();
        var gameState = new GameState(new int[] {
            0,0,0,0,0,0,0,0,0,
            0,7,0,0,0,0,0,0,0,
            0,0,0,0,0,0,4,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            1,0,0,0,8,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,2,0,0,0,0,0,0,
            0,0,0,0,0,0,3,0,0,
        }, this.nodes);
        this.history.Push(gameState);
    }

    void UpdateComponents()
    {
        if (history.Count > 0)
        {
            var st = history.Peek();
            // st.WaveFormCollapse();
            if (st.propagations.Count != 0)
            {
                Debug.LogError("Shits not fucking 0: " + st.propagations.Count);
            }
            if (st.hasHoles)
            {
                Debug.LogError("Shits not fucking 0: " + st.propagations.Count);
            }

            if (this.c_board != null)
            {
                this.c_board.UpdateState(st.board);
            }
            if (this.c_superpositions != null)
            {
                this.c_superpositions.UpdateState(st.spos);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    class GameState
    {
        public int[] board;
        public HashSet<int>[] spos; // super positions at that node
        public Stack<Node> propagations;
        int collapsed = 0;
        public bool hasHoles = false;
        Stack<Candidate> candidates;

        public GameState(int[] board, Node[] nodes)
        {
            InitState();

            for (int i = 0; i < 81; i++)
            {
                spos[i] = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                if (board[i] > 0)
                {
                    SetCell(nodes[i], board[i]);
                }
            }

            if (WaveFormCollapse())
            {

                // Initialize candidates for DFS.
                var cs = new List<Candidate>();
                for (int i = 0; i < 81; i++)
                {
                    if (board[i] == 0)
                    {
                        cs.Add(new Candidate(spos[i], nodes[i]));
                    }
                }
                InitCandidates(cs);
            }
            else
            {
                InitCandidates(new List<Candidate>());
            }

        }

        public GameState(GameState prev, Node node, int val)
        {
            InitState();

            this.collapsed = prev.collapsed;

            for (int i = 0; i < 81; i++)
            {
                this.board[i] = prev.board[i];
                this.spos[i] = new HashSet<int>(prev.spos[i]);
                if (node.id == i)
                {
                    SetCell(node, val);
                }
            }

            InitCandidates(WaveFormCollapse() ? prev.candidates : new List<Candidate>());
        }


        void InitState()
        {
            this.board = new int[81];
            this.spos = new HashSet<int>[81];
            this.propagations = new Stack<Node>();
        }

        void InitCandidates(IEnumerable<Candidate> candidates)
        {
            hasHoles = candidates.Count() == 0;

            // Re-use the previous candidates, as each selection narrows the possibilities.
            var cs = new List<Candidate>();
            foreach (var c0 in candidates)
            {
                if (this.board[c0.node.id] == 0)
                {
                    cs.Add(new Candidate(this.spos[c0.node.id], c0.node));
                }
            }
            // Negate this because we want the sorting in reverse, 
            // as it will populate the stack high count first.
            cs.Sort((c1, c2) => -c1.compareTo(c2));
            this.candidates = new Stack<Candidate>(cs);
        }

        public bool IsDone()
        {
            return collapsed == 81 || hasHoles;
        }

        /**
         returns Optional<GameState>
         */
        public GameState NextState()
        {
            while (candidates.Count() > 0)
            {
                var c = candidates.Pop();
                var val = c.spos.Pop();
                var st = new GameState(this, c.node, val);
                if (c.spos.Count > 0)
                {
                    candidates.Push(c);
                }
                if (!st.hasHoles)
                {
                    return st;
                }
            }

            return null;
        }

        public bool WaveFormCollapse()
        {
            while (propagations.Count > 0)
            {
                var node = propagations.Pop();
                var pos = this.spos[node.id].Single();
                this.board[node.id] = pos;
                foreach (var n in node.neighbors)
                {
                    switch (n.Visit(pos, this.spos))
                    {
                        case Node.Result.Collapsed:
                            collapsed++;
                            propagations.Push(n);
                            break;
                        case Node.Result.LowEntropy:
                            this.hasHoles = true;
                            return false;
                    }
                }
            }
            return true;
        }

        public void SetCell(Node n, int val)
        {
            board[n.id] = val;
            spos[n.id] = new HashSet<int> { val };
            this.collapsed++;
            propagations.Push(n);
        }

        class Candidate
        {
            public Node node;
            public Stack<int> spos;
            public Candidate(HashSet<int> s, Node n)
            {
                this.node = n;
                this.spos = new Stack<int>(s);
            }

            public Candidate(Stack<int> s, Node n)
            {
                this.node = n;
                this.spos = new Stack<int>(s);
            }

            public int compareTo(Candidate c)
            {
                return this.spos.Count.CompareTo(c.spos.Count);
            }
        }
    }


    class Node
    {
        public int id = 0;

        public IList<Node> neighbors;

        public Result Visit(int pos, HashSet<int>[] spos)
        {
            var res = Result.Undecided;
            if (spos[id].Contains(pos))
            {
                spos[id].Remove(pos);
                var entropy = spos[id].Count;

                if (entropy == 1)
                {
                    res = Result.Collapsed;
                }
                else if (entropy == 0)
                {
                    res = Result.LowEntropy;
                }
            }
            return res;
        }

        public enum Result { Undecided, Collapsed, LowEntropy }
    }
}
