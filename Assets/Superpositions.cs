using System.Collections;
using System.Collections.Generic;
// using System.Func;
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
            var cell = canvas.transform.Find(c.ToString());
            for (int i = 0; i < 9; i++)
            {
                var cellVal = i + 1;
                var bobj = Instantiate(button, cell.transform);
                bobj.name = "Spos" + i;
                bobj.transform.SetParent(cell.transform, false);
                bobj.transform.Find("Text").GetComponent<TMPro.TMP_Text>().text = "" + cellVal;
                bobj.transform.Translate(
                    -LIL_OFFSET + (i % 3) * LIL_WIDTH
                    , LIL_OFFSET - (i / 3) * LIL_WIDTH
                    , -.02f
                );
                bobj.SetActive(false);
                this.buttons[c, i] = bobj;
            }
        }
    }

    public void UpdateState(HashSet<int>[] state)
    {
        for (int i = 0; i < 81; i++)
        {
            if (this.state[i] != state[i])
            {
                if (state[i].Count < 2)
                {
                    // clear board if entropy too low
                    for (int p = 0; p < 9; p++)
                    {
                        buttons[i, p].SetActive(false);
                    }
                }
                else
                {
                    // Otherwise match up the state
                    for (int p = 0; p < 9; p++)
                    {
                        buttons[i, p].SetActive(state[i].Contains(p));
                    }
                }
            }
        }
    }

    public void RegisterOnclick(System.Action<int, int> callback)
    {
        for (int c = 0; c < 81; c++)
        {
            for (int i = 0; i < 9; i++)
            {
                var cb = this.buttons[c, i].GetComponent<Button>().onClick;
                cb.RemoveAllListeners();
                cb.AddListener(() => callback(c, i));
            }
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}
