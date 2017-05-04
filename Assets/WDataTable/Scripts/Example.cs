using System.Collections.Generic;
using UnityEngine;

namespace WDT
{
    public class Example : MonoBehaviour
    {
        public WDataTable testWDataTable;
        public bool isDynamic = false;
        private IList<string> _columns = new List<string>();
        private IList<IList<object>> _datas = new List<IList<object>>();


        // Use this for initialization
        void Start()
        {
            _columns.Add("ID");
            _columns.Add("A");
            _columns.Add("B");
            _columns.Add("C");
            _columns.Add("D");

            for (int i = 0; i < 6; i++)
            {
                List<object> tdatas = new List<object>();
                tdatas.Add(i + 1);
                tdatas.Add("dsada" + i.ToString());
                tdatas.Add(20.1 + i);
                tdatas.Add(Random.Range(0.0f, 1.0f));
                tdatas.Add(new Vector3(1, i, 2));
                _datas.Add(tdatas);
            }

            // init 
            testWDataTable.InitDataTable(_datas, _columns);
        }

        // Update is called once per frame
        void Update()
        {
            if (isDynamic)
            {
                testWDataTable.ColumnBg = Color.Lerp(Color.cyan, Color.magenta, Mathf.Sin(Time.time));
                testWDataTable.ItemWidth = (int)(Mathf.Sin(Time.time * 2) * 50) + 100;
                testWDataTable.ItemHeight = (int)(Mathf.Sin(Time.time * 3) * 20) + 50;
                testWDataTable.ColumnSequence = Color.Lerp(Color.red, Color.yellow, Mathf.Sin(3 * Time.time));
                testWDataTable.SortByIndex(Random.Range(0, 4));
            }
        }
    }
}