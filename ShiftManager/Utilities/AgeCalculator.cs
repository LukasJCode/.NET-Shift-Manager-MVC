using System;

namespace ShiftManager.Utilities
{
    public static class AgeCalculator
    {
        public static int CalculateEmployeeAge(DateTime DOB)
        {
            var birthday = DOB;
            var today = DateTime.Today;
            var age = today.Year - birthday.Year;

            //Checking if persons birthday already passed
            if (birthday > today.AddYears(-age))
                age--;

            return age;
        }
    }
}
