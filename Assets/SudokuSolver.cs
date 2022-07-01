using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SudokuSolver : MonoBehaviour
{
    Stack<GameState> history;
    public GameObject canvas;
    Node[] nodes;
    Stack<Node> propagations;
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
                var st = history.Peek().spos;
                nodes[i].Set(v, st);
                UpdateComponents();
            });
        }

        UpdateComponents();
    }

    void InitNodes() {
        this.propagations = new Stack<Node>();
        this.nodes = new Node[81];
        for (int i = 0; i < 81; i++)
        {
            var n = new Node();
            n.id = i;
            n.neighbors = new List<Node>();
            n.propagations = this.propagations;
            nodes[i] = n;
        }
        for (int i = 0; i < 81; i++)
        {
            for (int k = 0; k < 81; k++)
            {
                if (i != k && (CheckRow(k, i) + CheckCol(k, i) + CheckHood(k, i)) > 0)
                {
                    nodes[k].neighbors.Add(nodes[i]);
                }
            }
        }

        // For explanation https://www.desmos.com/calculator/rrucuhps2t
        int CheckRow(int candidate, int origin)
        {
            return Clamp(candidate / 9 - origin / 9);
        }
        int CheckCol(int candidate, int origin)
        {
            return Clamp((candidate - origin) % 9);
        }
        int CheckHood(int candidate, int origin)
        {
            return CheckK(candidate, origin) * Clamp((candidate / (9 * 3)) - (origin / (9 * 3)));
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
    }

    void InitBoardState() {
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

    void UpdateComponents() {
        var st = history.Peek();
        st.Propagate(propagations);
        if(this.c_board != null) {
            this.c_board.UpdateState(st.board);
        }
        if(this.c_superpositions != null) {
            this.c_superpositions.UpdateState(st.spos);
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
        public GameState(int[] board, Node[] nodes)
        {
            this.board = new int[81];
            this.spos = new HashSet<int>[81];
            for (int i = 0; i < 81; i++)
            {
                spos[i] = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                if(board[i] > 0) {
                    nodes[i].Set(board[i], spos);
                }
            }
        }
        public bool Propagate(Stack<Node> propagations)
        {
            var res = true;

            while (propagations.Count > 0)
            {
                var node = propagations.Pop();
                var pos = node.val(this.spos);
                this.board[node.id] = pos;
                foreach (var n in node.neighbors)
                {
                    res &= n.Visit(pos, this.spos);
                }
            }
            return res;
        }
    }

    class Node
    {
        public int id = 0;

        public IList<Node> neighbors;

        public Stack<Node> propagations;

        int Entropy(HashSet<int>[] spos)
        {
            return spos[id].Count;
        }

        public int val(HashSet<int>[] spos)
        {
            return spos[id].Single();
        }

        public bool Visit(int pos, HashSet<int>[] spos)
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


    }
}
