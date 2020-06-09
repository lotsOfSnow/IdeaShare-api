namespace IdeaShare.Domain
{
    public class RequestWithPayloadResult<T> : BaseRequestResult
    {
        public T Payload { get; set; }
    }
}
