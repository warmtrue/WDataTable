using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace WDT
{
    public class WButton : WElement
    {
        private Button m_button;
        private Text m_text;
        private LayoutElement m_layoutElement;
        private RectTransform m_rectTransform;

        protected override void InitElement()
        {
            base.InitElement();
            m_button = GetComponent<Button>();
            m_text = GetComponentInChildren<Text>();
            m_layoutElement = GetComponent<LayoutElement>();
            m_rectTransform = GetComponent<RectTransform>();
            Assert.IsNotNull(m_button);
            Assert.IsNotNull(m_text);
            Assert.IsNotNull(m_layoutElement);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            m_rectTransform.sizeDelta = new Vector2(width, height);
        }

        public override void SetInfo(object info, int rowIndex, int columnIndex, WDataTable dataTable)
        {
            base.SetInfo(info, rowIndex, columnIndex, dataTable);
            m_text.text = info.ToString();
            m_button.onClick.RemoveAllListeners();
            if (bindDataTable.CanSortByColumnIndex(columnIndex))
                m_button.onClick.AddListener(() => { bindDataTable.OnClickButton(rowIndex, columnIndex); });
        }
    }
}