using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text;
using Autodesk.Navisworks.Api.Controls;
using System.Diagnostics;
using Spatial_and_Temporal_Research;
using Microsoft.VisualBasic.ApplicationServices;

namespace Spatial_and_temporal_research
{
    static class program
    {
        /// <summary>
        /// the main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        { 
              //Set to single document mode
              Autodesk.Navisworks.Api.Controls.ApplicationControl.ApplicationType = ApplicationType.SingleDocument;

              //Initialise the api
              Autodesk.Navisworks.Api.Controls.ApplicationControl.Initialize();
              
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
           string[] args = Environment.GetCommandLineArgs();
            SingleInstanceController controller = new SingleInstanceController();
            controller.Run(args);

            //Finish use of the API.
            Autodesk.Navisworks.Api.Controls.ApplicationControl.Terminate(); 
        }
    }

    public class SingleInstanceController : WindowsFormsApplicationBase
    {
        public SingleInstanceController()
        {
            IsSingleInstance = true;

            StartupNextInstance += this_StartupNextInstance;
        }

        void this_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
        {
            MessageBox.Show("Spatial And Temporal Research is already running", "Spatial And Temporal Research", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        protected override void OnCreateMainForm()
        {
            MainForm = new mainForm();
            if(this.CommandLineArgs.Count > 1)
                (MainForm as mainForm).LoadDocument(this.CommandLineArgs[1], this.CommandLineArgs[1].Substring(0, this.CommandLineArgs[1].Length - 3) + "csv");
        }
    }
}

