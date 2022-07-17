using System.Collections.Generic;
using System.Linq;

class BoardState
{
    public int[] board;
    public HashSet<int>[] spos; // super positions at that node
    Stack<int> propagations;
    int collapsed = 0;
    public bool hasHoles = false;
    Stack<Candidate> candidates;

    readonly static int[,] neighbors = InitNeighbors();
    static int[,] InitNeighbors()
    {
        // return new int[81,20];
        var neighbors = new int[81, 20];

        for (int i = 0; i < 81; i++)
        {
            var count = 0;
            for (int j = 0; j < 81; j++)
            {
                if (i != j && IsNeighbor(j, i) > 0)
                {
                    neighbors[i, count] = j;
                    count++;
                }
            }

            while (count < 20)
            {
                neighbors[i, count] = -1;
                count++;
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

        return neighbors;
    }

    public BoardState(int[] board)
    {
        InitState();

        for (int i = 0; i < 81; i++)
        {
            spos[i] = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            if (board[i] > 0)
            {
                SetCell(i, board[i]);
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
                    cs.Add(new Candidate(spos[i], i));
                }
            }
            InitCandidates(cs.Count, cs);
        }
        else
        {
            InitCandidates(0, new List<Candidate>());
        }

    }

    public BoardState(BoardState prev, int i, int val)
    {
        InitState();

        this.collapsed = prev.collapsed;

        for (int _i = 0; _i < 81; _i++)
        {
            this.board[_i] = prev.board[_i];
            this.spos[_i] = new HashSet<int>(prev.spos[_i]);
            if (_i == i)
            {
                SetCell(i, val);
            }
        }

        if (WaveFormCollapse())
        {
            InitCandidates(prev.candidates.Count, prev.candidates);
        }
        else
        {
            InitCandidates(0, new List<Candidate>());
        }
    }


    void InitState()
    {
        this.board = new int[81];
        this.spos = new HashSet<int>[81];
        this.propagations = new Stack<int>();
    }

    void InitCandidates(int count, IEnumerable<Candidate> candidates)
    {
        hasHoles = count == 0;

        // Re-use the previous candidates, as each selection narrows the possibilities.
        // If C# didnt suck i would be able to use PriorityDeque for this
        var cs = new List<Candidate>();
        foreach (var c0 in candidates)
        {
            if (this.board[c0.cell] == 0)
            {
                cs.Add(new Candidate(this.spos[c0.cell], c0.cell));
            }
        }
        // Negate this because we want the sorting in reverse, 
        // as it will populate the stack high count first.
        cs.Sort((c1, c2) => -c1.compareTo(c2));
        this.candidates = new Stack<Candidate>(cs);
    }

    public bool IsDone()
    {
        return collapsed >= 81 || hasHoles;
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
            var st = new BoardState(this, c.cell, val);
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
            var cell = propagations.Pop();
            var pos = this.spos[cell].Single();
            this.board[cell] = pos;
            for (int i = 0; i < 20; i++) // foreach neighbor
            {
                var n = neighbors[cell, i];

                if (n == -1) break; // the end of the neighbors

                switch (Visit(n, pos, this.spos))
                {
                    case Result.Collapsed:
                        collapsed++;
                        propagations.Push(n);
                        break;
                    case Result.LowEntropy:
                        this.hasHoles = true;
                        return false;
                }
            }
        }
        return true;
    }

    public Result Visit(int i, int pos, HashSet<int>[] spos)
    {
        if (spos[i].Contains(pos))
        {
            spos[i].Remove(pos);
            var entropy = spos[i].Count;

            switch (entropy)
            {
                case 0:
                    return Result.LowEntropy;
                case 1:
                    return Result.Collapsed;
            }
        }

        return Result.Undecided;
    }
    public enum Result { Undecided, Collapsed, LowEntropy }


    public void SetCell(int i, int val)
    {
        board[i] = val;
        spos[i] = new HashSet<int> { val };
        this.collapsed++;
        propagations.Push(i);
    }

    class Candidate
    {
        public int cell;
        public Stack<int> spos;
        public Candidate(HashSet<int> s, int i)
        {
            this.cell = i;
            this.spos = new Stack<int>(s);
        }

        public Candidate(Stack<int> s, int i)
        {
            this.cell = i;
            this.spos = new Stack<int>(s);
        }

        public int compareTo(Candidate c)
        {
            return this.spos.Count.CompareTo(c.spos.Count);
        }
    }
}
