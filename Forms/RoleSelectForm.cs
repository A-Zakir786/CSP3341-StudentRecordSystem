using StudentRecordSystem.Services;

namespace StudentRecordSystem.Forms
{
    //Lets the user choose which type of account will sign in.
    public class RoleSelectForm : Form
    {
        private readonly DatabaseService _db;

        public RoleSelectForm(DatabaseService db)
        {
            _db = db;

            //Set up the role selection window.
            Text = "Student Record System";
            Size = new Size(420, 250);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var heading = new Label
            {
                Text = "Select Account Type",
                Location = new Point(125, 35),
                AutoSize = true
            };

            var adminButton = new Button
            {
                Text = "Admin", Location = new Point(70, 105), Size = new Size(120, 40)
            };

            var studentButton = new Button
            {
                Text = "Student", Location = new Point(215, 105), Size = new Size(120, 40)
            };

            //Open the login form using the selected account type.
            adminButton.Click += (_, _) => new LoginForm(_db, "Admin").ShowDialog();
            studentButton.Click += (_, _) => new LoginForm(_db, "Student").ShowDialog();

            Controls.AddRange(new Control[] { heading, adminButton, studentButton });
        }
    }
}