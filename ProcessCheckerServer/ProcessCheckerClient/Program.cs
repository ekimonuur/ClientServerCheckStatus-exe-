using System;

namespace ProcessCheckerClient
{
    class Program
    {
        static Client gateway = new Client("localhost", 11001);
        static void Main(string[] args)
        {
            Console.WriteLine("Client");

            gateway.Start();

            Console.WriteLine("Connected to gateway");

            Console.WriteLine("Enter your command");

            var command = "";
            while (command != "exit")
            {
                var response = "";
                command = Console.ReadLine();
                if (command != "exit")
                {
                    response = gateway.SendAndReceive(command);
                    Console.WriteLine("Response: {0}", response);
                }
            }


        }
    }
}
