using StudentRecordSystem.Models;

namespace StudentRecordSystem.Services
{
    //Provides grade, GPA and Weighted Average Mark (WAM) calculations.
    public static class WamCalculator
    {
        //Converting a numerical mark into its corresponding grade classification.
        public static string GetGrade(double mark)
        {
            if (mark >= 80) return "HD";
            if (mark >= 70) return "D";
            if (mark >= 60) return "CR";
            if (mark >= 50) return "P";
            return "N";
        }

        // Converts a numerical mark into grade points for the GPA calculation.
        public static double GetGradePoints(double mark)
        {
            if (mark >= 80) return 4.0;
            if (mark >= 70) return 3.0;
            if (mark >= 60) return 2.0;
            if (mark >= 50) return 1.0;
            return 0.0;
        }

        // Calculate the student's credit weighted WAM, GPA and overall result classification.
        public static WamResult Calculate(List<AcademicRecord> records)
        {
            // Handles students who do not yet have any academic records.
            if (records == null || records.Count == 0)
                return new WamResult { Standing = "No Records Available" };

            //Calculate the weighted mark total, weighted GPA total and completed credit points.
            double totalWeightedMarks =
                records.Sum(r => r.Mark * r.CreditPoints);

            double totalWeightedGpa =
                records.Sum(r => GetGradePoints(r.Mark) * r.CreditPoints);

            int totalCredits =
                records.Sum(r => r.CreditPoints);
                
            //Preventing division by zero if no valid credit points are available.
            if (totalCredits == 0)
                return new WamResult();

            // Calculating and rounding the final WAM and GPA values.
            double wam =
                Math.Round(totalWeightedMarks / totalCredits, 2);

            double gpa =
                Math.Round(totalWeightedGpa / totalCredits, 2);

            string standing =
                wam >= 80 ? "High Distinction (HD)" :
                wam >= 70 ? "Distinction (D)" :
                wam >= 60 ? "Credit (CR)" :
                wam >= 50 ? "Pass (P)" :
                            "Fail (N)";

            return new WamResult
            {
                Wam = wam,
                Gpa = gpa,
                TotalCredits = totalCredits,
                Standing = standing
            };
        }
    }
}