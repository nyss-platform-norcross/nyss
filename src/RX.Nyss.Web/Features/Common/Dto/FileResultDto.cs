namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class FileResultDto
    {
        public FileResultDto(byte[] data, string mimeType)
        {
            Data = data;
            MimeType = mimeType;
        }

        public byte[] Data { get; }

        public string MimeType { get; }
    }
}
