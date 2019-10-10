namespace RX.Nyss.Web.Features.DataContract
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string MessageKey { get; set; }
        public object MessageData { get; set; }

        private Result() { }

        private Result(bool isSuccess, string messageKey, object messageData)
        {
            IsSuccess = isSuccess;
            MessageKey = messageKey;
            MessageData = messageData;
        }

        public static Result Success(string messageKey, object messageData = null) => new Result(true, messageKey, messageData);
        public static Result Error(string messageKey, object messageData = null) => new Result(false, messageKey, messageData);

        public override string ToString() => $"{nameof(IsSuccess)}: {IsSuccess}, {nameof(MessageKey)}: {MessageKey}, {nameof(MessageData)}: {MessageData}";
    }
}
