using System.Collections.Generic;
using UnityEngine;

public class SudokuSolver : MonoBehaviour
{
    public GameObject canvas;

    Stack<BoardState> history;
    CellNode[] nodes;
    SudokuBoard c_board;
    Superpositions c_superpositions;

    // Start is called before the first frame update
    void Start()
    {

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
                Solve(81);
            });
        }

        InitNodes();

        InitBoardState(new int[] {
            0,0,0,0,0,0,0,0,0,
            0,7,0,0,0,0,0,0,0,
            0,0,0,0,0,0,4,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            1,0,0,0,8,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,2,0,0,0,0,0,0,
            0,0,0,0,0,0,3,0,0,
        });
    }

    void InitNodes()
    {
        this.nodes = new CellNode[81];
        for (int i = 0; i < 81; i++)
        {
            var n = new CellNode();
            n.id = i;
            n.neighbors = new List<CellNode>();
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

    public void InitBoardState(int[] board)
    {
        this.history = new Stack<BoardState>();
        var gameState = new BoardState(board, this.nodes);
        this.history.Push(gameState);
        UpdateComponents();
    }

    void UpdateComponents()
    {
        if (history.Count > 0)
        {
            var st = history.Peek();

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

    bool SetCell(int i, int v)
    {
        var st = new BoardState(history.Peek(), nodes[i], v);
        if(!st.hasHoles) {
            history.Push(st);
            return true;
        }
        return false;
    }

    public int Solve(int iterations)
    {
        var count = 0;
        while (
            history.Count > 0 &&
            history.Count < 81 &&
            !history.Peek().IsDone() &&
            iterations > count)
        {
            var st = this.history.Pop();
            var n = st.NextState();
            if (n != null && !n.hasHoles)
            {
                this.history.Push(st);
                this.history.Push(n);
                UpdateComponents();
            }
            count++;
        }
        UpdateComponents();
        return count;
    }

    // Update is called once per frame
    void Update()
    {
    }


}
