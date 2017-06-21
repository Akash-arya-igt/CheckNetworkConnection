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
using NSpeedTest;
using NSpeedTest.Models;
using System.Globalization;
using PingIP.Models;
using System.Diagnostics;
using System.Net;

namespace PingIP
{
    public partial class frmPingIPs : Form
    {
        StringBuilder strResult;
        private Settings settings;
        private SpeedTestClient client;
        private string DefaultCountry = "India";
        BackgroundWorker bgw = new BackgroundWorker();
        string strPingFormat = "Reply from {0}: bytes={1} time={2}ms TTL={3}" + Environment.NewLine;
        bool isSettingsLoaded = false;

        public frmPingIPs()
        {
            InitializeComponent();

            bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
            bgw.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);
            bgw.WorkerReportsProgress = true;

            BindCountryDropdown();
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            Ping ping = new Ping();
            strResult = new StringBuilder();
            CommonUtil objUtil = new CommonUtil();

            int intPct = 0;
            string strCheckServer;
            string serverIP = string.Empty;
            string strTelnetResult = string.Empty;

            List<int> lstPort = new List<int> { 23, 80, 443 };

            List<PingServerDetail> lstServer = GetServerList();
            int intTotalStepCount = (4 * lstServer.Count) + 1;

            #region STEP 1: CHECK INTERNET CONNECTON SPEED
            strResult.Append("***************** CHECKING INTERNET CONNECTION *****************" + Environment.NewLine);
            try
            {
                if (isSettingsLoaded)
                {
                    strResult.Append("Getting server detail with lowest latency..." + Environment.NewLine);
                    bgw.ReportProgress(0, 0);
                    var bestServer = SelectBestServer();
                    strResult.Append("Testing speed..." + Environment.NewLine);
                    var downloadSpeed = client.TestDownloadSpeed(bestServer, settings.Download.ThreadsPerUrl);
                    var uploadSpeed = client.TestUploadSpeed(bestServer, settings.Upload.ThreadsPerUrl);
                    strResult.Append(FormatSpeed("Download", downloadSpeed) + Environment.NewLine);
                    strResult.Append(FormatSpeed("Upload", uploadSpeed) + Environment.NewLine);
                }
                else
                {
                    strResult.Append("Setting are not loaded correctly..." + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                strResult.Append("Unable to get internet connection speed due to: " + ex.Message + Environment.NewLine);
            }
            #endregion

            strResult.Append(Environment.NewLine);
            intPct = ((1) * 100) / (intTotalStepCount);
            bgw.ReportProgress(intPct, 1);

            for (int i = 0; i < lstServer.Count; i++)
            {
                serverIP = string.Empty;
                string strPingedServerIP = string.Empty;
                strCheckServer = lstServer[i].ServerName;
                serverIP = !string.IsNullOrEmpty(serverIP) ? serverIP : lstServer[i].ServerIP;
                strResult.Append("***************** Server Name: " + strCheckServer + " *****************" + Environment.NewLine);

                #region STEP 2: LAUNCH BROWSER
                if (lstServer[i].HostURL != null)
                {
                    strResult.Append(Environment.NewLine + "Checking health of applcation running on URL: " + lstServer[i].HostURL + Environment.NewLine);
                    try
                    {
                        //Process.Start("iexplore.exe", lstServer[i].HostURL);
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(lstServer[i].HostURL);
                        using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                        {
                            if (response == null || response.StatusCode != HttpStatusCode.OK)
                                strResult.Append("Application having some issue"
                                                    + response != null && !string.IsNullOrEmpty(response.StatusCode.ToString())
                                                        ? "(" + response.StatusCode.ToString() + ")"
                                                        : string.Empty
                                                    + Environment.NewLine);
                            else
                                strResult.Append("Application running healthy" + Environment.NewLine);


                            response.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        strResult.Append(string.Format("Unable to get response from url {0} due to: {1}", lstServer[i].HostURL, ex.Message) + Environment.NewLine);
                    }
                }
                #endregion

                strResult.Append(Environment.NewLine + "PING " + strCheckServer + Environment.NewLine);
                intPct = (((4 * i) + 2) * 100) / (intTotalStepCount);
                bgw.ReportProgress(intPct, i);

                #region STEP 3: PING IP
                try
                {
                    for (int j = 0; j < 4; j++)
                    {
                        PingReply pingResult = ping.Send(strCheckServer, 3000);

                        if (j == 0 && pingResult.Address != null && string.IsNullOrEmpty(strPingedServerIP))
                            strPingedServerIP = pingResult.Address.ToString();

                        if (pingResult.Status == IPStatus.Success)
                        {
                            strResult.AppendFormat(strPingFormat, strCheckServer, pingResult.Buffer.Length.ToString(), pingResult.RoundtripTime.ToString(), pingResult.Options.Ttl.ToString());
                        }
                        else
                        {
                            strResult.Append(pingResult.Status.ToString() + Environment.NewLine);
                        }
                        pingResult = null;
                    }
                }
                catch (Exception ex)
                {
                    strResult.Append("Unable to ping server due to: " + ex.Message + Environment.NewLine);
                }

                if (!string.IsNullOrEmpty(strPingedServerIP))
                    strResult.Append("Server '" + strCheckServer + "' has IP: " + strPingedServerIP + Environment.NewLine);

                strResult.Append(Environment.NewLine + "PING " + serverIP + Environment.NewLine);
                try
                {
                    intPct = (int)(((4 * i) + 2.5) * 100) / (intTotalStepCount);
                    bgw.ReportProgress(intPct, i);
                }
                catch { /*DO NOTHING*/}
                try
                {
                    for (int j = 0; j < 4; j++)
                    {
                        PingReply pingResult = ping.Send(serverIP, 3000);

                        if (j == 0 && pingResult.Address != null)
                            serverIP = pingResult.Address.ToString();

                        if (pingResult.Status == IPStatus.Success)
                        {
                            strResult.AppendFormat(strPingFormat, strCheckServer, pingResult.Buffer.Length.ToString(), pingResult.RoundtripTime.ToString(), pingResult.Options.Ttl.ToString());
                        }
                        else
                        {
                            strResult.Append(pingResult.Status.ToString() + Environment.NewLine);
                        }
                        pingResult = null;
                    }
                }
                catch (Exception ex)
                {
                    strResult.Append("Unable to ping server due to: " + ex.Message + Environment.NewLine);
                }
                #endregion

                strResult.Append(Environment.NewLine + "TRACERT " + serverIP + Environment.NewLine);
                intPct = (((4 * i) + 3) * 100) / (intTotalStepCount);
                bgw.ReportProgress(intPct, i);

                #region STEP 4: TRACE IP
                try
                {
                    var lstTracert = objUtil.Tracert(serverIP, 30, 3000).ToList();
                    foreach (Tracert objTrace in lstTracert)
                    {
                        strResult.Append(objTrace.ToString() + Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    strResult.Append("Unable to Trace IP due to: " + ex.Message + Environment.NewLine);
                }
                #endregion

                strResult.Append(Environment.NewLine + "TELNET " + strCheckServer + Environment.NewLine);
                intPct = (((4 * i) + 4) * 100) / (intTotalStepCount);
                bgw.ReportProgress(intPct, i);

                #region STEP 5: TELNET CONNECTION
                try
                {
                    foreach (int port in lstPort)
                    {
                        try
                        {
                            strTelnetResult = objUtil.CheckTelnetConnection(strCheckServer, port);
                        }
                        catch (Exception ex)
                        {
                            strTelnetResult = ex.Message;
                        }

                        strResult.Append(strCheckServer + " " + port + " " + strTelnetResult + Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    strResult.Append("Unable to Telnet server due to: " + ex.Message + Environment.NewLine);
                }

                strResult.Append(Environment.NewLine + Environment.NewLine);
                #endregion

                intPct = (((4 * i) + 5) * 100) / (intTotalStepCount);
                bgw.ReportProgress(intPct, i);
            }
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
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to save due to: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("! Wait - Process is running.");
            }
        }

        private Server SelectBestServer()
        {
            string strLocalCountry = RegionInfo.CurrentRegion.DisplayName;
            var servers = settings.Servers.Where(s => s.Country.Equals(DefaultCountry)).Take(10).ToList();

            foreach (var server in servers)
            {
                server.Latency = client.TestServerLatency(server);
            }
            return servers.OrderBy(x => x.Latency).First();
        }

        private string FormatSpeed(string type, double speed)
        {
            if (speed > 1024)
            {
                return string.Format("{0} speed: {1} Mbps", type, Math.Round(speed / 1024, 2));
            }
            else
            {
                return string.Format("{0} speed: {1} Kbps", type, Math.Round(speed, 2));
            }
        }

        private void BindCountryDropdown()
        {
            string[] lstCountry = new string[]
            {
                "Abkhazia",
                "Abkhaziya",
                "Afghanistan",
                "Albania",
                "Algeria",
                "American Samoa",
                "Andorra",
                "Angola",
                "Anguilla",
                "Antigua and Barbuda",
                "Argentina",
                "Armenia",
                "Aruba",
                "Australia",
                "Austria",
                "Azerbaijan",
                "Bahamas",
                "Bahrain",
                "Bangladesh",
                "Barbados",
                "Belarus",
                "Belgium",
                "Belguim",
                "Belize",
                "Benin",
                "Bermuda",
                "Bhutan",
                "Bolivia",
                "Bonaire",
                "Bosnia and Herzegovina",
                "Botswana",
                "Brasil",
                "Brazil",
                "Brunei",
                "Bulgaria",
                "Burkina Faso",
                "Cabo Verde",
                "Cambodia",
                "Cameroon",
                "Canada",
                "Cape Verde",
                "Cayman Islands",
                "Chile",
                "China",
                "Colombia",
                "Congo",
                "Congo, the Democratic Republic of the",
                "Cook Islands",
                "Costa Rica",
                "Croatia",
                "Cyprus",
                "Czech republic",
                "Czech Republic",
                "Ð Ð¾ÑÑÐ¸Ð¹ÑÐºÐ°Ñ Ð¤ÐµÐ´ÐµÑ€Ð°Ñ†Ð¸Ñ",
                "Ð£ÐºÑ€Ð°Ð¸Ð½Ð°",
                "Denmark",
                "Djibouti",
                "Dominica",
                "Dominican Republic",
                "DR Congo",
                "Ecuador",
                "Egypt",
                "Ekaterinburg",
                "El Salvador",
                "Estonia",
                "Ethiopia",
                "Faroe Islands",
                "Fiji",
                "Finland",
                "FR",
                "France",
                "Francef",
                "French Guiana",
                "French Polynesia",
                "Gabon",
                "Gambia",
                "Georgia",
                "Germany",
                "Ghana",
                "Gibraltar",
                "Great Britain",
                "Greece",
                "Greenland",
                "Guadeloupe",
                "Guam",
                "Guatemala",
                "Guernsey",
                "Guinea",
                "Guinea-Bissau",
                "Guyana",
                "Haiti",
                "Honduras",
                "Hungary",
                "Iceland",
                "India",
                "Indonesia",
                "Iran",
                "Iran, Islamic Republic of",
                "Iraq",
                "Ireland",
                "Isle of Man",
                "Israel",
                "Italia",
                "Italy",
                "Ivory Coast",
                "Jamaica",
                "Japan",
                "Jersey",
                "Jordan",
                "Kazakhstan",
                "Kenya",
                "Kiribati",
                "Kosovo",
                "Kuwait",
                "Kyrgyzstan",
                "Lao PDR",
                "Latvia",
                "Lebanon",
                "Lesotho",
                "Liberia",
                "Libya",
                "Liechtenstein",
                "Lithuania",
                "Lithuanua",
                "Luxembourg",
                "Macau",
                "Macedonia",
                "Madagascar",
                "Malawi",
                "Malaysia",
                "Mali",
                "Malta",
                "Marshall Islands",
                "Martinique",
                "Mauritania",
                "Mauritius",
                "Mexico",
                "Moldova",
                "Monaco",
                "Mongolia",
                "Montenegro",
                "Morocco",
                "Mozambique",
                "Myanmar",
                "Namibia",
                "Nepal",
                "Netherland",
                "Netherlands",
                "Netherlands Antilles",
                "New Caledonia",
                "New Zealand",
                "Nicaragua",
                "Nigeria",
                "Northern Mariana",
                "Norway",
                "Oman",
                "Pakistan",
                "Palestine",
                "Panama",
                "Papua New Guinea",
                "Paraguay",
                "PerÃº",
                "Peru",
                "Philippines",
                "Poland",
                "Portugal",
                "Puerto Rico",
                "Qatar",
                "RÃ©union",
                "Republic of Maldives",
                "Republic of Moldova",
                "Republic of Singapore",
                "Reunion",
                "Romania",
                "Russia",
                "Russian Federation",
                "Rwanda",
                "Saint Kitts and Nevis",
                "Samoa",
                "Sao Tome and Principe",
                "Saudi Arabia",
                "Senegal",
                "Serbia",
                "Seychelles",
                "Sierra Leone",
                "Singapore",
                "Sint Maarten",
                "Slovakia",
                "Slovenia",
                "Solomon Islands",
                "Somalia",
                "South Africa",
                "South Korea",
                "South Sudan",
                "Spain",
                "Sri Lanka",
                "Sudan",
                "Suriname",
                "Sweden",
                "Switzerland",
                "Syria",
                "Taiwan",
                "Tajikistan",
                "Tanzania",
                "Tanzania, United Republic of",
                "Thailand",
                "Timor-Leste",
                "Togo",
                "Trinidad",
                "Trinidad and Tobago",
                "Tunisia",
                "Turkey",
                "Turkmenistan",
                "Turks & Caicos",
                "Uganda",
                "Ukraine",
                "United Arab Emirates",
                "United Kingdom",
                "United States",
                "Uruguay",
                "Uzbekistan",
                "Vanuatu",
                "Venezuela",
                "Venezuela, Bolivarian Republic of",
                "Viet Nam",
                "Vietnam",
                "Virgin Islands",
                "Wales",
                "Zambia",
                "Zimbabwe"
            };

            ddlCountry.DataSource = lstCountry;
        }

        private void frmPingIPs_Shown(object sender, EventArgs e)
        {
            try
            {
                client = new SpeedTestClient();
                settings = client.GetSettings();
                isSettingsLoaded = true;
            }
            catch (Exception ex)
            {
                isSettingsLoaded = false;
                MessageBox.Show("Unable to load settings due to: " + ex.Message);
            }

            if (settings != null && settings.Servers != null && settings.Servers.Count() > 0)
            {
                var strClosestCountry = settings.Servers.OrderBy(x => x.Distance).FirstOrDefault().Country;
                ddlCountry.Text = strClosestCountry;
                DefaultCountry = strClosestCountry;
            }
        }

        private List<PingServerDetail> GetServerList()
        {
            List<PingServerDetail> lstServer = new List<PingServerDetail>();
            lstServer.Add(new PingServerDetail() { ServerIP = "57.191.128.244", ServerName = "americas.aticloud.aero", HostURL = "https://americas.aticloud.aero/vpn/index.html" });
            lstServer.Add(new PingServerDetail() { ServerIP = "57.241.128.244", ServerName = "americas-can.aticloud.aero", HostURL = "https://americas-can.aticloud.aero" });
            lstServer.Add(new PingServerDetail() { ServerIP = "57.255.52.37", ServerName = "americas-pss.aticloud.aero", HostURL = "https://americas-pss.aticloud.aero" });
            lstServer.Add(new PingServerDetail() { ServerIP = "57.230.88.32", ServerName = "prod.horizon.sita.aero", HostURL = "https://prod.horizon.sita.aero" });
            lstServer.Add(new PingServerDetail() { ServerIP = "57.230.80.200", ServerName = "hm.horizon.dc.sita.aero", HostURL = "https://hm.horizon.dc.sita.aero" });

            return lstServer;
        }
    }
}
