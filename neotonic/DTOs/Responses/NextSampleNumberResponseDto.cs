namespace neotonic.DTOs.Responses
{
    public class NextSampleNumberResponseDto
    {
        public string NextNumber { get; set; } = string.Empty;
        public int NextSequence { get; set; }
        public int UomId { get; set; }
        public long UomNumber { get; set; }
    }
}