using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.IO;

namespace PingIP
{
    public partial class frmPingIPs : Form
    {
        StringBuilder strResult;
        BackgroundWorker bgw = new BackgroundWorker();
        string strPingFormat = "Reply from {0}: bytes={1} time={2}ms TTL={3}" + Environment.NewLine;
        public frmPingIPs()
        {
            InitializeComponent();

            bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
            bgw.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);
            bgw.WorkerReportsProgress = true;
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            Ping ping = new Ping();
            strResult = new StringBuilder();
            CommonUtil objUtil = new CommonUtil();
            List<string> lstIP = new List<string>
                                    {
                                        "172.32.16.235",
                                        "8.8.8.8"
                                    };

            List<int> lstPort = new List<int>{
                                                23,
                                                80,
                                                443
                                             };

            string strIP;
            int intPct = 0;
            int intTimeoutCount = 0;
            string strTelnetResult = string.Empty;
            for (int i = 0; i < lstIP.Count; i++)
            {
                strIP = lstIP[i];

                //PING IP
                strResult.Append("***************** IP: " + strIP + " *****************" + Environment.NewLine);
                strResult.Append("PING " + strIP + Environment.NewLine);
                intTimeoutCount = 0;
                for (int j = 0; j < 4; j++)
                {
                    PingReply pingResult = ping.Send(strIP, 3000);
                    if (pingResult.Status == IPStatus.Success)
                    {
                        strResult.AppendFormat(strPingFormat, strIP, pingResult.Buffer.Length.ToString(), pingResult.RoundtripTime.ToString(), pingResult.Options.Ttl.ToString());
                    }
                    else
                    {
                        intTimeoutCount = intTimeoutCount + 1;
                        strResult.Append(pingResult.Status.ToString() + Environment.NewLine);
                    }
                    pingResult = null;
                }
                strResult.Append(Environment.NewLine + "TRACERT " + strIP + Environment.NewLine);
                intPct = (((3 * i) + 1) * 100) / (lstIP.Count * 3);
                bgw.ReportProgress(intPct, i);


                //TRACE IP
                try
                {
                    var lstTracert = objUtil.Tracert(strIP, 30, 3000).ToList();
                    foreach (Tracert objTrace in lstTracert)
                    {
                        strResult.Append(objTrace.ToString() + Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    strResult.Append(ex.Message + Environment.NewLine);
                }

                strResult.Append(Environment.NewLine + "TELNET " + strIP + Environment.NewLine);
                intPct = (((3 * i) + 2) * 100) / (lstIP.Count * 3);
                bgw.ReportProgress(intPct, i);


                //TELNET CONNECTION
                foreach(int port in lstPort)
                {
                    try
                    {
                        strTelnetResult = objUtil.CheckTelnetConnection(strIP, port);
                    }
                    catch(Exception ex)
                    {
                        strTelnetResult = ex.Message;
                    }

                    strResult.Append(strIP + " " + port + " " + strTelnetResult + Environment.NewLine);
                }
                strResult.Append(Environment.NewLine + Environment.NewLine);
                intPct = (((3 * i) + 3) * 100) / (lstIP.Count * 3);
                bgw.ReportProgress(intPct, i);

            }

            //int total = 57; //some number (this is your variable to change)!!

            //for (int i = 0; i <= total; i++) //some number (total)
            //{
            //    System.Threading.Thread.Sleep(1000);
            //    int percents = (i * 100) / total;
            //    bgw.ReportProgress(percents, i);
            //    //2 arguments:
            //    //1. procenteges (from 0 t0 100) - i do a calcumation 
            //    //2. some current value!
            //}
        }

        private void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            txtPingResult.Text = strResult.ToString();
            txtPingResult.ScrollToCaret();
            pbProcessing.Value = e.ProgressPercentage;
        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            strResult.Append("Completed !!!");
            txtPingResult.ScrollToCaret();
            txtPingResult.Text = strResult.ToString();
            strResult = null;
        }

        private void btnStartPing_Click(object sender, EventArgs e)
        {
            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();
            else
            {
                MessageBox.Show("! Wait - Process is already running.");
            }
        }

        private void txtPingResult_TextChanged(object sender, EventArgs e)
        {
            txtPingResult.SelectionStart = txtPingResult.Text.Length;
            txtPingResult.ScrollToCaret();

        }

        private void btnClipBoard_Click(object sender, EventArgs e)
        {
            if (!bgw.IsBusy)
            {
                Clipboard.SetText(txtPingResult.Text);
                MessageBox.Show("Copied to Clipboard");
            }
            else
            {
                MessageBox.Show("! Wait - Process is running.");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!bgw.IsBusy)
            {
                try
                {
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.Filter = "Text File | *.txt";
                    saveFileDialog1.Title = "Save Trace Log";
                    bool isOk = saveFileDialog1.ShowDialog() == DialogResult.OK;
                    if (saveFileDialog1.FileName != "" && isOk)
                    {
                        StreamWriter writer = new StreamWriter(saveFileDialog1.OpenFile());
                        writer.WriteLine(txtPingResult.Text);
                        writer.Dispose();
                        writer.Close();
                        MessageBox.Show("File saved: " + saveFileDialog1.FileName);
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Unable to save due to: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("! Wait - Process is running.");
            }
        }
    }
}
