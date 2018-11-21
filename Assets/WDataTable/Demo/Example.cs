﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WDT;

public class Example : MonoBehaviour
{
    public WDataTable dataTable;
    public bool testDynamic;

    private IList<string> m_columns = null;
    private List<IList<object>> m_datas = null;
    private List<WColumnDef> m_columnDefs = null;

    // Use this for initialization
    void Start()
    {
        m_columns = new List<string>();
        m_datas = new List<IList<object>>();
        m_columns.Add("ID");
        m_columns.Add("A");
        m_columns.Add("B");
        m_columns.Add("C");
        m_columns.Add("D");
        m_columnDefs = new List<WColumnDef>();
        m_columnDefs.Add(new WColumnDef() {width = "40"});
        m_columnDefs.Add(null);
        m_columnDefs.Add(null);
        m_columnDefs.Add(null);
        m_columnDefs.Add(new WColumnDef() {width = "50%", disableSort = true});

        for (int i = 0; i < 30; i++)
        {
            m_datas.Add(GetRandomData(i));
        }

        dataTable.MsgHandle += HandleTableEvent;
        dataTable.InitDataTable(m_datas, m_columns, m_columnDefs);
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

    private List<object> GetRandomData(int i = 0)
    {
        return new List<object>
        {
            i + 1,
            "dsada" + i,
            20.1 + i,
            Random.Range(0.0f, 1.0f),
            new Vector3(1, i, 2)
        };
    }

    public void AddRow()
    {
        m_datas.Add(GetRandomData());
        dataTable.UpdateData(m_datas, null);
    }

    public void InsertRow(int index)
    {
        m_datas.Insert(index, GetRandomData());
        dataTable.UpdateData(m_datas, null);
    }

    public void RemoveRow(int index)
    {
        if (m_datas.Count == 0)
            return;

        m_datas.RemoveAt(index);
        dataTable.UpdateData(m_datas, null);
    }

    public void RemoveColumn(int index)
    {
        if (m_columns.Count == 0)
            return;

        m_columns.RemoveAt(index);
        foreach (var subData in m_datas)
        {
            subData.RemoveAt(index);
        }

        dataTable.UpdateData(m_datas, m_columns);
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