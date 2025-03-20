namespace ParkingAppApi
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Licenseplate { get; set; }
        public decimal Balance { get; set; } = 0.0m;
    }
}
