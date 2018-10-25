using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace WDT
{
    public class WHead : WContainter
    {
        private HorizontalLayoutGroup m_hLayoutGroup;
        private RectTransform m_rectTransform;

        protected override void InitContainter(string prefabName)
        {
            base.InitContainter(prefabName);
            m_rectTransform = GetComponent<RectTransform>();
            m_hLayoutGroup = GetComponent<HorizontalLayoutGroup>();
            Assert.IsNotNull(m_rectTransform);
            Assert.IsNotNull(m_hLayoutGroup);
        }

        public void UpdateHeadSize()
        {
            if (bindDataTable == null || columnSize <= 0)
                return;

            m_rectTransform.sizeDelta = new Vector2(bindDataTable.tableWidth, bindDataTable.itemHeight);
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].SetSize(bindDataTable.GetWidthByColumnIndex(i), bindDataTable.itemHeight);
            }
        }

        public void SetColumnInfo(IList<string> columns, WDataTable dataTable)
        {
            bindDataTable = dataTable;
            if (!init)
                InitContainter(bindDataTable.headElementPrefab);

            int size = columns.Count;
            if (columnSize != size)
            {
                columnSize = size;
                BuildChild();
            }

            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].SetInfo(columns[i], -1, i, bindDataTable);
            }

            UpdateHeadSize();
        }
    }
}