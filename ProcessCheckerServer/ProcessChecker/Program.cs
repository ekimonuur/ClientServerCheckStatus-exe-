using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProcessChecker
{
    class Program
    {
        //Server'ın adı S1 veya S2 açılışta soruluyor
        static string name;

        static void Main(string[] args)
        {
            Console.WriteLine("Server");
            Console.WriteLine("Enter server name");

            //Server'ın adının girilmesi isteniyor
            //Bunun amacı geriye döndürülen cevabın içine sunucunun adının doğru yazılması için
            name = Console.ReadLine();

            StartServer();
        }

        public static void StartServer()
        {
            // Get Host IP Address that is used to establish a connection  
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
            // If a host has multiple addresses, you will get a list of addresses  
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);


            try
            {

                // Create a Socket that will use Tcp protocol      
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method  
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.  
                // We will listen 1 requests at a time  
                listener.Listen(1);

                Console.WriteLine("Waiting for gateway...");
                Socket handler = listener.Accept();
                Console.WriteLine("Gateway connected.");
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
                        //status notepad<EOF>
                        data = data.Substring(0, data.Length - 5);

                        Console.WriteLine("Command received : {0}", data);

                        var parts = data.Split(' ');

                        var processList = Process.GetProcessesByName(parts[1]);

                        var result = "error";
                        if (processList.Length > 0)
                        {
                            result = "ok";
                        }

                        //{"S1":"ok"}
                        var response = $"{{\"{name}\":\"{result}\"}}";

                        byte[] msg = Encoding.ASCII.GetBytes(response + "<EOF>");
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
