using System.Collections.Generic;
using UnityEngine;

public class SudokuSolver : MonoBehaviour
{
    public GameObject canvas;
    public uint iterations;
    Stack<WFCState> history;
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
                Solve(iterations);
            });
        }

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

    public void InitBoardState(int[] board)
    {
        this.history = new Stack<WFCState>();
        var gameState = new WFCState(board);
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
                this.c_superpositions.UpdateState(st.superpositions);
            }
        }
    }

    bool SetCell(int i, int v)
    {
        var st = history.Peek().CollapseCell(i, v);
        if(!st.hasHoles) {
            history.Push(st);
            return true;
        }
        return false;
    }

    public int Solve(uint iterations)
    {
        var count = 0;

        while (
            history.Count > 0 &&
            history.Count < 81 &&
            !history.Peek().hasHoles &&
            iterations > count)
        {
            var st = this.history.Pop();
            var n = st.MaybeNextState();
            if (n != null && !n.hasHoles)
            {
                this.history.Push(st);
                this.history.Push(n);
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
