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
                history.Peek().SetCell(nodes[i], v);
                UpdateComponents();
            });
        }

        UpdateComponents();
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
            int Clamp(int k) { return k == 0 ? 1 : 0; }
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
        var st = history.Peek();
        st.WaveFormCollapse();

        if (this.c_board != null)
        {
            this.c_board.UpdateState(st.board);
        }
        if (this.c_superpositions != null)
        {
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
        Stack<Node> propagations;
        int collapsed = 0;

        public GameState(int[] board, Node[] nodes)
        {
            this.board = new int[81];
            this.spos = new HashSet<int>[81];
            this.propagations = new Stack<Node>();

            for (int i = 0; i < 81; i++)
            {
                spos[i] = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                if (board[i] > 0)
                {
                    SetCell(nodes[i], board[i]);
                }
            }
        }

        public void SetCell(Node n, int val)
        {
            board[n.id] = val;
            spos[n.id].Clear();
            spos[n.id].Add(val);
            propagations.Push(n);
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
                    var r = n.Visit(pos, this.spos);
                    switch (r)
                    {
                        case Node.Result.Collapsed:
                            propagations.Push(n);
                            break;
                        case Node.Result.LowEntropy:
                            Debug.LogError("Something fucked up");
                            return false;
                    }
                }
            }
            return true;
        }
    }

    class Node
    {
        public int id = 0;

        public IList<Node> neighbors;

        public Result Visit(int pos, HashSet<int>[] spos)
        {
            var res = Result.Success;
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

        public enum Result { Success, Collapsed, LowEntropy }
    }
}
