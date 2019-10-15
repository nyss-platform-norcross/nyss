namespace RX.Nyss.Web.Utils.DataContract
{
    public class Result
    {
        public bool IsSuccess { get; set; }

        public Message Message { get; set; }
        private Result() { }

        private Result(bool isSuccess, string messageKey, object messageData)
        {
            IsSuccess = isSuccess;
            Message = new Message() { Key = messageKey, Data = messageData};
        }

        public static Result Success(string messageKey, object messageData = null) => new Result(true, messageKey, messageData);
        public static Result Error(string messageKey, object messageData = null) => new Result(false, messageKey, messageData);

        public override string ToString() => $"{nameof(IsSuccess)}: {IsSuccess}, {nameof(Message)}: {Message}";
    }
}
