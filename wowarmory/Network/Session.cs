using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wowarmory.SRP;
using System.Diagnostics;
using System.Timers;
using System.Security.Cryptography;

namespace wowarmory.Network {
    public class Session {
        public delegate void OnSessionStateChangeDelegate();

        public event OnSessionStateChangeDelegate OnSessionEstablished;
        public event OnSessionStateChangeDelegate OnSessionClosed;
        public event Connection.OnResponseReceivedDelegate OnResponseReceived;

        public readonly Connection Connection;
        ClientSRP srp;
        int stage = 0;

        string accountName, password;

        public Session() {
            Connection = new Connection();

            Connection.OnConnectionClosed += new Network.Connection.OnConnectionStateChangeDelegate(OnConnectionClosed);
            Connection.OnResponseReceived += new Network.Connection.OnResponseReceivedDelegate(OnReceiveResponse);

            Trace.Listeners.Add(new TextWriterTraceListener("packets.txt"));
            Trace.AutoFlush = true;
        }

        public void Start(string accountName, string password) {
            this.accountName = accountName;
            this.password = password;

            Connection.Start();

            srp = new ClientSRP();

            var request = new Request("/authenticate1");
            var Abytes = srp.GetChallengeA();

            // TODO: "lol". 40-byte deviceId
            //var hash = MD5.Create().ComputeHash(Encoding.Default.GetBytes(accountName)).ToHexString();
            //var deviceId = hash + hash.Substring(0, 8);

            request["screenRes"] = "PHONE_MEDIUM";
            request["device"] = "iPhone";
            request["deviceSystemVersion"] = "4.0";
            request["deviceModel"] = "iPod2,1";
            request["appV"] = "3.0.0";
            request["deviceTime"] = (int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
            request["deviceTimeZoneId"] = "Europe/Berlin";
            request["clientA"] = Abytes;
            request["appId"] = "Armory";
            //request["deviceId"] = deviceId; // everything is fine without sending one so let's do that
            request["emailAddress"] = accountName;
            request["deviceTimeZone"] = "7200000";
            request["locale"] = "en_GB";

            /*request["device"] = "Android";
            request["deviceModel"] = "sdk";
            request["deviceSystemVersion"] = "3.0";
            request["screenRes"] = "PHONE_MEDIUM";
            request["accountName"] = accountName.ToUpperInvariant();
            request["deviceTimeZoneId"] = "GMT";
            request["appV"] = "2.0.1";
            request["deviceTime"] = ((int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds).ToString();
            request["clientA"] = Abytes;
            request["deviceId"] = "b00677852e37e8e3"; // TODO: hash account name for this?
            request["appId"] = "AuctionHouse";
            request["deviceTimeZone"] = "0";
            request["locale"] = "en_US";*/

            stage = 1;

            Connection.SendRequest(request);
        }

        public void Close() {
            Connection.Close();
        }

        public void OnReceiveResponse(Response response) {
            Trace.WriteLine("Response: " + response.Target);
            Trace.WriteLine("Status: " + response.Status);
            Trace.WriteLine(response.Format());
            Trace.WriteLine("-".Multiply(80));

            if (response.Status != 200) { // TODO: handle these
                Console.WriteLine("error: " + response["body"]);
                return;
            }

            if (stage == 1 && response.Target == "/authenticate1") {
                Console.WriteLine("sending client proof");
                var bbytes = (byte[])response["B"];
                var salt = (byte[])response["salt"];
                var user = (byte[])response["user"];
                var userHash = Encoding.ASCII.GetString(user);
                var password = FormatPassword();
                var proof = srp.CalculateAuth1Proof(userHash, password, salt, bbytes);
                var request = new Request("/authenticate2");
                request["clientProof"] = proof;

                stage = 2;

                Connection.SendRequest(request);
            } else if (stage == 2 && response.Target == "/authenticate2") {
                Console.WriteLine("sending client proof2");
                var request = new Request("/authenticate2");
                request["clientProof"] = new byte[0];

                stage = 3;
                Connection.SendRequest(request);
            } else if (stage == 3) {
                if (OnSessionEstablished != null)
                    OnSessionEstablished();

                stage = 4;
            }
            
            if (stage == 4) {
                if (OnResponseReceived != null)
                    OnResponseReceived(response);
            }
        }

        string FormatPassword() {
            var newStr = password;
            if (password.Length > 16)
                password.Substring(0, 16);
            return password.ToUpperInvariant();
        }

        public void OnConnectionClosed() {
            if (OnSessionClosed != null)
                OnSessionClosed();

            stage = 0;
        }
    }
}
