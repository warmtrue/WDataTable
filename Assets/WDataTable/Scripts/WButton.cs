using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
#if WDT_USE_TMPRO
using TMPro;

#endif

namespace WDT
{
    public class WButton : WElement
    {
        private Button m_button;
        private Text m_text;
#if WDT_USE_TMPRO
        private TextMeshProUGUI m_tmpText;
#endif
        private LayoutElement m_layoutElement;
        private RectTransform m_rectTransform;

        protected override void InitElement()
        {
            base.InitElement();
            m_button = GetComponent<Button>();
            m_text = GetComponentInChildren<Text>();
#if WDT_USE_TMPRO
            m_tmpText = GetComponentInChildren<TextMeshProUGUI>();
#endif
            m_layoutElement = GetComponent<LayoutElement>();
            m_rectTransform = GetComponent<RectTransform>();
            Assert.IsNotNull(m_button);
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
            if (m_text != null)
                m_text.text = info.ToString();
#if WDT_USE_TMPRO
            if (m_tmpText != null)
                m_tmpText.text = info.ToString();
#endif
            m_button.onClick.RemoveAllListeners();
            if (bindDataTable.CanSortByColumnIndex(columnIndex))
                m_button.onClick.AddListener(() => { bindDataTable.OnClickButton(rowIndex, columnIndex); });
        }
    }
}