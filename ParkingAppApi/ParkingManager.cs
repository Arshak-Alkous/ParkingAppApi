﻿namespace ParkingAppApi
{
    public class ParkingManager
    {
        private List<Period> periodList;
        private List<User> userList;
        private JsonFileManager<Period> periodFileManager = new JsonFileManager<Period>("periods2.json");
        private JsonFileManager<User> userFileManager = new JsonFileManager<User>("users2.json");

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
        public int? RegisterUser(User user)
        {
            User? newuser= userList.FirstOrDefault(u => u.UserName== user.UserName);
            if (newuser != null)
            {
                Console.WriteLine("User exists");
                return null;
            }
            int userid = userList.Count + 1;
            user.UserId = userid;
            userList.Add(user);
            userFileManager.WriteToFile(userList);
            Console.WriteLine($"User Added: {userid}");
            return userid;
        }
        public User? Login(string userName,string password)
        {
            User? user = userList.FirstOrDefault(u => u.UserName == userName);
            if (user == null || user.Password != password) return null;
            return user;
        }
        public User? GetUserDetails(int userid)
        {
            return userList.FirstOrDefault(u => u.UserId == userid);
        }
        public List<Period>? GetAllPeriodsForUser(int userid)
        {
            return periodList.Where(p=>p.UserId == userid).ToList();
        }
        public List<Period>? GetAllPreviousPeriodsForUser(int userid)
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
        public int BeginNewPeriod(int userid)
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
        public Period? GetPresentPeriod(int userid)
        {
                Period? presentperiod = periodList.FirstOrDefault(x => x.UserId == userid&&x.EndTime==null);
            if(presentperiod != null)
            {
                Console.WriteLine($"The present period is {presentperiod.PeriodId} for user ({userid}) ");
                presentperiod.PeriodCost = CalulateCostForPeriod(presentperiod.StartTime, DateTime.Now);
                return presentperiod;
            }    
            else {
                Console.WriteLine($"there isn't present Period for {userid}");
                return null;
            }
        }
        public Period? EndPresentPeriod(int userid) 
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
        public decimal GetCostForUser(int userid) 
        {
            User? user= userList.FirstOrDefault(u => u.UserId == userid);
            decimal totalCost= periodList
                .Where(p => p.UserId == userid)
                .Sum(p => p.PeriodCost);
            if (user!=null)user.Balance = totalCost;
            return totalCost;
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
