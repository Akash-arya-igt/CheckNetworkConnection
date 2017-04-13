using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace PingIP
{
    public class CommonUtil
    {
        /// <summary>
        /// Traces the route which data have to travel through in order to reach an IP address.
        /// </summary>
        /// <param name="ipAddress">The IP address of the destination.</param>
        /// <param name="maxHops">Max hops to be returned.</param>
        public IEnumerable<Tracert> Tracert(string ipAddress, int maxHops, int timeout)
        {
            IPAddress address;

            // Ensure that the argument address is valid.
            if (!IPAddress.TryParse(ipAddress, out address))
                throw new ArgumentException(string.Format("{0} is not a valid IP address.", ipAddress));

            // Max hops should be at least one or else there won't be any data to return.
            if (maxHops < 1)
                throw new ArgumentException("Max hops can't be lower than 1.");

            // Ensure that the timeout is not set to 0 or a negative number.
            if (timeout < 1)
                throw new ArgumentException("Timeout value must be higher than 0.");


            Ping ping = new Ping();
            PingOptions pingOptions = new PingOptions(1, true);
            Stopwatch pingReplyTime = new Stopwatch();
            PingReply reply;

            do
            {
                pingReplyTime.Start();
                reply = ping.Send(address, timeout, new byte[] { 0 }, pingOptions);
                pingReplyTime.Stop();

                //string hostname = string.Empty;
                //if (reply.Address != null)
                //{
                //    try
                //    {
                //        hostname = Dns.GetHostByAddress(reply.Address).HostName;    // Retrieve the hostname for the replied address.
                //    }
                //    catch (SocketException) { /* No host available for that address. */ }
                //}

                // Return out TracertEntry object with all the information about the hop.
                yield return new Tracert()
                {
                    HopID = pingOptions.Ttl,
                    Address = reply.Address == null ? "N/A" : reply.Address.ToString(),
                    Hostname = ipAddress,
                    ReplyTime = pingReplyTime.ElapsedMilliseconds,
                    ReplyStatus = reply.Status
                };

                pingOptions.Ttl++;
                pingReplyTime.Reset();
            }
            while (reply.Status != IPStatus.Success && pingOptions.Ttl <= maxHops);
        }

        public string CheckTelnetConnection(string ip, int port)
        {
            string strResult = string.Empty;

            TcpClient tc = null;
            try
            {
                tc = new TcpClient(ip, port);
                strResult = "connected successfully!";
            }
            catch (Exception se)
            {
                strResult = se.Message;
            }
            finally
            {
                if (tc != null)
                {
                    tc.Close();
                }
            }

            return strResult;
        }
    }
}
