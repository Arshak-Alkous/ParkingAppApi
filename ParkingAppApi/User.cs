namespace ParkingAppApi
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Password { get; set; }
        public string Licenseplate { get; set; }
        public decimal Balance { get; set; } = 0.0m;
    }
}
