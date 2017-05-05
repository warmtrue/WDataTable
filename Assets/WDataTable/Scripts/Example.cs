using System.Collections.Generic;
using UnityEngine;

namespace WDT
{
    public class Example : MonoBehaviour
    {
        public WDataTable testWDataTable;
        public bool isDynamic = false;
        public bool useSort = true;
        public bool useSelect = true;
        public bool isRadioSelect = true;
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

            for (var i = 0; i < 6; i++)
            {
                var tdatas = new List<object>
                {
                    i + 1,
                    "dsada" + i.ToString(),
                    20.1 + i,
                    Random.Range(0.0f, 1.0f),
                    new Vector3(1, i, 2)
                };
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
                testWDataTable.ConfigColomnColor(Color.Lerp(Color.cyan, Color.magenta, Mathf.Sin(Time.time)),
                    Color.Lerp(Color.red, Color.yellow, Mathf.Sin(3*Time.time)), Color.red);
                testWDataTable.ConfigSize((int) (Mathf.Sin(Time.time*2)*100) + 100,
                    (int) (Mathf.Sin(Time.time*3)*50) + 50, -1);
                testWDataTable.SortByIndex(Random.Range(0, 4));
            }

            testWDataTable.useSort = useSort;
            testWDataTable.useSelect = useSelect;
            testWDataTable.isRadioSelect = isRadioSelect;
        }
    }
}