using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SudokuSolver : MonoBehaviour
{

    public SudokuBoard board;

    Stack<BoardState> history;

    // Start is called before the first frame update
    void Start()
    {
        
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
