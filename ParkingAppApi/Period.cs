namespace ParkingAppApi
{
    public class Period
    {
        public int PeriodId { get; set; }
        public string UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal PeriodCost { get; set; }


    }
}
