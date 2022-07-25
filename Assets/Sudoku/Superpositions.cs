using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Superpositions : MonoBehaviour
{
    const float LIL_OFFSET = .5f - 0.166665f; // half unit  - half width which becomes top left
    const float LIL_WIDTH = 1.0f / 3.0f;
    public GameObject button;
    HashSet<int>[] state;

    GameObject[,] buttons;

    public void Init(GameObject canvas)
    {
        state = new HashSet<int>[81];
        buttons = new GameObject[81, 9];
        for (int c = 0; c < 81; c++)
        {
            state[c] = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var cell = canvas.transform.Find(c.ToString());
            for (int i = 0; i < 9; i++)
            {
                var cellVal = i + 1;
                var bobj = Instantiate(button, cell.transform);
                bobj.name = "Superposition " + i;
                bobj.transform.SetParent(cell.transform, false);
                bobj.transform.Find("Text").GetComponent<TMPro.TMP_Text>().text = "" + cellVal;
                bobj.transform.Translate(
                    -LIL_OFFSET + (i % 3) * LIL_WIDTH
                    , LIL_OFFSET - (i / 3) * LIL_WIDTH
                    , -.02f
                );
                // bobj.SetActive(false);
                this.buttons[c, i] = bobj;
            }
        }
    }

    public void UpdateState(HashSet<int>[] state)
    {
        for (int i = 0; i < 81; i++)
        {
            if (state[i].Count <= 1)
            {
                DisableCell(i);
            }
            else
            {
                UpdateCell(i, state[i]);
            }
        }
    }

    void DisableCell(int cell)
    {
        if (this.state[cell].Count == 0) return;

        for (int p = 1; p <= 9; p++)
        {
            buttons[cell, p - 1].SetActive(false);
        }
        this.state[cell] = new HashSet<int>();
    }

    void UpdateCell(int cell, HashSet<int> state)
    {
        if (this.state[cell] == state) return;

        for (int p = 1; p <= 9; p++)
        {
            buttons[cell, p - 1].SetActive(state.Contains(p));
        }
        this.state[cell] = new HashSet<int>(state);
    }

    public void RegisterOnclick(System.Action<int, int> callback)
    {
        for (int c = 0; c < 81; c++)
        {
            for (int i = 0; i < 9; i++)
            {
                var cb = this.buttons[c, i].GetComponent<Button>().onClick;
                // TODO: why does this make it work ? 
                var k = c;
                var j = i + 1;
                cb.RemoveAllListeners();
                cb.AddListener(() => callback(k, j));
            }
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}
