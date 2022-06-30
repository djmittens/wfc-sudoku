using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SudokuSolver : MonoBehaviour
{

    Stack<BoardState> history;
    public GameObject canvas;

    // Start is called before the first frame update
    void Start()
    {
        var board = gameObject.GetComponent<SudokuBoard>();
        if(board != null) {
            board.Init(canvas);
        }
        var spos = gameObject.GetComponent<Superpositions>();
        if(spos != null) {
            spos.Init(canvas);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public class BoardState{
        public int[] board;
        public bool finish;
        public int nextNode;
        public int[] spos; // super positions at that node
    }
}
