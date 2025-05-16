using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProtocolloTcp
{
    class Server
    {
        private static readonly string[] AllQuestions =
        {
            "Qual e' la capitale della Svizzera?",
            "In che scuderia corre Leclerc?",
            "In quale anno e' stata fondata Microsoft?",
            "Quanti continenti ci sono nel mondo?",
            "Qual e' il fiume piu' lungo d'Italia?"
        };

        private static readonly string[] AllAnswers =
        {
            "berna",
            "ferrari",
            "1975",
            "7",
            "po"
        };

        public static int Main(string[] args)
        {
            StartServer();
            return 0;
        }

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

                Console.WriteLine("In attesa di connessioni...");

                while (true)
                {
                    Socket handler = listener.Accept();
                    Console.WriteLine("Client connesso!");

                    try
                    {
                        byte[] buffer = new byte[1024];

                        int bytesRec = handler.Receive(buffer);
                        string playChoice = Encoding.ASCII.GetString(buffer, 0, bytesRec).Replace("<EOF>", "");

                        if (playChoice.ToLower() != "si")
                        {
                            Console.WriteLine("Client non vuole giocare. Chiudo connessione.");
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            continue;
                        }

                        bytesRec = handler.Receive(buffer);
                        int numQuestions = int.Parse(Encoding.ASCII.GetString(buffer, 0, bytesRec).Replace("<EOF>", ""));

                        if (numQuestions < 1) numQuestions = 1;
                        if (numQuestions > 5) numQuestions = 5;

                        Console.WriteLine($"Client vuole {numQuestions} domande");

                        int score = 0;
                        Random rnd = new Random();

                        for (int i = 0; i < numQuestions; i++)
                        {
                            int qIndex = rnd.Next(AllQuestions.Length);
                            string question = AllQuestions[qIndex];
                            string correctAnswer = AllAnswers[qIndex];

                            string questionMessage = question + "<EOF>";
                            byte[] msg = Encoding.ASCII.GetBytes(questionMessage);
                            handler.Send(msg);
                            Console.WriteLine($"Inviata domanda: {question}");

                            bytesRec = handler.Receive(buffer);
                            string answer = Encoding.ASCII.GetString(buffer, 0, bytesRec)
                                .Replace("<EOF>", "").Trim().ToLower();
                            Console.WriteLine($"Ricevuta risposta: {answer}");

                            if (answer == correctAnswer)
                            {
                                score++;
                            }
                        }

                        string scoreMessage = $"Il tuo punteggio finale e': {score} su {numQuestions}<EOF>";
                        byte[] scoreMsg = Encoding.ASCII.GetBytes(scoreMessage);
                        handler.Send(scoreMsg);
                    }
                    finally
                    {
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Errore: {e.Message}");
            }
        }
    }
}