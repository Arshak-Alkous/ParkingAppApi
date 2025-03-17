namespace ParkingAppApi
{
    public class ParkingManager
    {
        private List<Period> periodList;
        private List<User> userList;
        private JsonFileManager<Period> periodFileManager = new JsonFileManager<Period>("periods.json");
        private JsonFileManager<User> userFileManager = new JsonFileManager<User>("users1.json");

        public ParkingManager()
        {
            try 
            {
                periodList = periodFileManager.ReadFromFile();
                userList = userFileManager.ReadFromFile();
            }
            catch (Exception ex) {Console.WriteLine(ex.Message);}
        }
       /* public void SaveData()
        {
            periodFileManager.WriteToFile(periodList);
            userFileManager.WriteToFile(userList);
        }*/
        public string? RegisterUser(User user)
        {
            User? newuser= userList.FirstOrDefault(u => u.UserName== user.UserName);
            if (newuser != null)
            {
                Console.WriteLine("User exists");
                return null;
            }
            string userid = user.FirstName + "_" + (userList.Count + 1).ToString();
            user.UserId = userid;
            userList.Add(user);
            userFileManager.WriteToFile(userList);
            Console.WriteLine($"User Added: {userid}");
            return userid;
        }
        public User? GetUserDetails(string userid)
        {
            return userList.FirstOrDefault(u => u.UserId == userid);
        }
        public List<Period>? GetAllPeriodsForUser(string userid)
        {
            return periodList.Where(p=>p.UserId == userid).ToList();
        }
        public List<Period>? GetAllPreviousPeriodsForUser(String userid)
        {
            List<Period>? previousPeriodes= periodList.Where(p => p.UserId == userid && p.EndTime != null).ToList();
            if (previousPeriodes != null)
                return previousPeriodes;
            else
            {
                Console.WriteLine($"there isn't previous Periods for {userid}");
                return null;
            }
        }
        public int BeginNewPeriod(string userid)
        {
            int periodid= periodList.Count + 1;
            Period period = new Period()
            {
                PeriodId = periodid,
                UserId = userid,
                StartTime = DateTime.Now,
                EndTime = null,
                PeriodCost = 0,
            }; 
            periodList.Add(period);
            periodFileManager.WriteToFile(periodList);
            Console.WriteLine($"period{periodid} begin for user {userid}");
            return periodid;
        }
        public Period? GetPresentPeriod(string userid)
        {
                Period? presentperiod = periodList.FirstOrDefault(x => x.UserId == userid&&x.EndTime==null);
            if(presentperiod != null)
            {
                Console.WriteLine($"The present period is {presentperiod.PeriodId} for user ({userid}) ");
                return presentperiod;
            }    
            else {
                Console.WriteLine($"there isn't present Period for {userid}");
                return null;
            }
        }
        public Period? EndPresentPeriod(string userid) 
        {
            Period? presentperiod = GetPresentPeriod(userid);
            if (presentperiod != null)
            {
                presentperiod.EndTime = DateTime.Now;
                presentperiod.PeriodCost = CalulateCostForPeriod(presentperiod.StartTime, presentperiod.EndTime.Value);
                periodFileManager.WriteToFile(periodList);
                var totalHuors = (presentperiod.EndTime.Value - presentperiod.StartTime).TotalHours;
                //var totalMinutes = (presentperiod.EndTime.Value - presentperiod.StartTime).TotalMinutes;
                Console.WriteLine($"the period {presentperiod.PeriodId} is ended for user :{userid} \n ,where cost for this period is {presentperiod.PeriodCost}and total time {totalHuors} Hours ");
                return presentperiod;
            }
            else
            {
                Console.WriteLine($"there isn't present Period for {userid}");
                return null;
            }
        }
        public decimal GetCostForUser(string userid) 
        {
            return periodList
                .Where(p=>p.UserId==userid)
                .Sum(p=>p.PeriodCost);
        }
        private decimal CalulateCostForPeriod(DateTime start, DateTime end)
        {
            decimal cost = 0;
            var minutes= (end - start).TotalMinutes;
            DateTime current=start;
            while (current<end) 
            {
                DateTime next;
                if (current.Hour >= 8 && current.Hour < 18)
                {
                    next = new DateTime(current.Year, current.Month, current.Day, 18, 0, 0);
                    if (next > end) next = end;
                    cost += (decimal)(next - current).TotalMinutes * (14m / 60);
                }
                else 
                {
                    next= current.Hour>=18?current.Date.AddDays(1).AddHours(8):current.Date.AddHours(8);
                    if (next > end) next = end;
                    cost += (decimal)(next - current).TotalMinutes * (6m / 60);
                }
                current = next;
            }
            /*for (int i = 0; i < minutes; i++) 
            {
                var currentMinute=start.AddMinutes(i);
                if(currentMinute.Hour >= 8 && currentMinute.Hour < 18)   cost += (decimal)(14.0m/60); 
                else  cost += (decimal)(6.0m / 60); 
            }*/
            return cost;
        }
       
    }
}
