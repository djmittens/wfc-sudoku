using System.Collections.Generic;
using System.Linq;

class WFCState
{
    public int[] board { get; private set; }
    public int collapsed { get; private set; }
    public bool hasHoles { get; private set; }
    public HashSet<int>[] superpositions { get; private set; } // super positions at that node

    private Stack<Candidate> candidates;

    public WFCState(int[] board)
    {
        InitState();

        var propagations = new Stack<int>();
        for (int i = 0; i < 81; i++)
        {
            superpositions[i] = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            if (board[i] > 0)
            {
                _CollapseCell(i, board[i]);
                propagations.Push(i);
            }
        }

        if (WaveFormCollapse(propagations))
        {
            // Initialize candidates for DFS.
            InitCandidates(Enumerable
                .Range(0, 81)
                .Where(i => board[i] == 0 && superpositions[i].Count > 0));
        }
        else
        {
            InitCandidates(Enumerable.Empty<int>());
        }
    }

    public WFCState(WFCState prev, int i, int val)
    {
        InitState();

        this.collapsed = prev.collapsed;
        var propagations = new Stack<int>();

        for (int _i = 0; _i < 81; _i++)
        {
            this.board[_i] = prev.board[_i];
            this.superpositions[_i] = new HashSet<int>(prev.superpositions[_i]);
            if (_i == i)
            {
                _CollapseCell(i, val);
                propagations.Push(i);
            }
        }

        if (WaveFormCollapse(propagations))
        {
            InitCandidates(prev.candidates.Select(c => c.cell));
        }
        else
        {
            InitCandidates(Enumerable.Empty<int>());
        }
    }


    void InitState()
    {
        this.board = new int[81];
        this.superpositions = new HashSet<int>[81];
        this.hasHoles = false;
    }

    void InitCandidates(IEnumerable<int> candidates)
    {
        // Re-use the previous candidates, as each selection narrows the possibilities.
        this.candidates = new Stack<Candidate>(
            candidates
                .Select(i => new Candidate(new Stack<int>(this.superpositions[i]), i))
                .OrderBy(c => c.spos.Count)
                .Reverse()
        );
    }

    private void _CollapseCell(int i, int val)
    {
        this.board[i] = val;
        this.superpositions[i] = new HashSet<int> { val };
        this.collapsed++;
    }

    public WFCState CollapseCell(int i, int val)
    {
        return new WFCState(this, i, val);
    }

    /**
     returns Optional<GameState>
     */
    public WFCState MaybeNextState()
    {
        while (candidates.Count > 0)
        {
            var c = candidates.Pop();
            var val = c.spos.Pop();
            var st = new WFCState(this, c.cell, val);
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

    private bool WaveFormCollapse(Stack<int> propagations)
    {
        while (propagations.Count > 0)
        {
            var cell = propagations.Pop();
            var pos = this.superpositions[cell].Single();
            this.board[cell] = pos;

            for (int i = 0; i < 20; i++) // foreach neighbor
            {
                var n = WFCNeighbors.table[cell, i];

                switch (PropagateCollapse(n, pos, this.superpositions))
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

    Result PropagateCollapse(int i, int val, HashSet<int>[] spos)
    {
        if (spos[i].Contains(val))
        {
            spos[i].Remove(val);
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

    private record Candidate(Stack<int> spos, int cell);
}

static class WFCNeighbors
{
    public readonly static int[,] table = InitNeighbors();
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

        return neighbors;
    }

    // For explanation https://www.desmos.com/calculator/rrucuhps2t
    static bool IsNeighbor(int candidate, int origin)
    {
        return
            Clamp((candidate / 3 - origin / 3) % 3) && //neighborhood
            Clamp(candidate / 27 - origin / 27) || // neighborhood mask
            Clamp(candidate / 9 - origin / 9) || //row
            Clamp((candidate - origin) % 9);  //column

        bool Clamp(int k) { return k == 0; };
    }
}

// LOL, really great language we got here  
// https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}