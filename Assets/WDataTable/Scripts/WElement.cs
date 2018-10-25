using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace WDT
{
    public abstract class WElement : MonoBehaviour
    {
        protected WDataTable bindDataTable;
        private bool m_init;

        protected virtual void InitElement()
        {
            m_init = true;
        }

        public virtual void SetInfo(object info, int rowIndex, int columnIndex, WDataTable dataTable)
        {
            if (!m_init)
                InitElement();

            bindDataTable = dataTable;
            bindDataTable.OnInitElement(rowIndex, columnIndex, this);
        }

        public virtual void SetSize(int width, int height)
        {
            if (!m_init)
                InitElement();
        }
    }
}