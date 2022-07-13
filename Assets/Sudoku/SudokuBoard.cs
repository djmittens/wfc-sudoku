using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SudokuBoard : MonoBehaviour
{
    const float CELL_OFFSET = 9.0f / 2 - .070f; // board width / cell number
    public int[] state;
    TMPro.TMP_Text[] cells;
    public GameObject cellObject;

    public void Init(GameObject canvas) {
        this.cells = new TMPro.TMP_Text[81];
        this.state = new int[81];
        for(int i = 0; i < 81; i ++ ) {
            var obj = Instantiate(cellObject, canvas.transform);
            obj.name = i.ToString();
            this.cells[i] = obj.transform.Find("Text").GetComponent<TMPro.TMP_Text>();
            this.UpdateCell(i, this.state[i]);
            var squeeze_x = (i % 3) - 1;
            var squeeze_y = (i / 9) % 3 - 1;

            var row = i / 9;
            var col = i % 9;
            obj.transform.Translate(
                -CELL_OFFSET + col * 1.107f - (squeeze_x * 0.025f),
                CELL_OFFSET - row * 1.107f + (squeeze_y * 0.025f),
            -0.02f);
        }
    }

    public void UpdateState(int[] state){
        for(int i = 0; i < 81; i++) {
            if(this.state[i] != state[i]) {
                this.state[i] = state[i];
                this.UpdateCell(i, state[i]);
            }
        }
    }

    void UpdateCell(int cell, int val) {
        if(val == 0) {
            this.cells[cell].text = "";
        } else {
            this.cells[cell].text = val.ToString();
        }
    }


    void Update() {}
    public void Reset() {}
}