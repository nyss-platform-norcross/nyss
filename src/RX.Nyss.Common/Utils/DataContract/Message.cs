namespace RX.Nyss.Common.Utils.DataContract
{
    public class Message
    {
        public string Key { get; set; }
        public object Data { get; set; }

        public override string ToString() => $"{nameof(Key)}: {Key}, {nameof(Data)}: {Data}";
    }
}
