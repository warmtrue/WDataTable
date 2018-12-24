using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
#if WDT_USE_TMPRO
using TMPro;
#endif

namespace WDT
{
    public class WText : WElement
    {
        private Text m_text;
#if WDT_USE_TMPRO
        private TextMeshProUGUI m_tmpText;
#endif
        private RectTransform m_rectTransform;

        protected override void InitElement()
        {
            base.InitElement();
            m_text = GetComponentInChildren<Text>();
#if WDT_USE_TMPRO
            m_tmpText = GetComponentInChildren<TextMeshProUGUI>();
#endif
            m_rectTransform = GetComponent<RectTransform>();
            Assert.IsNotNull(m_rectTransform);
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
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            m_rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}