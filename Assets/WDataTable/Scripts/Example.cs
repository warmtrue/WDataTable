using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WDT
{
    public class Example : MonoBehaviour
    {
        [System.Serializable]
        public class TestInfo
        {
            public int ID;
            public string A;
            public double B;
            public float C;
            public Vector3 D;
        }

        [System.Serializable]
        public class TestInfoList
        {
            public TestInfo[] data;
        }

        public enum LoadDataType
        {
            ListArrayType,
            DictArrayType,
            JsonType,
        }

        public WDataTable testWDataTable;
        public bool isDynamic = false;
        public bool useSort = true;
        public bool useSelect = true;
        public bool isRadioSelect = true;

        public LoadDataType loadType = LoadDataType.ListArrayType;

        private IList<string> _columns = new List<string>();
        private IList<IList<object>> _datas = new List<IList<object>>();
        private IList<IDictionary<string, object>> _datasDict = new List<IDictionary<string, object>>();

        // Use this for initialization
        void Start()
        {
            _columns.Add("ID");
            _columns.Add("A");
            _columns.Add("B");
            _columns.Add("C");
            _columns.Add("D");

            // init 
            switch (loadType)
            {
                case LoadDataType.ListArrayType:
                case LoadDataType.DictArrayType:
                    for (int i = 0; i < 6; i++)
                    {
                        var tdatas = new List<object>
                        {
                            i + 1,
                            "dsada" + i.ToString(),
                            20.1 + i,
                            Random.Range(0.0f, 1.0f),
                            new Vector3(1, i, 2)
                        };

                        var tdatasDict = new Dictionary<string, object>
                        {
                            {"ID", i + 1},
                            {"A", "dsada" + i.ToString()},
                            {"B", 20.1 + i},
                            {"C", Random.Range(0.0f, 1.0f)},
                            {"D", new Vector3(1, i, 2)}
                        };

                        _datas.Add(tdatas);
                        _datasDict.Add(tdatasDict);
                    }
                    if (loadType == LoadDataType.ListArrayType)
                        testWDataTable.InitDataTable(_datas, _columns);
                    else
                        testWDataTable.InitDataTable(_datasDict, _columns);
                    break;
                case LoadDataType.JsonType:
                    var textAsset = Resources.Load("TestJson") as TextAsset;
                    if (textAsset == null)
                        Debug.LogError("Not found TestJson.json in Resources Directory");
                    else
                    {
                        var testInfoList = JsonUtility.FromJson<TestInfoList>(textAsset.text);
                        for (int i = 0; i < testInfoList.data.Length; i++)
                        {
                            TestInfo ti = testInfoList.data[i];
                            var tdatas = new List<object>
                            {
                                ti.ID,
                                ti.A,
                                ti.B,
                                ti.C,
                                ti.D,
                            };
                            _datas.Add(tdatas);
                        }
                        testWDataTable.InitDataTable(_datas, _columns);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isDynamic)
            {
                testWDataTable.ConfigColomnColor(Color.Lerp(Color.cyan, Color.magenta, Mathf.Sin(Time.time)),
                    Color.Lerp(Color.red, Color.yellow, Mathf.Sin(3*Time.time)), Color.red);
                testWDataTable.ConfigSize((int) (Mathf.Sin(Time.time*2)*50) + 100,
                    (int) (Mathf.Sin(Time.time*3)*20) + 50, -1);
                testWDataTable.SortByIndex(Random.Range(0, 4));

                testWDataTable.ConfigSelectColor(Color.Lerp(Color.cyan, Color.magenta, Mathf.Sin(2*Time.time)));
            }

            testWDataTable.useSort = useSort;
            testWDataTable.useSelect = useSelect;
            testWDataTable.isRadioSelect = isRadioSelect;
        }
    }
}