using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    const float LIL_OFFSET = .5f - 0.166665f; // half unit  - half width which becomes top left
    const float LIL_WIDTH = 1.0f / 3.0f;
    public TMPro.TMP_Text text;
    public HashSet<int> spos;

    public int ID;

    public GameObject lilButton;

    private GameObject[] buttons;

    public SudokuBoard board;

    // Start is called before the first frame update
    void Start()
    {
        // text = transform.Find("Text").gameObject;
        text.text = "";
        buttons = new GameObject[9];
        for (int i = 0; i < 9; i++)
        {
            var cellVal = i + 1;
            var bobj = Instantiate(lilButton, gameObject.transform);
            bobj.name = "Spos" + i;
            bobj.transform.SetParent(gameObject.transform, false);
            bobj.transform.Find("Text").GetComponent<TMPro.TMP_Text>().text = "" + cellVal;
            bobj.transform.Translate(
                -LIL_OFFSET + (i % 3) * LIL_WIDTH
                , LIL_OFFSET - (i / 3) * LIL_WIDTH
                , -.02f
            );

            bobj.GetComponent<Button>().onClick.AddListener(() => { board.Set(ID, cellVal);});
            
            buttons[i] = bobj;
        }
    }

    int entropy()
    {
        return spos.Count;
    }

    public void Set(int i) {

    }

    // Update is called once per frame
    void Update()
    {
        if (entropy() == 1)
        {
            text.text = "" + spos.Single();
            // Avoid unnecessary allocations, we are done with updates.
            // spos.Clear();
        }

        for (int i = 0; i < 9; i++)
        {
            // Activate if it contains stuff
            buttons[i].SetActive(spos.Contains(i + 1) && entropy() != 1);
        }
    }
}
