using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

class BoardState
{
    public int[] board { get; private set; }
    public int collapsed { get; private set; }
    public bool hasHoles { get; private set; }
    public HashSet<int>[] superpositions { get; private set; } // super positions at that node

    Stack<int> propagations;
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
                if (i != j && IsNeighbor(j, i))
                {
                    neighbors[i, count] = j;
                    count++;
                }
            }
        }

        // For explanation https://www.desmos.com/calculator/rrucuhps2t
        bool IsNeighbor(int candidate, int origin)
        {
            return
                (Clamp((candidate / 3 - origin / 3) % 3) && //neighborhood
                Clamp(candidate / 27 - origin / 27)) || // neighborhood mask
                Clamp(candidate / 9 - origin / 9) || //row
                Clamp((candidate - origin) % 9);  //column

            bool Clamp(int k) { return k == 0; };
        }

        return neighbors;
    }

    public BoardState(int[] board)
    {
        InitState();

        for (int i = 0; i < 81; i++)
        {
            superpositions[i] = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            if (board[i] > 0)
            {
                _CollapseCell(i, board[i]);
            }
        }

        if (WaveFormCollapse())
        {

            // Initialize candidates for DFS.
            var cs = new List<Candidate>();
            for (int i = 0; i < 81; i++)
            {
                if (board[i] == 0 && superpositions[i].Count > 0)
                {
                    cs.Add(new Candidate(superpositions[i], i));
                }
            }
            InitCandidates(cs.Count, cs);
        }
        else
        {
            InitCandidates(0, Enumerable.Empty<Candidate>());
        }

    }

    public BoardState(BoardState prev, int i, int val)
    {
        InitState();

        this.collapsed = prev.collapsed;

        for (int _i = 0; _i < 81; _i++)
        {
            this.board[_i] = prev.board[_i];
            this.superpositions[_i] = new HashSet<int>(prev.superpositions[_i]);
            if (_i == i)
            {
                _CollapseCell(i, val);
            }
        }

        if (WaveFormCollapse())
        {
            InitCandidates(prev.candidates.Count, prev.candidates);
        }
        else
        {
            InitCandidates(0, Enumerable.Empty<Candidate>());
        }
    }


    void InitState()
    {
        this.board = new int[81];
        this.superpositions = new HashSet<int>[81];
        this.propagations = new Stack<int>();
        this.hasHoles = false;
    }

    void InitCandidates(int count, IEnumerable<Candidate> candidates)
    {
        // Re-use the previous candidates, as each selection narrows the possibilities.
        // If C# didnt suck i would be able to use PriorityDeque for this
        var cs = new List<Candidate>();
        foreach (var c0 in candidates)
        {
            if (this.board[c0.cell] == 0)
            {
                cs.Add(new Candidate(this.superpositions[c0.cell], c0.cell));
            }
        }
        // Negate this because we want the sorting in reverse, 
        // as it will populate the stack high count first.
        cs.Sort();
        this.candidates = new Stack<Candidate>(cs.Reverse<Candidate>());
    }

    /**
     returns Optional<GameState>
     */
    public BoardState MaybeNextState()
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

    bool WaveFormCollapse()
    {
        while (propagations.Count > 0)
        {
            var cell = propagations.Pop();
            var pos = this.superpositions[cell].Single();
            this.board[cell] = pos;
            for (int i = 0; i < 20; i++) // foreach neighbor
            {
                var n = neighbors[cell, i];

                switch (Visit(n, pos, this.superpositions))
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

    Result Visit(int i, int pos, HashSet<int>[] spos)
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

    private enum Result { Undecided, Collapsed, LowEntropy }

    private void _CollapseCell(int i, int val)
    {
        board[i] = val;
        superpositions[i] = new HashSet<int> { val };
        this.collapsed++;
        propagations.Push(i);
    }

    public BoardState CollapseCell(int i, int val) {
        return new BoardState(this, i, val);
    }

    private class Candidate : System.IComparable
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

        public int CompareTo(object obj)
        {
            if (obj is Candidate)
            {
                return this.spos.Count.CompareTo(((Candidate)obj).spos.Count);
            }
            else
            {
                return -1;
            }
        }
    }
}
