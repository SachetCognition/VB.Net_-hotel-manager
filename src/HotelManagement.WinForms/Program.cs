// WinForms entry point - requires Windows runtime for full UI functionality.
// On Linux/CI, only Core, Data, Tests, and Reports projects are built.

#if WINDOWS
using System;
using System.Windows.Forms;

namespace HotelManagement.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        // Application.Run(new Forms.frmLogin());
        // TODO: Implement WinForms UI layer
    }
}
#endif
