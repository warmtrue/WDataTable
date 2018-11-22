using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace WDT
{
    public abstract class WContainter : MonoBehaviour
    {
        protected IList<WColumnDef> columnsDefs;
        protected readonly List<WElement> elements = new List<WElement>();
        protected WDataTable bindDataTable;
        protected bool init;
        private readonly List<string> m_initPoolNames = new List<string>();

        private GameObject GetObject(string prefabName)
        {
            if (!m_initPoolNames.Contains(prefabName))
            {
                SG.ResourceManager.Instance.InitPool(prefabName, 0);
                m_initPoolNames.Add(prefabName);
            }

            return SG.ResourceManager.Instance.GetObjectFromPool(prefabName);
        }

        protected virtual void InitContainter()
        {
            SG.ResourceManager.Instance.InitPool("ButtonElement", 0);
            SG.ResourceManager.Instance.InitPool("TextElement", 0);
            foreach (Transform child in transform)
                DestroyImmediate(child.gameObject);
            init = true;
        }

        protected void BuildChild()
        {
            if (columnsDefs == null)
                return;

            foreach (WElement element in elements)
                SG.ResourceManager.Instance.ReturnObjectToPool(element.gameObject);
            elements.Clear();

            for (int i = 0; i < columnsDefs.Count; i++)
            {
                GameObject go = GetObject(GetObjectName(i));
                go.transform.SetParent(transform, false);
                WElement element = go.GetComponent<WElement>();
                Assert.IsNotNull(element);
                elements.Add(element);
            }
        }

        protected abstract string GetObjectName(int columnIndex);
    }
}