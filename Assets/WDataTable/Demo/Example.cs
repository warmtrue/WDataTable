using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WDT;

public class Example : MonoBehaviour
{
    public WDataTable dataTable;
    public bool testDynamic;

    // Use this for initialization
    void Start()
    {
        IList<string> columns = new List<string>();
        IList<IList<object>> datas = new List<IList<object>>();
        columns.Add("ID");
        columns.Add("A");
        columns.Add("B");
        columns.Add("C");
        columns.Add("D");
        IList<WColumnDef> columnDefs = new List<WColumnDef>();
        columnDefs.Add(new WColumnDef() {width = "40"});
        columnDefs.Add(null);
        columnDefs.Add(null);
        columnDefs.Add(null);
        columnDefs.Add(new WColumnDef() {width = "50%", disableSort = true});

        for (int i = 0; i < 120; i++)
        {
            var tdatas = new List<object>
            {
                i + 1,
                "dsada" + i,
                20.1 + i,
                Random.Range(0.0f, 1.0f),
                new Vector3(1, i, 2)
            };
            datas.Add(tdatas);
        }

        dataTable.MsgHandle += HandleTableEvent;
        dataTable.InitDataTable(datas, columns, columnDefs);
    }

    public void HandleTableEvent(WEventType messageType, params object[] args)
    {
        if (messageType == WEventType.INIT_ELEMENT)
        {
            int rowIndex = (int) args[0];
            int columnIndex = (int) args[1];
            WElement element = args[2] as WElement;
            if (element == null)
                return;
            Text text = element.GetComponent<Text>();
            if (text == null)
                return;
            text.color = columnIndex % 2 == 0 ? Color.blue : Color.red;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!testDynamic)
            return;
        dataTable.tableWidth = (int) (Mathf.Sin(Time.time * 2) * 100) + 600;
        dataTable.tableHeight = (int) (Mathf.Sin(Time.time * 2) * 50) + 200;
        dataTable.itemHeight = (int) (Mathf.Cos(Time.time * 2) * 10) + 40;
        dataTable.UpdateSize();
    }
}