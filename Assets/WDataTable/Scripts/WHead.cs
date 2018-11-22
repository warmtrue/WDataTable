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

        protected override void InitContainter()
        {
            base.InitContainter();
            m_rectTransform = GetComponent<RectTransform>();
            m_hLayoutGroup = GetComponent<HorizontalLayoutGroup>();
            Assert.IsNotNull(m_rectTransform);
            Assert.IsNotNull(m_hLayoutGroup);
        }

        protected override string GetObjectName(int columnIndex)
        {
            if (bindDataTable == null || columnsDefs.Count <= 0)
                return "";

            if (columnIndex < 0 || columnIndex >= columnsDefs.Count)
                return "";

            string objectName = columnsDefs[columnIndex].headPrefabName;
            return string.IsNullOrEmpty(objectName) ? bindDataTable.defaultHeadPrefabName : objectName;
        }

        public void UpdateHeadSize()
        {
            if (bindDataTable == null || columnsDefs.Count <= 0)
                return;

            m_rectTransform.sizeDelta = new Vector2(bindDataTable.tableWidth, bindDataTable.itemHeight);
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].SetSize(bindDataTable.GetWidthByColumnIndex(i), bindDataTable.itemHeight);
            }
        }

        public void SetColumnInfo(IList<WColumnDef> columnsDefsIn, WDataTable dataTable)
        {
            bindDataTable = dataTable;
            if (!init)
                InitContainter();

            columnsDefs = columnsDefsIn;
            BuildChild();

            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].SetInfo(columnsDefs[i].name, -1, i, bindDataTable);
            }

            UpdateHeadSize();
        }
    }
}