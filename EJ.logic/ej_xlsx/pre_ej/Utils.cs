namespace EJ.logic.ej_xlsx.pre_ej
{
    enum OperatorType { Atm, Terminal }
    enum OperationType { First, Operator, NDC, Client, Balance, Other }
    class KeyValueInfo
    {
        public int Key { get; set; }
        public int Value { get; set; }
        public KeyValueInfo(int k, int v)
        {
            Key = k;
            Value = v;
        }
    }

    class SummaryInformation
    {
        public string Key { get; set; }
        public KeyValueInfo Inform { get; set; }
        public SummaryInformation(string k, KeyValueInfo kv)
        {
            Key = k;
            Inform = kv;
        }
    }
}
