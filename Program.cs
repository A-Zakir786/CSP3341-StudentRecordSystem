using StudentRecordSystem.Forms;
using StudentRecordSystem.Services;

namespace StudentRecordSystem;

static class Program
{
    //Main entry point for the application.
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        //Prepare the database and demo data.
        var db = new DatabaseService();
        db.Initialize();

        //Open the first screen.
        Application.Run(new RoleSelectForm(db));
    }
}

