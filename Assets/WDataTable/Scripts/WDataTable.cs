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
        private int _itemWidth = 100;
        private int _itemHeight = 50;
        private int _scrollHeight = 200;
        private Font _useFont = null;

        private Color _columnBgColor = Color.gray;
        private Color _columnSequenceColor = Color.green;
        private Color _columnReverseColor = Color.red;
        private ColorBlock _columnColorBlock = new ColorBlock();

        private Color _rowSelectColor = Color.blue;
        private ColorBlock _rowColorBlock = new ColorBlock();

        // backup data
        private IList<IList<object>> _datas = new List<IList<object>>();
        private IList<string> _columns = new List<string>();

        // temp record list
        private List<GameObject> _rowObjectList = new List<GameObject>();
        private List<GameObject> _columnObjectList = new List<GameObject>();
        private GameObject _columnRowObject = null;
        private List<Image> _rowBgList = new List<Image>();
        private List<Image> _columnBgList = new List<Image>();
        private List<Text> _textList = new List<Text>();
        private List<LayoutElement> _layoutList = new List<LayoutElement>();
        private List<SortItem> _sortItems = new List<SortItem>();

        // state
        private bool _isSelect = true;
        private bool _isRadioSelect = false;
        private List<int> _selectIndexList = new List<int>();

        private bool _isSort = true;
        private int _currentSortIndex = -1;
        private bool _isSortSequence = true;

        private GameObject _contentObject = null;
        private GameObject _scrollViewObject = null;
        private bool _isBuildedUi = false;
        private Navigation _noneNav = new Navigation();

        private const int SCROLL_BAR_WIDTH = 30;

        public IList<int> GetSelectResult()
        {
            return _selectIndexList;
        }

        // dynamic setting
        public bool Sort
        {
            get { return _isSort; }
            set
            {
                if (_isSort == value) return;
                _isSort = value;
                RevertSort();
            }
        }

        public bool Select
        {
            get { return _isSelect; }
            set
            {
                if (_isSelect == value) return;
                _isSelect = value;
                RevertSelect();
            }
        }

        public bool RadioSelect
        {
            get { return _isRadioSelect; }
            set
            {
                if (_isRadioSelect == value) return;
                _isRadioSelect = value;
                RevertSelect();
            }
        }

        public Font UseFont
        {
            get { return _useFont; }
            set
            {
                _useFont = value;
                if (_isBuildedUi)
                    UpdateTextFont();
            }
        }

        // -1 for not change
        public void ConfigSize(int itemWidth, int itemHeight, int scrollHeight)
        {
            if (itemWidth != -1)
                _itemWidth = itemWidth;
            if (itemHeight != -1)
                _itemHeight = itemHeight;
            if (scrollHeight != -1)
                _scrollHeight = scrollHeight;
            if (_isBuildedUi)
                UpdateLayoutSize();
        }

        public void ConfigColomnColor(Color columnBg, Color columnSequence, Color columnReverse)
        {
            _columnBgColor = columnBg;
            _columnSequenceColor = columnSequence;
            _columnReverseColor = columnReverse;
            if (_isBuildedUi)
                UpdateColumnImage();
        }

        // init data
        // need ensure right data
        public void InitDataTable(IList<IList<object>> datas, IList<string> columns)
        {
            if (!CheckInputData(datas, columns))
                return;

            _isBuildedUi = true;

            // copy
            _datas = new List<IList<object>>(datas);
            _columns = new List<string>(columns);
            _rowObjectList.Clear();
            _columnRowObject = null;
            _columnObjectList.Clear();
            _rowBgList.Clear();
            _columnBgList.Clear();
            _textList.Clear();
            _layoutList.Clear();
            _sortItems.Clear();

            for (var i = 0; i < datas.Count; i++)
                _sortItems.Add(new SortItem(0, null));

            _currentSortIndex = -1;
            _isSortSequence = true;

            var columnRowObject = new GameObject("columnRow");
            columnRowObject.AddComponent<HorizontalLayoutGroup>();
            columnRowObject.transform.SetParent(_contentObject.transform);
            _columnRowObject = columnRowObject;

            for (int i = 0; i < columns.Count; i++)
            {
                var columnObject = new GameObject("column" + i);
                columnObject.transform.SetParent(columnRowObject.transform);
                ConfigLayoutItem(columnObject);
                ConfigColumnBgObject(columnObject);
                ConfigTextObject(columnObject, columns[i]);
                _columnObjectList.Add(columnObject);
            }

            for (var i = 0; i < datas.Count; i++)
            {
                var rowObject = new GameObject(datas[i][0].ToString());
                rowObject.AddComponent<HorizontalLayoutGroup>();
                rowObject.transform.SetParent(_contentObject.transform);

                var rowBgCom = rowObject.AddComponent<Image>();
                rowBgCom.color = Color.gray;
                rowBgCom.raycastTarget = true;
                _rowBgList.Add(rowBgCom);

                var rowBtnCom = rowObject.AddComponent<Button>();
                rowBtnCom.colors = _rowColorBlock;
                rowBtnCom.navigation = _noneNav;
                var index = i;
                rowBtnCom.onClick.AddListener(() => { this.OnClickRow(index); });

                for (var j = 0; j < datas[i].Count; j++)
                {
                    var item = new GameObject("item" + (i + 1) + "_" + (j + 1));
                    item.transform.parent = rowObject.transform;
                    ConfigLayoutItem(item);
                    ConfigTextObject(item, datas[i][j].ToString());
                }
                _rowObjectList.Add(rowObject);
            }

            UpdateLayoutSize();
        }

        // turn back select state
        public void RevertSelect()
        {
            _selectIndexList.Clear();
            UpdateRowImage();
        }

        // turn back sort state
        public void RevertSort()
        {
            _currentSortIndex = -1;
            _isSortSequence = true;
            UpdateColumnImage();
            for (var i = 0; i < _rowObjectList.Count; i++)
                _rowObjectList[i].transform.SetSiblingIndex(i + 1);
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

        // selec by row index
        public void SelectByIndex(int index)
        {
            if (index < 0 || index >= _datas.Count || _datas.Count == 0)
                return;

            var idx = _selectIndexList.IndexOf(index);
            if (idx < 0)
            {
                if (_isRadioSelect)
                    _selectIndexList.Clear();
                _selectIndexList.Add(index);
            }
            else
                _selectIndexList.RemoveAt(idx);

            UpdateRowImage();
        }

        private void Awake()
        {
            var tContCom = GetComponentInChildren<VerticalLayoutGroup>(true);
            Assert.IsNotNull(tContCom);
            _contentObject = tContCom.gameObject;

            var tSclCom = GetComponentInChildren<ScrollRect>(true);
            _scrollViewObject = tSclCom != null ? tSclCom.gameObject : gameObject;

            // for default color block
            _columnColorBlock.highlightedColor = Color.green;
            _columnColorBlock.normalColor = Color.white;
            _columnColorBlock.pressedColor = Color.white;
            _columnColorBlock.colorMultiplier = 1;
            _columnColorBlock.fadeDuration = 0.2f;

            _rowColorBlock.highlightedColor = Color.green;
            _rowColorBlock.normalColor = Color.white;
            _rowColorBlock.pressedColor = Color.white;
            _rowColorBlock.colorMultiplier = 1;
            _rowColorBlock.fadeDuration = 0.2f;

            // default arial
            _useFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            _noneNav.mode = Navigation.Mode.None;
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
            bgCom.color = _columnBgColor;
            bgCom.raycastTarget = true;

            var index = -1;
            int.TryParse(parentObject.name.Substring(6), out index);

            var btnCom = bgObject.AddComponent<Button>();
            btnCom.colors = _columnColorBlock;
            btnCom.navigation = _noneNav;
            btnCom.onClick.AddListener(() => { this.OnClickColumn(index); });

            _columnBgList.Add(bgCom);
        }

        // for dynamic size update
        private void UpdateLayoutSize()
        {
            for (var i = 0; i < _layoutList.Count; i++)
            {
                _layoutList[i].minWidth = _itemWidth;
                _layoutList[i].minHeight = _itemHeight;
            }

            ConfigUIObjectSize(_columnRowObject, _columns.Count*_itemWidth, _itemHeight);
            for (var i = 0; i < _rowObjectList.Count; i++)
                ConfigUIObjectSize(_rowObjectList[i], _columns.Count*_itemWidth, _itemHeight);

            if (_scrollViewObject.gameObject == gameObject)
                ConfigUIObjectSize(_scrollViewObject, _columns.Count*_itemWidth, (_datas.Count + 1)*_itemHeight);
            else
                ConfigUIObjectSize(_scrollViewObject, _columns.Count*_itemWidth + SCROLL_BAR_WIDTH, _scrollHeight);
            ConfigUIObjectSize(_contentObject, _columns.Count*_itemWidth, _datas.Count*_itemHeight);
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
                _columnBgList[i].color = _columnBgColor;
                if (i == _currentSortIndex)
                    _columnBgList[i].color = _isSortSequence ? _columnSequenceColor : _columnReverseColor;
            }
        }

        private void UpdateRowImage()
        {
            for (var i = 0; i < _rowBgList.Count; i++)
            {
                _rowBgList[i].color = _selectIndexList.IndexOf(i) >= 0 ? _rowSelectColor : Color.gray;
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

        private void OnClickRow(int index)
        {
            Debug.Log("You have clicked the row!" + index);
            if (_isSelect)
            {
                SelectByIndex(index);
            }
        }
    }
}