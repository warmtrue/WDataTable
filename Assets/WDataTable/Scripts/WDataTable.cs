using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace WDT
{
    public enum WEventType
    {
        SELECT_ROW,
        SELECT_COLUMN,
        SORT_BY_COLUMN,
        INIT_ELEMENT,
        EVENT_COUNT,
    }

    public class WColumnDef
    {
        public string width;
        public bool disableSort;
    }

    public delegate void WMsgHandle(WEventType messageType, params object[] args);

    public class WDataTable : MonoBehaviour
    {
        public class RowElementInfo
        {
            public int rowIndex;
            public WDataTable bindDataTable;
        }

        private class SortItem
        {
            public SortItem(int indexIn, object itemIn)
            {
                rowIndex = indexIn;
                item = itemIn;
            }

            public readonly int rowIndex;
            public readonly object item;
        }

        [HideInInspector] public event WMsgHandle MsgHandle;

        public string rowPrefab;
        public string headElementPrefab;
        public string rowElementPrefab;
        public int itemHeight;
        public int tableWidth;
        public int tableHeight;
        public bool isUseSort = true;
        public bool isUseSelect = true;
        private bool m_init;
        private WHead m_head;
        private LoopVerticalScrollRect m_scrollRect;
        private RectTransform m_scrollRectTransform;
        private RectTransform m_scrollbarRectTransform;
        private IList<IList<object>> m_datas = new List<IList<object>>();
        private IList<string> m_columns = new List<string>();
        private IList<WColumnDef> m_columnDefs = new List<WColumnDef>();
        private readonly IList<RowElementInfo> m_rowInfos = new List<RowElementInfo>();
        private readonly IList<int> m_columnWidths = new List<int>();
        private readonly List<SortItem> m_sortItems = new List<SortItem>();

        #region public

        /// <summary>
        /// Sorts the index of the by.
        /// </summary>
        /// <param name="columnIndex">The index.</param>
        public void SortByIndex(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= m_columns.Count)
                return;

            if (m_rowInfos.Count == 0 || !(m_datas[0][columnIndex] is IComparable))
                return;

            m_sortItems.Clear();
            for (int i = 0; i < m_datas.Count; i++)
                m_sortItems.Add(new SortItem(i, m_datas[i][columnIndex]));

            m_sortItems.Sort((x, y) =>
            {
                var cpX = x.item as IComparable;
                var cpY = y.item as IComparable;
                if (cpX == null || cpY == null)
                    return 0;
                return cpX.CompareTo(cpY);
            });

            for (int i = 0; i < m_sortItems.Count; i++)
                m_rowInfos[i].rowIndex = m_sortItems[i].rowIndex;

            UpdateByRowInfo();

            if (MsgHandle != null)
                MsgHandle(WEventType.SORT_BY_COLUMN, columnIndex);
        }

        /// <summary>
        /// update data of the data table. need ensure right data
        /// </summary>
        /// <param name="datas">The datas.</param>
        /// <param name="columns">The columns.</param>
        public void UpdateData(IList<IList<object>> datas, IList<string> columns)
        {
            if (datas == null && columns == null)
                return;

            if (!m_init)
            {
                Debug.LogError("not init data table");
                return;
            }

            IList<IList<object>> tDatas = datas;
            IList<string> tColumns = columns;
            if (tDatas == null)
                tDatas = m_datas;

            if (tColumns == null)
                tColumns = m_columns;

            if (!CheckInputData(tDatas, tColumns, null))
                return;

            if (!CheckConfig())
                return;

            if (datas != null)
            {
                m_datas = datas;
                m_rowInfos.Clear();
                for (int i = 0; i < m_datas.Count; i++)
                    m_rowInfos.Add(new RowElementInfo {rowIndex = i, bindDataTable = this});
            }

            if (columns != null)
            {
                m_columns = new List<string>(columns);
                UpdateColumnWidths();
                m_head.SetColumnInfo(m_columns, this);
            }

            UpdateByRowInfo();
        }

        /// <summary>
        /// Initializes the data table. need ensure right data
        /// </summary>
        /// <param name="datas">The datas.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="columnDefs"></param>
        public void InitDataTable(IList<IList<object>> datas, IList<string> columns,
            IList<WColumnDef> columnDefs = null)
        {
            if (!CheckInputData(datas, columns, columnDefs))
                return;

            if (!CheckConfig())
                return;

            if (!m_init)
                Init();

            // copy
            m_datas = datas;
            m_columnDefs = columnDefs;
            m_rowInfos.Clear();
            for (int i = 0; i < m_datas.Count; i++)
                m_rowInfos.Add(new RowElementInfo {rowIndex = i, bindDataTable = this});
            m_columns = new List<string>(columns);

            UpdateColumnWidths();
            m_head.SetColumnInfo(m_columns, this);
            m_scrollRect.prefabSource.prefabName = rowPrefab;
            UpdateScrollRectSize();
            UpdateByRowInfo();
        }

        public IList<object> GetInfosByRowIndex(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= m_datas.Count)
                return null;

            return m_datas[rowIndex];
        }

        public void OnClickRow(int rowIndex)
        {
            Debug.Log("clicked rowIndex" + rowIndex);
            if (MsgHandle != null)
                MsgHandle(WEventType.SELECT_ROW, rowIndex);
        }

        public void OnClickColumn(int columnIndex)
        {
            if (isUseSort)
                SortByIndex(columnIndex);

            Debug.Log("clicked columnIndex " + columnIndex);
            if (MsgHandle != null)
                MsgHandle(WEventType.SELECT_COLUMN, columnIndex);
        }

        public void OnClickButton(int rowIndex, int columnIndex)
        {
            Debug.Log("clicked button row " + rowIndex + " columnIndex " + columnIndex);
            if (rowIndex == -1)
                OnClickColumn(columnIndex);
        }

        public void OnInitElement(int rowIndex, int columnIndex, WElement element)
        {
            if (MsgHandle != null)
                MsgHandle(WEventType.INIT_ELEMENT, rowIndex, columnIndex, element);
        }

        [ContextMenu("UpdateSize")]
        public void UpdateSize()
        {
            if (!m_init)
                return;

            UpdateColumnWidths();
            m_head.UpdateHeadSize();
            UpdateScrollRectSize();
            m_scrollRect.RefillCells();
        }

        public int GetWidthByColumnIndex(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= m_columnWidths.Count)
                return 0;
            return m_columnWidths[columnIndex];
        }

        public bool CanSortByColumnIndex(int columnIndex)
        {
            if (m_columnDefs == null)
                return true;

            if (columnIndex < 0 || columnIndex >= m_columns.Count)
                return false;

            if (m_columnDefs[columnIndex] == null)
                return true;

            return !m_columnDefs[columnIndex].disableSort;
        }

        #endregion

        private void Init()
        {
            m_scrollRect = GetComponentInChildren<LoopVerticalScrollRect>();
            m_head = GetComponentInChildren<WHead>();
            Assert.IsNotNull(m_scrollRect);
            Assert.IsNotNull(m_head);
            m_scrollRectTransform = m_scrollRect.GetComponent<RectTransform>();
            m_scrollbarRectTransform = m_scrollRect.verticalScrollbar.GetComponent<RectTransform>();
            Assert.IsNotNull(m_scrollRectTransform);
            Assert.IsNotNull(m_scrollbarRectTransform);
            m_init = true;
        }

        private void UpdateColumnWidths()
        {
            m_columnWidths.Clear();
            if (m_columnDefs == null || m_columnDefs.Count == 0)
            {
                for (int i = 0; i < m_columns.Count; i++)
                {
                    m_columnWidths.Add(tableWidth / m_columns.Count);
                }
            }
            else
            {
                int totalWidth = 0;
                int totalCount = 0;
                for (int i = 0; i < m_columns.Count; i++)
                {
                    int width = 0;
                    if (m_columnDefs[i] != null)
                    {
                        if (m_columnDefs[i].width.Contains('%'))
                        {
                            string percentString = m_columnDefs[i].width.Replace("%", "");
                            int percent;
                            int.TryParse(percentString, out percent);
                            width = (int) (tableWidth * (percent / 100.0f));
                        }
                        else
                        {
                            int.TryParse(m_columnDefs[i].width, out width);
                        }
                    }

                    m_columnWidths.Add(width);
                    if (width > 0)
                    {
                        totalCount += 1;
                        totalWidth += width;
                    }
                }

                if (totalCount < m_columns.Count)
                {
                    int otherWidth = (tableWidth - totalWidth) / (m_columns.Count - totalCount);
                    if (otherWidth <= 0)
                        Debug.LogError("Error columnDef for calculate column width");

                    for (int i = 0; i < m_columnWidths.Count; i++)
                    {
                        if (m_columnWidths[i] == 0)
                            m_columnWidths[i] = otherWidth;
                    }
                }
            }
        }

        private void UpdateScrollRectSize()
        {
            m_scrollRectTransform.sizeDelta = new Vector2(tableWidth, tableHeight);
            m_scrollbarRectTransform.anchoredPosition = new Vector2(tableWidth, 0);
            m_scrollbarRectTransform.sizeDelta = new Vector2(20, tableHeight);
        }

        private void UpdateByRowInfo()
        {
            m_scrollRect.objectsToFill = m_rowInfos.ToArray();
            m_scrollRect.totalCount = m_rowInfos.Count;
            m_scrollRect.RefillCells();
        }

        private bool CheckConfig()
        {
            if (string.IsNullOrEmpty(rowPrefab))
            {
                Debug.LogError("need set rowPrefab name");
                return false;
            }

            if (string.IsNullOrEmpty(headElementPrefab))
            {
                Debug.LogError("need set headElementPrefab name");
                return false;
            }

            if (string.IsNullOrEmpty(rowElementPrefab))
            {
                Debug.LogError("need set rowElementPrefab name");
                return false;
            }

            if (itemHeight <= 0 || tableWidth <= 0 || tableHeight <= 0)
            {
                Debug.LogError("size number greater than zero");
                return false;
            }

            if (itemHeight > tableHeight)
            {
                Debug.LogError("need itemHeight < tableHeight");
                return false;
            }

            return true;
        }

        private static bool CheckInputData(IList<IList<object>> datas, ICollection<string> columns,
            ICollection<WColumnDef> columnDefs)
        {
            if (columns.Count == 0 || datas.Count == 0)
            {
                Debug.LogError("empty data");
                return false;
            }

            if (columnDefs != null)
            {
                if (columnDefs.Count != columns.Count)
                {
                    Debug.LogError("columnDefs count not equal columns.Count");
                    return false;
                }
            }

            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i].Count != columns.Count)
                {
                    Debug.LogError("row data length not equal columns length:" + i);
                    return false;
                }
            }

            for (int i = 0; i < columns.Count; i++)
            {
                for (int j = 0; j < datas.Count - 1; j++)
                {
                    if ((datas[j][i] == null) || (datas[j + 1][i] == null))
                    {
                        if ((datas[j][i] == null) && (datas[j + 1][i] == null))
                            continue;

                        Debug.LogError("data type not same:[" + j + "," + i + "], [" + (j + 1) + "," + i + "]");
                        return false;
                    }

                    if (datas[j][i].GetType() == datas[j + 1][i].GetType())
                        continue;

                    Debug.LogError("data type not same:[" + j + "," + i + "], [" + (j + 1) + "," + i + "]");
                    return false;
                }
            }

            return true;
        }
    }
}