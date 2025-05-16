using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProtocolloTcp
{
    class Server
    {
        public static int Main(String[] args)
        {
            StartServer();
            return 0;
        }
        private static readonly string[] Questions =
        {
            "Qual e' la capitale della Svizzera?",
            "In che scuderia corre Leclerc?",
            "In quale anno e' stata fondata Microsoft?"
        };

        private static readonly string[] Answers =
        {
            "berna",
            "ferrari",
            "1975"
        };
        public static void StartServer()
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            try
            {
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();

                int score = 0;
                byte[] bytes = new byte[1024];

                for (int i = 0; i < Questions.Length; i++)
                {
                    string questionMessage = Questions[i] + "<EOF>";
                    byte[] msg = Encoding.ASCII.GetBytes(questionMessage);
                    handler.Send(msg);
                    Console.WriteLine($"Sent question: {Questions[i]}");

                    int bytesRec = handler.Receive(bytes);
                    string answer = Encoding.ASCII.GetString(bytes, 0, bytesRec).Replace("<EOF>", "").Trim().ToLower();
                    Console.WriteLine($"Received answer: {answer}");

                    if (answer == Answers[i])
                    {
                        score++;
                    }
                }

                string scoreMessage = $"Il tuo punteggio finale e': {score} su {Questions.Length}<EOF>";
                byte[] scoreMsg = Encoding.ASCII.GetBytes(scoreMessage);
                handler.Send(scoreMsg);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
