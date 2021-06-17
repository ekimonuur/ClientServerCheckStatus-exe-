using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProcessCheckerClient
{
    public class Client
    {
        string ip;
        int port;
        Socket sender;

        public Client(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

        }

        public string SendAndReceive(string command)
        {

            // Encode the data string into a byte array.    
            byte[] msg = Encoding.ASCII.GetBytes(command + "<EOF>");

            // Send the data through the socket.    
            int bytesSent = sender.Send(msg);

            // Incoming data from the client.    
            string data = null;
            byte[] bytes = null;
            while (true)
            {
                bytes = new byte[1024];
                int bytesRec = sender.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (data.IndexOf("<EOF>") > -1)
                {
                    //{"S1":"ok"}<EOF>
                    data = data.Substring(0, data.Length - 5);
                    return data;
                }
            }
        }

        public void Stop()
        {
            // Release the socket.    
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public void Start()
        {

            try
            {
                // Connect to a Remote server  
                // Get Host IP Address that is used to establish a connection  
                // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
                // If a host has multiple addresses, you will get a list of addresses  
                IPHostEntry host = Dns.GetHostEntry(ip);
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP  socket.    
                sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.    
                try
                {
                    // Connect to Remote EndPoint  
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
