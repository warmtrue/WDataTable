using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace WDT
{
    public abstract class WContainter : MonoBehaviour
    {
        protected int columnSize = -1;
        protected readonly List<WElement> elements = new List<WElement>();
        protected WDataTable bindDataTable;
        protected bool init;
        private string m_prefabName;

        protected virtual void InitContainter(string prefabName)
        {
            m_prefabName = prefabName;
            //SG.ResourceManager.Instance.InitPool(m_prefabName, 0);
            SG.ResourceManager.Instance.InitPool("ButtonElement", 0);
            SG.ResourceManager.Instance.InitPool("TextElement", 0);
            foreach (Transform child in transform)
                DestroyImmediate(child.gameObject);
            init = true;
        }

        protected void BuildChild()
        {
            foreach (WElement element in elements)
                SG.ResourceManager.Instance.ReturnObjectToPool(element.gameObject);
            elements.Clear();

            for (int i = 0; i < columnSize; i++)
            {
                GameObject go = SG.ResourceManager.Instance.GetObjectFromPool(GetElemType(i));
                go.transform.SetParent(transform, false);
                WElement element = go.GetComponent<WElement>();
                Assert.IsNotNull(element);
                elements.Add(element);
            }
        }

        internal abstract string GetElemType(int i);
    }
}