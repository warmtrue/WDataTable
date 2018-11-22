using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WDT;

public class Example : MonoBehaviour
{
    public WDataTable dataTable;
    public bool testDynamic;
    public Text text;

    private IList<string> m_columns = null;
    private List<IList<object>> m_datas = null;
    private List<WColumnDef> m_columnDefs = null;
    private int m_tempSelectIndex = -1;

    // Use this for initialization
    void Start()
    {
        m_columns = new List<string>();
        m_datas = new List<IList<object>>();
        // name is necessary in columnDefs
        m_columnDefs = new List<WColumnDef>
        {
            new WColumnDef() {name = "ID", width = "40"},
            new WColumnDef()
            {
                name = "A",
                elementPrefabName = "ButtonElement",
                headPrefabName = "TextElement"
            },
            new WColumnDef() {name = "B"},
            new WColumnDef() {name = "C"},
            new WColumnDef() {name = "D", width = "50%", disableSort = true}
        };

        for (int i = 0; i < 30; i++)
        {
            m_datas.Add(GetRandomData(i));
        }

        dataTable.MsgHandle += HandleTableEvent;
        dataTable.InitDataTable(m_datas, m_columnDefs);
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
            Text tText = element.GetComponent<Text>();
            if (tText == null)
                return;
            tText.color = columnIndex % 2 == 0 ? Color.blue : Color.red;
        }
        else if (messageType == WEventType.SELECT_ROW)
        {
            int rowIndex = (int) args[0];
            if (text != null)
                text.text = "Select Row" + rowIndex;
            m_tempSelectIndex = rowIndex;
        }
    }

    private List<object> GetRandomData(int i = -1)
    {
        return new List<object>
        {
            i,
            "dsada" + i,
            20.1 + i,
            Random.Range(0.0f, 1.0f),
            new Vector3(1, i, 2)
        };
    }

    public void AddRow()
    {
        m_datas.Add(GetRandomData());
        dataTable.UpdateData(m_datas);
    }

    public void InsertRow(int index)
    {
        m_datas.Insert(index, GetRandomData());
        dataTable.UpdateData(m_datas);
    }

    public void RemoveRow(int index)
    {
        if (m_datas.Count == 0)
            return;

        m_datas.RemoveAt(index);
        dataTable.UpdateData(m_datas);
    }

    public void RemoveSelectRow()
    {
        if (m_datas.Count == 0)
            return;

        if (m_tempSelectIndex < 0 || m_tempSelectIndex >= m_datas.Count)
            return;

        int oldSize = m_datas.Count;
        float oldPostion = dataTable.GetPosition();
        m_datas.RemoveAt(m_tempSelectIndex);
        int newSize = m_datas.Count;
        dataTable.UpdateData(m_datas);
        dataTable.SetPosition(dataTable.GetPositionByNewSize(oldPostion, oldSize, newSize));
    }

    private void Update()
    {
        if (!testDynamic)
            return;
        dataTable.tableWidth = (int) (Mathf.Sin(Time.time * 2) * 100) + 600;
        dataTable.tableHeight = (int) (Mathf.Sin(Time.time * 2) * 50) + 200;
        dataTable.itemHeight = (int) (Mathf.Cos(Time.time * 2) * 10) + 40;
        dataTable.UpdateSize();
    }
}