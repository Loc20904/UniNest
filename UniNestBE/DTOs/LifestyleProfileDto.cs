namespace UniNestBE.DTOs
{
    public class LifestyleProfileDto
    {
        public string SleepSchedule { get; set; } = "Normal"; 
        public int CleanlinessLevel { get; set; } = 3;
        public bool Smoking { get; set; } = false;
        public bool HasPet { get; set; } = false;
        public string CookingHabit { get; set; } = "Sometimes";
        public string? GuestFrequency { get; set; } = "Occasional";
        public string? PreferredDistricts { get; set; }
        public string? PersonalityTraits { get; set; }
        public decimal BudgetMin { get; set; }
        public decimal BudgetMax { get; set; }
        public bool IsComplete { get; set; } = false;
        public List<int> LifestyleHabitIds { get; set; } = new List<int>();
    }
}
