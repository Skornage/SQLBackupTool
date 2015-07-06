using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Data.SqlClient;

namespace SQLBackupTool
{
	class Program
	{
		static void Main(string[] args)
		{
			//SqlConnection conn = new SqlConnection("Data Source=(LocalDb)\v11.0;Initial Catalog=ContosoUniversity2;Integrated Security=SSPI;");
			//conn.Open();
			//SqlCommand cmd = new SqlCommand("show tables", conn);
			//cmd.ExecuteNonQuery();

			ProcessStartInfo cmdStartInfo = new ProcessStartInfo();
			cmdStartInfo.FileName = @"C:\Windows\System32\cmd.exe";
			cmdStartInfo.RedirectStandardOutput = true;
			cmdStartInfo.RedirectStandardError = true;
			cmdStartInfo.RedirectStandardInput = true;
			cmdStartInfo.UseShellExecute = false;
			cmdStartInfo.CreateNoWindow = true;

			Process p = new Process();
			p.StartInfo = cmdStartInfo;
			p.OutputDataReceived += runCommand;
			p.Start();
			p.BeginOutputReadLine();
			p.BeginErrorReadLine();
			string[] commands = System.IO.File.ReadAllLines(@"./../../TextFile1.bat");
			foreach (string command in commands)
			{
				p.StandardInput.WriteLine(command);
			}
			//p.StandardInput.WriteLine("Testing...");
			p.WaitForExit();
		}

		static void runCommand(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine(e.Data);
		}
	}
}
