using StudentRecordSystem.Models;
using StudentRecordSystem.Services;

namespace StudentRecordSystem.Forms
{
    //Handles sign in for the selected Admin or Student role.
    public class LoginForm : Form
    {
        private readonly DatabaseService _db;
        private readonly string _role;
        private readonly TextBox _username = new();
        private readonly TextBox _password = new();
        private readonly Label _status = new();

        public LoginForm(DatabaseService db, string role)
        {
            //Keep the shared database service and selected role for authentication.
            _db = db;
            _role = role;

            //Set up the main login window.
            Text = $"{role} Sign In";
            Size = new Size(420, 330);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var heading = new Label
            {
                Text = $"{role} Sign In",
                Location = new Point(35, 25),
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold)
            };

            var usernameLabel = new Label
            {
                Text = "Username:", Location = new Point(35, 70), AutoSize = true
            };
            _username.Location = new Point(35, 92);
            _username.Width = 330;

            var passwordLabel = new Label
            {
                Text = "Password:", Location = new Point(35, 130), AutoSize = true
            };

            _password.Location = new Point(35, 152);
            _password.Width = 330;
            _password.UseSystemPasswordChar = true;

            var signIn = new Button
            {
                Text = "Sign In", Location = new Point(35, 190), Size = new Size(120, 32)
            };
            signIn.Click += SignIn;

            _status.Location = new Point(35, 255);
            _status.Size = new Size(330, 25);

            //Display the appropriate demo account for the selected role.
            var demo = new Label
            {
                Text = role == "Admin"
                    ? "Demo: admin / admin123"
                    : "Demo: student / student123",
                Location = new Point(175, 223),
                AutoSize = true
            };

            Controls.AddRange(new Control[]
            {
                heading, usernameLabel, _username,
                passwordLabel, _password,
                signIn, _status, demo
            });

            AcceptButton = signIn;
        }

        //Checks the credentials and opens the correct interface.
        private void SignIn(object? sender, EventArgs e)
        {
            //Prevent authentication when either login field is empty.
            if (string.IsNullOrWhiteSpace(_username.Text) ||
                string.IsNullOrWhiteSpace(_password.Text))
            {
                _status.Text = "Enter both username and password.";
                return;
            }

            User? user = _db.Authenticate(
                _username.Text,
                _password.Text,
                _role);

            if (user == null)
            {
                _status.Text = "Invalid username, password or role.";
                return;
            }

            Hide();

            //Open the interface that matches the authenticated user type.
            if (user is AdminUser admin)
                new AdminForm(admin, _db).ShowDialog();
            else if (user is StudentUser student)
                new StudentForm(student, _db).ShowDialog();

            Close();
        }
    }
}