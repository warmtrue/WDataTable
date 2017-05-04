using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


namespace WDT
{
    public class WDataTable : MonoBehaviour
    {
        private class SortItem
        {
            public SortItem(int indexIn, object itemIn)
            {
                index = indexIn;
                item = itemIn;
            }

            public int index;
            public object item;
        }

        // config
        private bool _isSort = true;
        private int _itemWidth = 100;
        private int _itemHeight = 50;
        private int _tableHeight = 200;
        private Font _useFont = null;
        private Color _columnBg = Color.gray;
        private Color _columnSequence = Color.green;
        private Color _columnReverse = Color.red;
        private ColorBlock _defaultColorBlock = new ColorBlock();

        // backup data
        private IList<IList<object>> _datas = new List<IList<object>>();
        private IList<string> _columns = new List<string>();

        // temp record list
        private List<GameObject> _rowObjectList = new List<GameObject>();
        private List<GameObject> _columnObjectList = new List<GameObject>();
        private List<Image> _columnBgList = new List<Image>();
        private List<Text> _textList = new List<Text>();
        private List<LayoutElement> _layoutList = new List<LayoutElement>();
        private List<SortItem> _sortItems = new List<SortItem>();

        // state
        private int _currentSortIndex = -1;
        private bool _isSortSequence = true;
        private GameObject _contentObject = null;
        private GameObject _scrollViewObject = null;
        private bool _isBuildUI = false;

        private const int SCROLL_BAR_WIDTH = 30;

        // dynamic setting
        public bool Sort
        {
            get { return _isSort; }
            set { _isSort = value; }
        }

        public int ItemWidth
        {
            get { return _itemWidth; }
            set
            {
                if (_itemWidth == value)
                    return;

                _itemWidth = value;
                if (_isBuildUI)
                    UpdateLayoutSize();
            }
        }

        public int ItemHeight
        {
            get { return _itemHeight; }
            set
            {
                if (_itemHeight == value)
                    return;

                _itemHeight = value;
                if (_isBuildUI)
                    UpdateLayoutSize();
            }
        }

        public int TableHeight
        {
            get { return _tableHeight; }
            set
            {
                if (_tableHeight == value)
                    return;

                _tableHeight = value;
                if (_isBuildUI)
                    UpdateLayoutSize();
            }
        }

        public Font UseFont
        {
            get { return _useFont; }
            set
            {
                _useFont = value;
                if (_isBuildUI)
                    UpdateTextFont();
            }
        }

        public Color ColumnBg
        {
            get { return _columnBg; }
            set
            {
                _columnBg = value;
                if (_isBuildUI)
                    UpdateColumnImage();
            }
        }

        public Color ColumnSequence
        {
            get { return _columnSequence; }
            set
            {
                _columnSequence = value;
                if (_isBuildUI)
                    UpdateColumnImage();
            }
        }

        public Color ColumnReverse
        {
            get { return _columnReverse; }
            set
            {
                _columnReverse = value;
                if (_isBuildUI)
                    UpdateColumnImage();
            }
        }

        // init data
        // need ensure right data
        public void InitDataTable(IList<IList<object>> datas, IList<string> columns)
        {
            if (!CheckInputData(datas, columns))
                return;

            _isBuildUI = true;

            // copy
            _datas = new List<IList<object>>(datas);
            _columns = new List<string>(columns);
            _rowObjectList.Clear();
            _columnObjectList.Clear();
            _columnBgList.Clear();
            _textList.Clear();
            _layoutList.Clear();
            _sortItems.Clear();

            for (var i = 0; i < datas.Count; i++)
                _sortItems.Add(new SortItem(0, null));

            _currentSortIndex = -1;
            _isSortSequence = true;

            UpdatePrimarySize();

            var columnRowObject = new GameObject("columnRow");
            columnRowObject.AddComponent<HorizontalLayoutGroup>();
            columnRowObject.transform.SetParent(_contentObject.transform);
            ConfigUIObjectSize(columnRowObject, columns.Count*_itemWidth, _itemHeight);

            for (int i = 0; i < columns.Count; i++)
            {
                var columnObject = new GameObject("column" + i);
                columnObject.transform.SetParent(columnRowObject.transform);
                ConfigLayoutItem(columnObject);
                ConfigColumnBgObject(columnObject);
                ConfigTextObject(columnObject, columns[i]);
                _columnObjectList.Add(columnObject);
            }

            for (int i = 0; i < datas.Count; i++)
            {
                var rowObject = new GameObject(datas[i][0].ToString());
                rowObject.AddComponent<HorizontalLayoutGroup>();
                ConfigUIObjectSize(rowObject, columns.Count*_itemWidth, _itemHeight);
                rowObject.transform.SetParent(_contentObject.transform);

                for (int j = 0; j < datas[i].Count; j++)
                {
                    var item = new GameObject("item" + (i + 1) + "_" + (j + 1));
                    item.transform.parent = rowObject.transform;
                    ConfigLayoutItem(item);
                    ConfigTextObject(item, datas[i][j].ToString());
                }
                _rowObjectList.Add(rowObject);
            }
        }

        // sort by custom column index
        public void SortByIndex(int index)
        {
            if (index < 0 || index >= _columns.Count)
                return;

            if (_datas.Count == 0 || !(_datas[0][index] is IComparable))
                return;

            if (_currentSortIndex == index)
                _isSortSequence = !_isSortSequence;
            else
            {
                _currentSortIndex = index;
                _isSortSequence = true;
            }

            UpdateColumnImage();

            for (var i = 0; i < _datas.Count; i++)
            {
                _sortItems[i].index = i;
                _sortItems[i].item = _datas[i][index];
            }

            _sortItems.Sort((x, y) =>
            {
                var cpX = x.item as IComparable;
                var cpY = y.item as IComparable;
                if (cpX == null || cpY == null)
                    return 0;
                if (_isSortSequence)
                    return cpX.CompareTo(cpY);
                else
                    return -cpX.CompareTo(cpY);
            });

            for (var i = 0; i < _sortItems.Count; i++)
                _rowObjectList[_sortItems[i].index].transform.SetSiblingIndex(i + 1);
        }

        private void Awake()
        {
            var tContCom = GetComponentInChildren<VerticalLayoutGroup>(true);
            Assert.IsNotNull(tContCom);
            _contentObject = tContCom.gameObject;

            var tSclCom = GetComponentInChildren<ScrollRect>(true);
            if (tSclCom != null)
                _scrollViewObject = tSclCom.gameObject;
            else
                _scrollViewObject = gameObject;

            // for default color block
            _defaultColorBlock.highlightedColor = Color.white;
            _defaultColorBlock.pressedColor = Color.yellow;
            _defaultColorBlock.colorMultiplier = 1;
            _defaultColorBlock.normalColor = Color.white;
            _defaultColorBlock.fadeDuration = 0.2f;

            // default arial
            _useFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        }

        private bool CheckInputData(IList<IList<object>> datas, IList<string> columns)
        {
            if (columns.Count == 0 || datas.Count == 0)
            {
                Debug.LogError("empty data");
                return false;
            }

            for (var i = 0; i < datas.Count; i++)
            {
                if (datas[i].Count != columns.Count)
                {
                    Debug.LogError("row data lenght not equal columns lenght:" + i);
                    return false;
                }
            }

            for (var i = 0; i < columns.Count; i++)
            {
                for (var j = 0; j < datas.Count - 1; j++)
                {
                    if (datas[j][i].GetType() == datas[j + 1][i].GetType())
                        continue;
                    Debug.LogError("data type not same:[" + j + "," + i + "], [" + (j + 1) + "," + i + "]");
                    return false;
                }
            }
            return true;
        }

        private void ConfigUIObjectSize(GameObject uiObject, int width, int height)
        {
            var rTrans = uiObject.GetComponent<RectTransform>();
            rTrans.sizeDelta = new Vector2(width, height);
        }


        private void ConfigLayoutItem(GameObject layoutObject)
        {
            var layoutCom = layoutObject.AddComponent<LayoutElement>();
            layoutCom.minWidth = _itemWidth;
            layoutCom.minHeight = _itemHeight;

            _layoutList.Add(layoutCom);
        }

        private void ConfigTextObject(GameObject parentObject, string text)
        {
            var textObject = new GameObject("text", typeof(RectTransform));
            textObject.transform.SetParent(parentObject.transform);

            var rTrans = textObject.GetComponent<RectTransform>();
            rTrans.anchoredPosition = Vector2.zero;
            rTrans.sizeDelta = Vector2.zero;

            rTrans.anchorMin = Vector2.zero;
            rTrans.anchorMax = Vector2.one;
            rTrans.pivot = new Vector2(0.5f, 0.5f);

            var textCom = textObject.AddComponent<Text>();
            textCom.text = text;
            textCom.alignment = TextAnchor.MiddleCenter;
            textCom.font = _useFont;
            textCom.raycastTarget = false;

            _textList.Add(textCom);
        }

        private void ConfigColumnBgObject(GameObject parentObject)
        {
            var bgObject = new GameObject("bg", typeof(RectTransform));
            bgObject.transform.SetParent(parentObject.transform);

            var rTrans = bgObject.GetComponent<RectTransform>();
            rTrans.anchoredPosition = Vector2.zero;
            rTrans.sizeDelta = new Vector2(-6, -6);

            rTrans.anchorMin = Vector2.zero;
            rTrans.anchorMax = Vector2.one;
            rTrans.pivot = new Vector2(0.5f, 0.5f);

            var bgCom = bgObject.AddComponent<Image>();
            bgCom.color = _columnBg;
            bgCom.raycastTarget = true;

            var index = -1;
            int.TryParse(parentObject.name.Substring(6), out index);

            var btnCom = bgObject.AddComponent<Button>();
            btnCom.colors = _defaultColorBlock;
            btnCom.onClick.AddListener(() => { this.OnClickColumn(index); });

            _columnBgList.Add(bgCom);
        }

        // for dynamic size update
        private void UpdateLayoutSize()
        {
            UpdatePrimarySize();
            for (var i = 0; i < _layoutList.Count; i++)
            {
                _layoutList[i].minWidth = _itemWidth;
                _layoutList[i].minHeight = _itemHeight;
            }
        }

        private void UpdatePrimarySize()
        {
            if (_scrollViewObject.gameObject == gameObject)
                ConfigUIObjectSize(_scrollViewObject, _columns.Count * _itemWidth, (_datas.Count + 1) * _itemHeight);
            else
                ConfigUIObjectSize(_scrollViewObject, _columns.Count * _itemWidth + SCROLL_BAR_WIDTH, _tableHeight);
            ConfigUIObjectSize(_contentObject, _columns.Count * _itemWidth, _datas.Count * _itemHeight);
        }

        private void UpdateTextFont()
        {
            for (var i = 0; i < _textList.Count; i++)
            {
                _textList[i].font = _useFont;
            }
        }

        private void UpdateColumnImage()
        {
            for (var i = 0; i < _columnBgList.Count; i++)
            {
                _columnBgList[i].color = _columnBg;
                if (i == _currentSortIndex)
                    _columnBgList[i].color = _isSortSequence ? _columnSequence : _columnReverse;
            }
        }

        private void OnClickColumn(int index)
        {
            // Debug.Log("You have clicked the button!" + index);
            if (_isSort)
            {
                SortByIndex(index);
            }
        }
    }
}