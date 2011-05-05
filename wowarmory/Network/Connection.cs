using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace wowarmory.Network {
    public class Connection {
        public delegate void OnConnectionStateChangeDelegate();
        public delegate void OnResponseReceivedDelegate(Response response);

        public event OnConnectionStateChangeDelegate OnConnectionClosed;
        public event OnResponseReceivedDelegate OnResponseReceived;

        string host;
        int port;

        bool closed = false;

        TcpClient client = new TcpClient();
        Queue<Request> requestQueue = new Queue<Request>();

        public Connection(string host = "m.eu.wowarmory.com", int port = 8780) {
            this.host = host;
            this.port = port;
        }

        public void Start() {
            closed = false;
            client.Connect(host, port);

            new Thread(new ThreadStart(HandleRequests)).Start();
            new Thread(new ThreadStart(HandleResponses)).Start();
        }

        public void Close(string reason = "") {
            if (closed)
                return;

            client.Close();

            if (OnConnectionClosed != null)
                OnConnectionClosed();
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
                while (client.Connected) {
                    var response = Response.Parse(reader);

                    if (OnResponseReceived != null)
                        OnResponseReceived(response);
                }
            } catch (Exception e) {
                Close(e.Message);
            }
        }

        void HandleRequests() {
            var writer = new BinaryWriter(client.GetStream());

            Request request;
            while (client.Connected) {
                lock (requestQueue) {
                    while (requestQueue.Count == 0)
                        Monitor.Wait(requestQueue);
                    request = requestQueue.Dequeue();
                }

                if (client.Connected)
                    request.WriteTo(writer);
            }
        }
    }
}
