using  StudentRecordSystem.Models;
using StudentRecordSystem.Services;

namespace StudentRecordSystem.Forms  
{
    //Displays the signed in students academic information in a read only view.
    public class StudentForm : Form
    {
        private readonly StudentUser _student;
        private readonly DatabaseService _db;

        private readonly Label _wam = new();
        private readonly Label _gpa = new();
        private readonly Label _standing = new();
        private readonly DataGridView _transcript = new();

        public StudentForm(StudentUser student, DatabaseService db)
        {
            _student = student;
            _db = db;

            //setup the basic window appearance  
            Text = "Student Academic Record";
            Size = new Size(900, 560);
            StartPosition = FormStartPosition.CenterScreen;

            var heading = new Label
            {
                Text = $"Student: {student.FullName}    ID: {student.StudentId}",
                Location = new Point(20, 20),
                AutoSize = true
            };

            var signOut = new Button
            {
                Text = "Sign Out",
                Location = new Point(775, 15),
                Size = new Size(90, 30)
            };

            signOut.Click += (_, _) => Close();

            //Adds the three calculated academic values to the summary area.
            AddSummary("WAM:", _wam, 20);
            AddSummary("GPA:", _gpa, 185);
            AddSummary("Classification:", _standing, 350);

            var title = new Label
            {
                Text = "Academic Transcript",
                Location = new Point(20, 105),
                AutoSize = true
            };

            //Configure the transcript table as a read only display
            _transcript.Location = new Point(20, 130);
            _transcript.Size = new Size(845, 360);
            _transcript.ReadOnly = true;
            _transcript.MultiSelect = false;
            _transcript.RowHeadersVisible = false;
            _transcript.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            Controls.AddRange(new Control[]
            {
                heading,
                signOut,
                title,
                _transcript
            });

            LoadData();  //Load the students calculated academic results when the form opens.
        }

        //Adds one result label and its value to the summary section.
        private void AddSummary(string title, Label value, int x)
        {
            var label = new Label
            {
                Text = title,
                Location = new Point(x, 65),
                AutoSize = true
            };

            value.Location = new Point(x, 85);
            value.AutoSize = true;

            Controls.Add(label);
            Controls.Add(value);
        }

        //Loads the current students transcript and calculated academic results.
        private void LoadData()
        {
            var records = _db.GetRecords(_student.StudentId);
            _transcript.DataSource = records;

            WamResult result = WamCalculator.Calculate(records);

            _wam.Text = $"{result.Wam}%";
            _gpa.Text = $"{result.Gpa} / 4.0";
            _standing.Text = result.Standing;
        }
    }
}