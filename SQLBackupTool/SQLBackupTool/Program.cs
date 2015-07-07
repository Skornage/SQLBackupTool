using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Mail;
using System.Data.SqlClient;

namespace SQLBackupTool
{
	class Program
	{
		static void Main(string[] args)
		{
			string[] cmdArgs = Environment.GetCommandLineArgs();
			Dictionary<String, String> parameters = getConfigParameters(cmdArgs[1]);

			try 
			{
				SqlConnection conn = new SqlConnection(parameters["connectionString"]);
				conn.Open();
				SqlCommand cmd = new SqlCommand("BACKUP DATABASE " + parameters["dbName"] + " TO DISK='" + parameters["backupPath"] + parameters["dbName"] + "-" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + "-.bak'", conn);

				cmd.ExecuteNonQuery();
				string[] backupFiles = System.IO.Directory.GetFiles(parameters["backupPath"], parameters["dbName"] + "*");
				int countBackups = backupFiles.Length;
				if (countBackups > Int32.Parse(parameters["maxBackups"]))
				{
					removeOldestBackup(backupFiles, parameters);
				}
			}
			catch (Exception e)
			{
				SmtpClient client = new SmtpClient("smtp.gmail.com");
				client.Port = 587;
				client.EnableSsl = true;
				client.UseDefaultCredentials = false;
				client.Credentials = new System.Net.NetworkCredential("wddtester@gmail.com", "wddtester89");

				MailMessage mail = new MailMessage();
				mail.From = new MailAddress("wddtester@gmail.com");
				string[] recipients = parameters["notifyEmailAddresses"].Split(',');
				foreach (string recipient in recipients)
				{
					mail.To.Add(new MailAddress(recipient.Trim()));
				}
				mail.Subject = "Error with Database Backup";
				mail.Body = @"There was an error backing up " + parameters["dbName"] + "\n" + e.Message;

				client.Send(mail);
			}
			
		}

		private static void removeOldestBackup(string[] backupFiles, Dictionary<String, String> parameters)
		{
			DateTime[] timeStamps = new DateTime[backupFiles.Length];
			for (int i = 0; i < backupFiles.Length; i++)
			{
				string[] timestamp = backupFiles[i].Split('-');
				DateTime toAdd = new DateTime(Int32.Parse(timestamp[1]), Int32.Parse(timestamp[2]), Int32.Parse(timestamp[3]),
					Int32.Parse(timestamp[4]), Int32.Parse(timestamp[5]), Int32.Parse(timestamp[6]));
				timeStamps[i] = toAdd;
			}

			DateTime leastRecentBackup = timeStamps.OrderBy(x => x).FirstOrDefault();
			string filePath = parameters["backupPath"] + parameters["dbName"] + "-" + leastRecentBackup.ToString("yyyy-MM-dd-HH-mm-ss") + "-.bak";
			System.IO.File.Delete(filePath);
		}

		static Dictionary<String, String> getConfigParameters(String filename)
		{
			Dictionary<String, String> result = new Dictionary<String, String>();
			using (System.IO.StreamReader sr = new System.IO.StreamReader("./../../" + filename))
			{
				while (!sr.EndOfStream)
				{
					String nextLine = sr.ReadLine();
					string[] split = nextLine.Split(new string[] { "=>" }, StringSplitOptions.None);
					result.Add(split[0].Trim(), split[1].Trim());
				}
			}
			return result;
		}
	}
}
