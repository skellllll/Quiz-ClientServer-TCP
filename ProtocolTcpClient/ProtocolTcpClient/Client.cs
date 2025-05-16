using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProtocolTcpClient
{
    class Client
    {
        public static int Main(string[] args)
        {
            StartClient();
            return 0;
        }

        public static void StartClient()
        {
            byte[] buffer = new byte[1024];

            try
            {
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                using (Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    sender.Connect(remoteEP);
                    Console.WriteLine("Connesso al server!");

                    Console.Write("Vuoi giocare al quiz? (si/no): ");
                    string playChoice = Console.ReadLine().ToLower();

                    byte[] msg = Encoding.ASCII.GetBytes(playChoice + "<EOF>");
                    sender.Send(msg);

                    if (playChoice != "si")
                    {
                        Console.WriteLine("OK, alla prossima!");
                        return;
                    }

                    int numQuestions;
                    do
                    {
                        Console.Write("Quante domande vuoi? (1-5): ");
                    } while (!int.TryParse(Console.ReadLine(), out numQuestions) || numQuestions < 1 || numQuestions > 5);

                    msg = Encoding.ASCII.GetBytes(numQuestions.ToString() + "<EOF>");
                    sender.Send(msg);

                    Console.WriteLine("\nIniziamo il quiz!\n");

                    for (int i = 0; i < numQuestions; i++)
                    {
                        int bytesRec = sender.Receive(buffer);
                        string question = Encoding.ASCII.GetString(buffer, 0, bytesRec).Replace("<EOF>", "");
                        Console.WriteLine($"Domanda {i + 1}: {question}");

                        Console.Write("La tua risposta: ");
                        string answer = Console.ReadLine();
                        msg = Encoding.ASCII.GetBytes(answer + "<EOF>");
                        sender.Send(msg);
                    }

                    int finalBytesRec = sender.Receive(buffer);
                    string score = Encoding.ASCII.GetString(buffer, 0, finalBytesRec).Replace("<EOF>", "");
                    Console.WriteLine($"\n{score}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Errore: {e.Message}");
            }

            Console.WriteLine("\nPremi un tasto per uscire...");
            Console.ReadKey();
        }
    }
}