using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace wowarmory.Network {
    public class Connection {
        public delegate void OnConnectionClosedDelegate(string reason);
        public delegate void OnResponseReceivedDelegate(Response response);
        public delegate void OnRequestSentDelegate(Request request);

        public event OnConnectionClosedDelegate OnConnectionClosed;
        public event OnResponseReceivedDelegate OnResponseReceived;
        public event OnRequestSentDelegate OnRequestSent;

        string host;
        int port;

        bool closed = true;

        TcpClient client;
        Queue<Request> requestQueue = new Queue<Request>();

        public Connection(string host = "m.eu.wowarmory.com", int port = 8780) {
            this.host = host;
            this.port = port;
        }

        public void Start() {
            Close();

            closed = false;
            client = new TcpClient();
            client.Connect(host, port);

            new Thread(new ThreadStart(HandleRequests)).Start();
            new Thread(new ThreadStart(HandleResponses)).Start();
        }

        public void Close(string reason = "") {
            if (IsClosed)
                return;

            closed = true;

            lock (requestQueue) {
                Monitor.Pulse(requestQueue);
            }

            client.Close();

            if (OnConnectionClosed != null)
                OnConnectionClosed(reason);
        }

        public bool IsClosed {
            get {
                return closed || !client.Connected;
            }
        }

        public void SendRequest(Request request) {
            lock (requestQueue) {
                requestQueue.Enqueue(request);
                Monitor.Pulse(requestQueue);
            }
        }

        void HandleResponses() {
            var reader = new BinaryReader(client.GetStream());

            try {
                while (!IsClosed) {
                    var response = Response.Parse(reader);

                    if (IsClosed)
                        return;

                    if (OnResponseReceived != null)
                        OnResponseReceived(response);
                }
            } catch (Exception e) {
                Close(e.Message);
            }
        }

        void HandleRequests() {
            var writer = new BinaryWriter(client.GetStream());

            while (!IsClosed) {
                Request request = null;
                lock (requestQueue) {
                    while (requestQueue.Count == 0 && !IsClosed)
                        Monitor.Wait(requestQueue);
                    if (requestQueue.Count > 0)
                        request = requestQueue.Dequeue();
                }

                if (!IsClosed && request != null) {
                    request.WriteTo(writer);

                    if (OnRequestSent != null)
                        OnRequestSent(request);
                }
            }
        }
    }
}
