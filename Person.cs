namespace PerformanceBenchmarks;

public class Person
{
    public string FirstName { get; set; } = null!;
    public string MiddleInitial { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int Age { get; set; }
    public int CSharpExperienceYears { get; set; }
    public int SQLExperienceYears { get; set; }
    public int HTMLExperienceYears { get; set; }

    public string FullName
    {
        get
        {
            return string.Concat(LastName, ", ", FirstName, " ", MiddleInitial);
        }
    }
    public override string ToString()
    {
        return $"{FirstName},{MiddleInitial},{LastName},{Age},{CSharpExperienceYears},{SQLExperienceYears},{HTMLExperienceYears}";
    }
}