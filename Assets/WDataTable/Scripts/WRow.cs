using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WDT
{
    public class WRow : WContainter
    {
        private RectTransform m_rectTransform;
        private LayoutElement m_layoutElement;
        private Button m_button;

        protected override void InitContainter(string prefabName)
        {
            base.InitContainter(prefabName);
            m_rectTransform = GetComponent<RectTransform>();
            m_layoutElement = GetComponent<LayoutElement>();
            m_button = GetComponent<Button>();
            Assert.IsNotNull(m_rectTransform);
            Assert.IsNotNull(m_button);
            Assert.IsNotNull(m_layoutElement);
        }

        private void ScrollCellContent(object info)
        {
            WDataTable.RowElementInfo rei = (WDataTable.RowElementInfo) info;
            bindDataTable = rei.bindDataTable;
            IList<object> infos = bindDataTable.GetInfosByRowIndex(rei.rowIndex);

            if (!init)
                InitContainter(bindDataTable.rowElementPrefab);

            if (columnSize != infos.Count)
            {
                columnSize = infos.Count;
                BuildChild();
            }

            m_rectTransform.sizeDelta = new Vector2(bindDataTable.tableWidth, bindDataTable.itemHeight);
            m_layoutElement.preferredHeight = bindDataTable.itemHeight;

            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].SetSize(bindDataTable.GetWidthByColumnIndex(i), bindDataTable.itemHeight);
                elements[i].SetInfo(infos[i], rei.rowIndex, i, bindDataTable);
            }

            m_button.onClick.RemoveAllListeners();
            m_button.onClick.AddListener(() =>
            {
                bindDataTable.OnClickRow(rei.rowIndex);
                if (!bindDataTable.isUseSelect)
                    EventSystem.current.SetSelectedGameObject(null);
            });
        }
    }
}