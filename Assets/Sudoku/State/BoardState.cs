using System.Collections.Generic;
using System.Linq;

class BoardState
{
    public int[] board;
    public HashSet<int>[] spos; // super positions at that node
    Stack<CellNode> propagations;
    int collapsed = 0;
    public bool hasHoles = false;
    Stack<Candidate> candidates;

    public BoardState(int[] board, CellNode[] nodes)
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
            InitCandidates(cs.Count, cs);
        }
        else
        {
            InitCandidates(0, new List<Candidate>());
        }

    }

    public BoardState(BoardState prev, CellNode node, int val)
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

        if(WaveFormCollapse()) {
            InitCandidates(prev.candidates.Count, prev.candidates);
        } else {
          InitCandidates(0, new List<Candidate>());
        }
    }


    void InitState()
    {
        this.board = new int[81];
        this.spos = new HashSet<int>[81];
        this.propagations = new Stack<CellNode>();
    }

    void InitCandidates(int count, IEnumerable<Candidate> candidates)
    {
        hasHoles = count == 0;

        // Re-use the previous candidates, as each selection narrows the possibilities.
        // If C# didnt suck i would be able to use PriorityDeque for this
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
    public BoardState NextState()
    {
        while (candidates.Count > 0)
        {
            var c = candidates.Pop();
            var val = c.spos.Pop();
            var st = new BoardState(this, c.node, val);
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
                    case CellNode.Result.Collapsed:
                        collapsed++;
                        propagations.Push(n);
                        break;
                    case CellNode.Result.LowEntropy:
                        this.hasHoles = true;
                        return false;
                }
            }
        }
        return true;
    }

    public void SetCell(CellNode n, int val)
    {
        board[n.id] = val;
        spos[n.id] = new HashSet<int> { val };
        this.collapsed++;
        propagations.Push(n);
    }

    class Candidate
    {
        public CellNode node;
        public Stack<int> spos;
        public Candidate(HashSet<int> s, CellNode n)
        {
            this.node = n;
            this.spos = new Stack<int>(s);
        }

        public Candidate(Stack<int> s, CellNode n)
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
