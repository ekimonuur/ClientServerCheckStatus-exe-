using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProcessCheckerGateway
{
    class Program
    {
        static Client server1 = new Client("localhost", 11000);
        static Client server2 = new Client("localhost", 11000);

        static void Main(string[] args)
        {
            Console.WriteLine("Gateway");

            server1.Start();
            //server2.Start();

            StartGateway();
        }

        public static void StartGateway()
        {
            // Get Host IP Address that is used to establish a connection  
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
            // If a host has multiple addresses, you will get a list of addresses  
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11001);


            try
            {

                // Create a Socket that will use Tcp protocol      
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method  
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.  
                // We will listen 1 requests at a time  
                listener.Listen(1);

                Console.WriteLine("Waiting for client...");
                Socket handler = listener.Accept();
                Console.WriteLine("Client connected.");
                // Incoming data from the client.    
                string data = null;

                //buffer
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        //status notepad S1S2<EOF>
                        data = data.Substring(0, data.Length - 5);

                        Console.WriteLine("Command received : {0}", data);

                        var parts = data.Split(' ');
                        var result1 = "";
                        var result2 = "";

                        if (parts[2].Contains("S1"))
                        {
                            result1 = server1.SendAndReceive(parts[0] + " " + parts[1]);
                            //{"S1":"ok"}
                            if (result1.Contains("ok"))
                                result1 = "ok";
                            else if (result1.Contains("error"))
                                result1 = "error";
                            else
                                result1 = "";
                        }

                        if (parts[2].Contains("S2"))
                        {
                            result2 = server2.SendAndReceive(parts[0] + " " + parts[1]);
                            //{"S1":"ok"}
                            if (result2.Contains("ok"))
                                result2 = "ok";
                            else if (result2.Contains("error"))
                                result2 = "error";
                            else
                                result2 = "";
                        }

                        //{"S1":"ok","S2":"error"}
                        var response = $"{{\"S1\":\"{result1}\",\"S2\":\"{result2}\"}}";

                        byte[] msg = Encoding.ASCII.GetBytes(response + "<EOF>");

                        //handler burada bize bağlanan ilk komutu gönderen client ile gateway arasındaki socket bağlantısını temsil ediyor
                        handler.Send(msg);

                        Console.WriteLine("Response sent: {0}", response);
                        data = "";
                    }
                }


                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
