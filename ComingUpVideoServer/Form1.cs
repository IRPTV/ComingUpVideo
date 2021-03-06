﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ComingUpVideoServer.MyDBTableAdapters;
namespace ComingUpVideoServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.ForeColor = Color.White;
            button1.Text = "Started";
            button1.BackColor = Color.Red;

            //string[] FilesList = Directory.GetFiles(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim());
            //foreach (string item in FilesList)
            //{
            //    try
            //    {
            //        if (File.GetLastAccessTime(item) < DateTime.Now.AddHours(-48))
            //        {
            //            File.Delete(item);
            //            richTextBox1.Text += (item) + " *Deleted* \n";
            //            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            //            richTextBox1.ScrollToCaret();
            //            Application.DoEvents();
            //        }
            //    }
            //    catch (Exception Exp)
            //    {
            //        richTextBox1.Text += (Exp) + " \n";
            //        richTextBox1.SelectionStart = richTextBox1.Text.Length;
            //        richTextBox1.ScrollToCaret();
            //        Application.DoEvents();
            //    }

            //}
           

            //GetList:
            tblDataTableAdapter Ta = new tblDataTableAdapter();
            MyDB.tblDataDataTable Dt = Ta.GetList();
            if (Dt.Rows.Count==2)
            {
                //ffmpeg -i movie.mp4 -ss 00:00:03 -t 00:00:08 -async 1 cut.mp4
                //Trim1:
                LogWriter("Start Triming Video 1");
                Process proc = new Process();
                proc.StartInfo.FileName = Path.GetDirectoryName(Application.ExecutablePath) + "//ffmpeg";
                string time1 = Dt[0]["time"].ToString().Trim().Split('#')[0];
                if (time1.Trim().Length <5)
                    time1 = "00:00:00";
                File.Delete(ConfigurationSettings.AppSettings["VideosPath"].ToString().Trim() + "001.mp4");
                File.Delete(ConfigurationSettings.AppSettings["VideosPath"].ToString().Trim() + "01.mp4");

                File.Copy(Dt[0]["source"].ToString().Trim().Split('[')[0].Trim(), ConfigurationSettings.AppSettings["VideosPath"].ToString().Trim() + "001.mp4",true);

                proc.StartInfo.Arguments = " -ss " + time1 + " -i " + "  \"" + ConfigurationSettings.AppSettings["VideosPath"].ToString().Trim() + "001.mp4" + "\"   -t 00:00:10 -y \"" + ConfigurationSettings.AppSettings["VideosPath"].ToString().Trim() + "01.mp4\"";
                LogWriter(proc.StartInfo.Arguments);
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.EnableRaisingEvents = true;
                proc.Start();
                proc.PriorityClass = ProcessPriorityClass.Normal;
                StreamReader reader = proc.StandardError;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    LogWriter(line);
                }
                proc.Close();
                LogWriter("End Triming Video 1");

                ////Trim2:
                LogWriter("Start Triming Video 2");
                Process proc2 = new Process();
                string time2 = Dt[1]["time"].ToString().Trim().Split('#')[0];
                if (time2.Trim().Length< 5)
                    time2 = "00:00:00";
                File.Delete(ConfigurationSettings.AppSettings["VideosPath"].ToString().Trim() + "002.mp4");
                File.Delete(ConfigurationSettings.AppSettings["VideosPath"].ToString().Trim() + "02.mp4");
                File.Copy(Dt[1]["source"].ToString().Trim().Split('[')[0].Trim(), ConfigurationSettings.AppSettings["VideosPath"].ToString().Trim() + "002.mp4",true);

                proc2.StartInfo.FileName = Path.GetDirectoryName(Application.ExecutablePath) + "//ffmpeg";
                proc2.StartInfo.Arguments = " -ss " + time2 + " -i   \"" + ConfigurationSettings.AppSettings["VideosPath"].ToString().Trim() + "002.mp4" + "\"   -t 00:00:10 -y \"" + ConfigurationSettings.AppSettings["VideosPath"].ToString().Trim() + "02.mp4\"";
                LogWriter(proc2.StartInfo.Arguments);
                proc2.StartInfo.RedirectStandardError = true;
                proc2.StartInfo.UseShellExecute = false;
                proc2.StartInfo.CreateNoWindow = true;
                proc2.EnableRaisingEvents = true;
                proc2.Start();
                proc2.PriorityClass = ProcessPriorityClass.Normal;
                StreamReader reader2 = proc2.StandardError;
                string line2;
                while ((line2 = reader2.ReadLine()) != null)
                {
                    LogWriter(line2);
                }
                proc2.Close();
                LogWriter("End Triming Video 2");

                //Xml:
                StringBuilder Data = new StringBuilder();
                for (int i = 0; i < Dt.Rows.Count; i++)
                {
                    string ProgName = Dt.Rows[i]["name"].ToString();

                    //2014-01-25 Replace Documentry by Doc
                    ProgName = ProgName.Replace("documentary", "Doc");
                    ProgName = ProgName.Replace("Documentary", "Doc");


                    int FirstIndex = ProgName.IndexOf("-");
                    int SecondIndex = 0;
                    if (FirstIndex > 0)
                    {
                        if (ProgName.StartsWith("Doc"))
                        {
                            SecondIndex = ProgName.IndexOf("-", FirstIndex + 1);
                            if (SecondIndex > FirstIndex)
                            {
                                ProgName = ProgName.Remove(FirstIndex, SecondIndex - FirstIndex + 1);
                                ProgName = ProgName.Insert(FirstIndex, ":");
                            }
                            ProgName = ProgName.Replace("  :", ":");
                            ProgName = ProgName.Replace(" :", ":");
                        }
                        else
                        {
                            //2015-07-13
                            ProgName = ProgName.Remove(FirstIndex, ProgName.Length - FirstIndex);
                        }

                    }

                    //Commented for new Coming UP:
                    int TitleLenght = int.Parse(ConfigurationSettings.AppSettings["TitleLenght"].ToString().Trim());
                    if (ProgName.Length > TitleLenght)
                    {
                        char[] delimiters = new char[] { ' ' };
                        string[] PrgNameList = ProgName.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                        int CutIndex = 0;
                        string OutName = "";
                        bool AllowAdd = true;
                        foreach (string item in PrgNameList)
                        {
                            if (CutIndex + item.Length + 1 < TitleLenght)
                            {
                                if (AllowAdd)
                                {
                                    CutIndex += item.Length + 1;
                                    OutName += item + " ";
                                }

                            }
                            else
                            {
                                AllowAdd = false;
                            }
                        }
                        //ProgName = ProgName.Remove(CutIndex, ProgName.Length - CutIndex);
                        //ProgName += "...";

                        ProgName = OutName + "...";
                    }
                    DateTime ProgTime = DateTime.Parse(Dt.Rows[i]["datetime"].ToString());
                    //Program1 = ["The World after Fukushima 2","01:00"]

                    string ProgTimeText = ProgTime.Hour.ToString("00") + ":" + ConfigurationSettings.AppSettings["TimeScheduleMinute"].ToString();
                    Data.AppendLine("Program" + (i + 1).ToString() + " = [\"" + ProgName + "\"]");
                    Data.AppendLine("Time" + (i + 1).ToString() + " = [\"" + ProgTimeText + "\"]");

                }
                StreamWriter StrW = new StreamWriter(ConfigurationSettings.AppSettings["XmlDataFile"].ToString().Trim());
                StrW.Write(Data);
                StrW.Close();


                //Render:
              Renderer();

            }
            timer1.Enabled = true;
        }
        protected void LogWriter(string LogText)
        {
            if (richTextBox1.Lines.Length > 8)
            {
                richTextBox1.Text = "";
            }

            richTextBox1.Text += (LogText) + " [ " + DateTime.Now.ToString("hh:mm:ss") + " ] \n";
            richTextBox1.Text += "===================\n";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
            Application.DoEvents();
        }
        protected void Renderer()
        {
            timer1.Enabled = false;
            Process proc = new Process();
            proc.StartInfo.FileName = "\"" + ConfigurationSettings.AppSettings["AeRenderPath"].ToString().Trim() + "\"";
            string DateTimeStr = string.Format("{0:0000}", DateTime.Now.Year) + "-" + string.Format("{0:00}", DateTime.Now.Month) + "-" + string.Format("{0:00}", DateTime.Now.Day) + "_" + string.Format("{0:00}", DateTime.Now.Hour) + "-" + string.Format("{0:00}", DateTime.Now.Minute) + "-" + string.Format("{0:00}", DateTime.Now.Second);

            DirectoryInfo Dir = new DirectoryInfo(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim());

            if (!Dir.Exists)
            {
                Dir.Create();
            }

            proc.StartInfo.Arguments = " -project " + "\"" + ConfigurationSettings.AppSettings["AeProjectFile"].ToString().Trim() + "\"" + "   -comp   \"" + ConfigurationSettings.AppSettings["Composition"].ToString().Trim() + "\" -output " + "\"" + ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + DateTimeStr + ".mp4" + "\"";
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.EnableRaisingEvents = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            if (!proc.Start())
            {
                return;
            }

            proc.PriorityClass = ProcessPriorityClass.Normal;
            StreamReader reader = proc.StandardOutput;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                richTextBox1.Text += (line) + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();
            }
            proc.Close();

            try
            {
                string StaticDestFileName = ConfigurationSettings.AppSettings["ScheduleDestFileName"].ToString().Trim();
                // File.Delete(StaticDestFileName);
                File.Copy(ConfigurationSettings.AppSettings["OutputPath"].ToString().Trim() + ConfigurationSettings.AppSettings["OutPutFileName"].ToString().Trim() + "_" + DateTimeStr + ".mp4", StaticDestFileName, true);
                richTextBox1.Text += "COPY FINAL:" + StaticDestFileName + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();
            }
            catch (Exception Ex)
            {
                richTextBox1.Text += Ex.Message + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();
            }

            timer1.Enabled = true;
            button1.ForeColor = Color.White;
            button1.Text = "Start";
            button1.BackColor = Color.Navy;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            button1_Click(new object(), new EventArgs());
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Interval = int.Parse(ConfigurationSettings.AppSettings["RenderIntervalSec"].ToString().Trim()) * 1000;
        }
    }
}
