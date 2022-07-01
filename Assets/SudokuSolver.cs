using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SudokuSolver : MonoBehaviour
{
    Stack<BoardState> history;
    public GameObject canvas;
    Node[] nodes;
    Stack<Node> propagations;

    // Start is called before the first frame update
    void Start()
    {
        this.history = new Stack<BoardState>();
        this.propagations = new Stack<Node>();
        var boardState = new BoardState();
        boardState.board = new int[] {
            0,0,0,0,0,0,0,0,0,
            0,7,0,0,0,0,0,0,0,
            0,0,0,0,0,0,4,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            1,0,0,0,8,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,2,0,0,0,0,0,0,
            0,0,0,0,0,0,3,0,0,
        };
        boardState.spos = new HashSet<int>[81];
        this.history.Push(boardState);

        this.nodes = new Node[81];
        for (int i = 0; i < 81; i++)
        {
            var n = new Node();
            n.id = i;
            n.hood = i / 3;
            n.row = i / 9;
            n.col = i % 9;
            n.neighbors = new List<Node>();
            n.propagations = this.propagations;
            nodes[i] = n;
        }

        var board = gameObject.GetComponent<SudokuBoard>();
        if (board != null)
        {
            board.Init(canvas);
            board.UpdateState(boardState.board);
        }
        var spos = gameObject.GetComponent<Superpositions>();
        if (spos != null)
        {
            for (int i = 0; i < 81; i ++) {
                boardState.spos[i] = new HashSet<int>{1,2,3,4,5,6,7,8,9};
            }

            for (int i = 0; i < 81; i ++) {
                if(boardState.board[i] > 0) {
                    nodes[i].Set(boardState.board[i], boardState.spos);
                }
            }

            spos.Init(canvas);
            spos.RegisterOnclick((i, v) =>
            {
                var st = history.Peek().spos;
                nodes[i].Set(v, st);
                nodes[i].Propagate(boardState);
                spos.UpdateState(boardState.spos);
                board.UpdateState(boardState.board);
            });
        }


        // For explanation https://www.desmos.com/calculator/rrucuhps2t
        int CheckRow(int candidate, int origin)
        {
            return Clamp(candidate/ 9 - origin / 9);
        }
        int CheckCol(int candidate, int origin)
        {
            return Clamp(candidate % 9 - origin % 9);
        }
        int CheckHood(int candidate, int origin)
        {
            return CheckK(candidate, origin) * Clamp((candidate / (9 * 3)) - (origin/ (9 * 3)));
        }
        int CheckK(int candidate, int origin)
        {
            return Clamp((candidate / 3) % 3 - (origin / 3) % 3);
        }
        int Clamp(int k)
        {
            if (k < 0)
            {
                return 0;
            }
            else if (k < 1)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        for (int i = 0; i < 81; i++)
        {
            for (int k = 0; k < 81; k++)
            {
                if (i != k && (CheckRow(k, i) + CheckCol(k, i) + CheckHood(k, i) ) > 0)
                {
                    nodes[k].neighbors.Add(nodes[i]);
                }
            }
        }

        nodes[0].Propagate(boardState);
        board.UpdateState(boardState.board);
        spos.UpdateState(boardState.spos);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public class BoardState
    {
        public int[] board;
        public HashSet<int>[] spos; // super positions at that node
    }
}

class Node
{
    public int id = 0;
    public int hood = 0;
    public int row = 0;
    public int col = 0;
    // public HashSet<int> spos = new HashSet<int>();

    public IList<Node> neighbors;

    public Stack<Node> propagations;

    int Entropy(HashSet<int>[] spos)
    {
        return spos[id].Count;
    }

    int val(HashSet<int>[] spos)
    {
        return spos[id].Single();
    }

    bool Visit(int pos, HashSet<int>[] spos)
    {
        if (spos[id].Contains(pos))
        {
            spos[id].Remove(pos);
            var e = Entropy(spos);
            if (e == 1)
            {
                Set(val(spos), spos);
            }
            else if (e == 0)
            {
                Debug.LogError("Something fucked up");
                return false;
            }
        }
        return true;
    }

    public void Set(int v, HashSet<int>[] spos)
    {
        spos[id].Clear();
        spos[id].Add(v);
        propagations.Push(this);
    }

    public bool Propagate(SudokuSolver.BoardState st)
    {
        var res = true;

        while (propagations.Count > 0)
        {
            var node = propagations.Pop();
            var pos = node.val(st.spos);
            st.board[node.id] = pos;
            foreach (var n in node.neighbors)
            {
                res &= n.Visit(pos, st.spos);
            }
        }
        return res;
    }

}