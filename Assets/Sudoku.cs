using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Sudoku : MonoBehaviour
{

    //TODO workout a formula for perfect alignment
    const float CELL_OFFSET = 9.0f / 2 - .070f; // board width / cell number
    public GameObject bigButton;

    Node[] nodes;

    Queue<Node> propagations;

    // Start is called before the first frame update
    void Start()
    {
        var cells = new Cell[81];
        nodes = new Node[81];
        var hoods = new Node[9, 9];
        propagations = new Queue<Node>();

        var canvas = this.transform.Find("Canvas");

        for (int i = 0; i < 81; i++)
        {
            //Init node
            var node = new Node();
            nodes[i] = node;
            node.nodes = nodes;
            node.hoods = hoods;
            node.propagations = propagations;
            node.id = i;
            node.row = i / 9;
            node.col = i % 9;

            var hid = ((i / 3) % 3) * 3 + (i / 27);
            var nid = (i % 3) + ((i / 9) * 3) % 9;

            hoods[hid, nid] = node;
            node.hood = hid;

            // Init cell
            var bobj = Instantiate(bigButton, canvas);
            // var lobj = Instantiate(lilButton, new Vector3(0, 0, -3), Quaternion.identity);
            bobj.name = "Cell" + i;
            cells[i] = bobj.GetComponent<Cell>();
            cells[i].ID = i;
            cells[i].spos = node.spos;
            cells[i].board = this;

            var squeeze_x = (i % 3) - 1;
            var squeeze_y = (i / 9) % 3 - 1;

            bobj.transform.Translate(
                -CELL_OFFSET + node.col * 1.107f - (squeeze_x * 0.025f),
                CELL_OFFSET - node.row * 1.107f + (squeeze_y * 0.025f),
            -0.02f);
        }
    }

    public void Set(int id, int v)
    {
        nodes[id].Set(v);
    }

    // Update is called once per frame
    void Update()
    {
        var c = 0;
        while(propagations.Count() > 0 && c < 50) {
            propagations.Dequeue().Propagate();
            c++;
        }
    }


    class Node
    {
        public int id = 0;
        public int hood = 0;
        public int row = 0;
        public int col = 0;
        public HashSet<int> spos = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public Node[] nodes;
        public Node[,] hoods;

        public Queue<Node> propagations;

        int Entropy()
        {
            return spos.Count;
        }

        int val()
        {
            return spos.Single();
        }

        bool Visit(int pos)
        {
            if (spos.Contains(pos))
            {
                spos.Remove(pos);
                var e = Entropy();
                if (e == 1)
                {
                    Debug.LogError("Setting shit on fire");
                    Set(val());
                } else if (e == 0) {
                    Debug.LogError("Something fucked up");
                }
            }
            return true;
        }

        public void Set(int v)
        {
            spos.Clear();
            spos.Add(v);
            propagations.Enqueue(this);
        }

        public void Propagate() {
            PropagateRow();
            PropagateCol();
            PropagateHood();
        }

        void PropagateRow()
        {
            for (int i = 0; i < 9; i++)
            {
                var n = nodes[row * 9 + i];
                if (n.id != id) n.Visit(val());
            }
        }

        void PropagateCol()
        {
            for (int i = 0; i < 9; i++)
            {
                var n = nodes[i * 9 + col];
                if (n.id != id) n.Visit(val());
            }
        }
        void PropagateHood()
        {
            for (int i = 0; i < 9; i++)
            {
                var n = hoods[hood, i];
                if (n.id != id) n.Visit(val());
            }
        }
    }

}
