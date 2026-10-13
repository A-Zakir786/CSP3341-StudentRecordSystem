using StudentRecordSystem.Models;
using StudentRecordSystem.Services;

namespace StudentRecordSystem.Forms
{
    //Provides the administrator view for student profiles and academic results.
    public class AdminForm : Form
    {
        private readonly DatabaseService _db;

        private readonly DataGridView _students = new();
        private readonly DataGridView _records = new();

        private readonly TextBox _studentId = new();
        private readonly TextBox _fullName = new();
        private readonly TextBox _email = new();
        private readonly TextBox _course = new();

        private readonly NumericUpDown _year = new() { Minimum = 1, Maximum = 6 };
        private readonly ComboBox _unit = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown _mark = new()
        {
            Minimum = 0,
            Maximum = 100,
            DecimalPlaces = 1,
            Value = 70
        };

        private readonly Label _summary = new();

        public AdminForm(AdminUser admin, DatabaseService db)
        {
            //Keep the shared database service available to the form.
            _db = db;
            Text = "Student Record System - Admin";
            Size = new Size(1020, 650);
            StartPosition = FormStartPosition.CenterScreen;
            var heading = CreateLabel($"Admin: {admin.FullName}", 20, 15);
            var signOut = CreateButton("Sign Out", 895, 10, 90);
            signOut.Click += (_, _) => Close();

            // Build the two main areas of the administrator interface.
            BuildStudentSection();
            BuildRecordSection();
            Controls.AddRange(new Control[]
            {
                heading,
                signOut
            });
            //Load the existing student profiles when the form opens.
            LoadStudents();
        }

        //Builds the student list, profile fields and management buttons.
        private void BuildStudentSection()
        {
            Controls.Add(CreateLabel("Students", 20, 55));
            _students.Location = new Point(20, 80);
            _students.Size = new Size(470, 210);
            SetupGrid(_students);
            _students.SelectionChanged += (_, _) => ShowSelectedStudent();
            int y = 315;
            AddField("Student ID:", _studentId, y);
            AddField("Full Name:", _fullName, y += 38);
            AddField("Email:", _email, y += 38);
            AddField("Course:", _course, y += 38);
            AddField("Year:", _year, y += 38);
            var newButton = CreateButton("New", 20, 515, 90);
            var saveButton = CreateButton("Add / Update", 120, 515, 120);
            var deleteButton = CreateButton("Delete Student", 250, 515, 120);
            newButton.Click += (_, _) => ClearFields();
            saveButton.Click += SaveStudent;
            deleteButton.Click += DeleteStudent;
            Controls.AddRange(new Control[]
            {
                _students,
                newButton,
                saveButton,
                deleteButton
            });
        }

        //Builds the academic record grid and grade management controls.
        private void BuildRecordSection()
        {
            Controls.Add(CreateLabel("Academic Records", 520, 55));
            _summary.Location = new Point(520, 80);
            _summary.Size = new Size(450, 40);
            _records.Location = new Point(520, 120);
            _records.Size = new Size(450, 250);
            SetupGrid(_records);
            var unitLabel = CreateLabel("Unit:", 520, 395);
            _unit.Location = new Point(520, 417);
            _unit.Width = 350;
            var markLabel = CreateLabel("Mark:", 520, 455);
            _mark.Location = new Point(520, 477);
            _mark.Width = 100;
            var saveGrade = CreateButton("Add / Update Grade", 520, 525, 150);
            var deleteGrade = CreateButton("Delete Grade", 680, 525, 120);
            Controls.AddRange(new Control[]
            {
                _summary,
                _records,
                unitLabel,
                _unit,
                markLabel,
                _mark,
                saveGrade,
                deleteGrade
            });
        }

        //Applies the common read-only display settings to both grids.
        private static void SetupGrid(DataGridView grid)
        {
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        //Adds one labelled input control to the student profile section.
        private void AddField(string text, Control input, int y)
        {
            Controls.Add(CreateLabel(text, 20, y));
            input.Location = new Point(120, y - 3);
            input.Width = input is NumericUpDown ? 80 : 300;

            Controls.Add(input);
        }

        //Creates a standard label used by this form.
        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        //Creates a standard button used by this form.
        private static Button CreateButton(string text, int x, int y, int width)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 32)
            };
        }

        //Reloads the student list from the database.
        private void LoadStudents()
        {
            _students.DataSource = _db.GetStudents();

            if (_students.Rows.Count == 0)
            {
                ClearFields();
                _summary.Text = "No student selected.";
            }
        }

        //Copies the selected student's profile into the edit controls.
        private void ShowSelectedStudent()
        {
            Student? student = GetSelectedStudent();

            if (student == null) return;

            _studentId.Text = student.StudentId;
            _studentId.ReadOnly = true;
            _fullName.Text = student.FullName;
            _email.Text = student.Email;
            _course.Text = student.CourseTitle;
            _year.Value = student.YearLevel;
        }

        //Returns the student currently selected in the grid.
        private Student? GetSelectedStudent()
        {
            if (_students.SelectedRows.Count == 0) return null;

            return _students.SelectedRows[0].DataBoundItem as Student;
        }

        //Clears the profile fields so a new student can be entered.
        private void ClearFields()
        {
            _students.ClearSelection();

            _studentId.ReadOnly = false;
            _studentId.Clear();
            _fullName.Clear();
            _email.Clear();
            _course.Clear();
            _year.Value = 1;

            _records.DataSource = null;
            _summary.Text = "Enter a new student.";
        }

        //Adds a new student or updates the currently selected profile.
        private void SaveStudent(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_studentId.Text) ||
                string.IsNullOrWhiteSpace(_fullName.Text))
            {
                MessageBox.Show("Student ID and Full Name are required.");
                return;
            }

            var student = new Student
            {
                StudentId = _studentId.Text.Trim(),
                FullName = _fullName.Text.Trim(),
                Email = _email.Text.Trim(),
                CourseTitle = _course.Text.Trim(),
                YearLevel = (int)_year.Value
            };

            if (!_db.SaveStudent(student))
            {
                MessageBox.Show("The student could not be saved.");
                return;
            }

            LoadStudents();
            SelectStudent(student.StudentId);
        }

        //reselects the saved student after the grid refreshes.
        private void SelectStudent(string studentId)
        {
            foreach (DataGridViewRow row in _students.Rows)
            {
                if (row.DataBoundItem is Student student &&
                    student.StudentId == studentId)
                {
                    _students.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }

        //deletes the selected student and their linked records.
        private void DeleteStudent(object? sender, EventArgs e)
        {
            Student? student = GetSelectedStudent();

            if (student == null)
            {
                MessageBox.Show("Select a student first.");
                return;
            }

            DialogResult confirmation = MessageBox.Show(
                $"Delete {student.FullName}?",
                "Confirm Delete",
                MessageBoxButtons.YesNo);

            if (confirmation != DialogResult.Yes) return;

            if (!_db.DeleteStudent(student.StudentId))
            {
                MessageBox.Show("The student could not be deleted.");
                return;
            }

            LoadStudents();
        }
    }
}
