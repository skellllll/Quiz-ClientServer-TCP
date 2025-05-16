using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProtocolTcpClient
{
    class Client
    {
        public static int Main(String[] args)
        {
            StartClient();
            return 0;
        }

        public static void StartClient()
        {
            byte[] bytes = new byte[1024];

            try
            {
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    sender.Connect(remoteEP);
                    Console.WriteLine("Connesso al server...\n");

                    for (int i = 0; i < 3; i++)
                    {
                        // Ricevi la domanda
                        int bytesRec = sender.Receive(bytes);
                        string question = Encoding.ASCII.GetString(bytes, 0, bytesRec).Replace("<EOF>", "");
                        Console.WriteLine($"Domanda {i + 1}: {question}");

                        // Invia la risposta
                        Console.Write("La tua risposta: ");
                        string answer = Console.ReadLine();
                        string answerMessage = $"{answer}<EOF>";
                        byte[] msg = Encoding.ASCII.GetBytes(answerMessage);
                        sender.Send(msg);
                    }

                    // Ricevi il punteggio finale
                    int finalBytesRec = sender.Receive(bytes);
                    string score = Encoding.ASCII.GetString(bytes, 0, finalBytesRec).Replace("<EOF>", "");
                    Console.WriteLine($"\n{score}");

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
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

            Console.WriteLine("\nPremi un tasto per uscire...");
            Console.ReadKey();
        }
    }
}