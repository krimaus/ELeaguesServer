﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ELeaguesServer.Models;


namespace ELeaguesServer
{

    class Program
    {

        static void Main(string[] args)
        {
            ExecuteServer();
        }

        public static void ExecuteServer()
        {
            // Establish the local endpoint
            // for the socket. Dns.GetHostName
            // returns the name of the host
            // running the application.
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 23177);

            // Creation TCP/IP Socket using
            // Socket Class Constructor
            Socket listener = new Socket(ipAddr.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

            try
            {

                // Using Bind() method we associate a
                // network address to the Server Socket
                // All client that will connect to this
                // Server Socket must know this network
                // Address
                listener.Bind(localEndPoint);

                // Using Listen() method we create
                // the Client list that will want
                // to connect to Server
                listener.Listen(10);

                while (true)
                {

                    Console.WriteLine("Waiting connection ... ");

                    // Suspend while waiting for
                    // incoming connection Using
                    // Accept() method the server
                    // will accept connection of client
                    Socket clientSocket = listener.Accept();

                    // Data buffer
                    byte[] bytes = new Byte[1024];
                    string commString = "";

                    while (true)
                    {

                        int numByte = clientSocket.Receive(bytes);

                        commString += Encoding.ASCII.GetString(bytes,
                                                0, numByte);

                        if (commString.IndexOf("<EOF>") > -1)
                            break;
                    }

                    Console.WriteLine("Text received -> {0} ", commString);
                    
                    //splitting recieved message by ':'
                    string[] separatedCommStringParts = commString.Split(':');
                    
                    //checking and executing client request
                    byte[] message;
                    string tempServerReply;

                    switch (separatedCommStringParts[0])
                    {
                        case "ca":
                            if (CreateAccount(separatedCommStringParts)) message = Encoding.ASCII.GetBytes("sr:approved");
                            else message = Encoding.ASCII.GetBytes("sr:disapproved");
                            break;
                        case "cl":
                            if (CreateLeague(separatedCommStringParts)) message = Encoding.ASCII.GetBytes("sr:approved");
                            else message = Encoding.ASCII.GetBytes("sr:disapproved");
                            break;
                        case "ct":
                            if (CreateTourney(separatedCommStringParts)) message = Encoding.ASCII.GetBytes("sr:approved");
                            else message = Encoding.ASCII.GetBytes("sr:disapproved");
                            break;
                        case "ap":
                            if (AddPlayer(separatedCommStringParts)) message = Encoding.ASCII.GetBytes("sr:approved");
                            else message = Encoding.ASCII.GetBytes("sr:disapproved");
                            break;
                        case "cm":
                            if (CreateMatch(separatedCommStringParts)) message = Encoding.ASCII.GetBytes("sr:approved");
                            else message = Encoding.ASCII.GetBytes("sr:disapproved");
                            break;
                        case "em":
                            if (EditMatch(separatedCommStringParts)) message = Encoding.ASCII.GetBytes("sr:approved");
                            else message = Encoding.ASCII.GetBytes("sr:disapproved");
                            break;
                        case "sq":
                            tempServerReply = ServerQuery(separatedCommStringParts);
                            if (tempServerReply != "sr:disapproved") message = Encoding.ASCII.GetBytes(tempServerReply);
                            else message = Encoding.ASCII.GetBytes("sr:disapproved");
                            break;
                        default:
                            message = Encoding.ASCII.GetBytes("sr:disapproved");
                            break;
                    }

                    // Send a message to Client
                    // using Send() method
                    clientSocket.Send(message);

                    // Close client Socket using the
                    // Close() method. After closing,
                    // we can use the closed Socket
                    // for a new Client Connection
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //metoda obsługująca rozpoznawanie rodzaju zapytania od klienta
        public static string ServerQuery(string[] separatedCommStringParts)
        {
            string reply;

            switch (separatedCommStringParts[1])
            {
                case "isadmin":
                    reply = IsAdminQuery(separatedCommStringParts);
                    break;
                case "logincheck":
                    reply = LoginCheck(separatedCommStringParts);
                    break;
                case "alltourneys":
                    reply = AllTourneys(separatedCommStringParts);
                    break;
                case "mytourneys":
                    reply = MyTourneys(separatedCommStringParts);
                    break;
                default:
                    reply = "sr:disapproved";
                    break;
            }

            return reply;
        }

        // metody wysyłające zapytania do bazy danych
        public static string IsAdminQuery(string[] separatedCommStringParts) 
        {
            bool isAdmin = false;
            //zapytanie czy separatedCommStringParts[2] jest adminem
            if (isAdmin) return "sr:isadmin";
            else return "sr:isnotadmin";
        }

        public static string LoginCheck(string[] separatedCommStringParts)
        {
            bool loginCheck = false;
            //check if username exists in database and password is correct
            if (loginCheck) return "sr:approved";
            else return "sr:disapproved";
        }

        public static string AllTourneys(string[] separatedCommStringParts)
        {
            string allTourneys = "";
            //zapytanie o wszystkie turnieje, dopisywanie kolejnych nazw do stringa w pętli (dzielić nazwy ":")
            return allTourneys;
        }

        public static string MyTourneys(string[] separatedCommStringParts)
        {
            string myTourneys = "";
            //zapytanie o turnieje do których zapisany jest zawodnik o nazwie separatedCommStringParts[2], dopisywanie kolejnych nazw do stringa w pętli (dzielić nazwy ":")
            return myTourneys;
        }

        // metody modyfikujące bazę danych
        public static bool CreateAccount(string[] separatedCommStringParts)
        {

            bool createAccount = false;
            using(var db = new KrzmauContext())
            {
                //add new user to database if username is free
                if (!db.Uzytkownicies.Where(u => u.Nazwa.Equals(separatedCommStringParts[1])).Any())
                {
                    var newUser = new Uzytkownicy { Nazwa = separatedCommStringParts[1], Haslo = separatedCommStringParts[2],
                        Administrator = (separatedCommStringParts[3] == "true") ? true : false};
                    db.Add<Uzytkownicy>(newUser);
                    db.SaveChanges();
                    createAccount = true;
                }
            }
            // todo: add check to see if no extra ':' are present
            return createAccount;    
        }

        public static bool CreateLeague(string[] separatedCommStringParts)
        {
            if (separatedCommStringParts[0] == "cl")
            {
                //add new league to database
                return true;
            }
            else return false;
        }

        public static bool CreateTourney(string[] separatedCommStringParts)
        {
            if (separatedCommStringParts[0] == "ct")
            {
                // add new Tourney to database, use CreateMatch to generate an empty bracket
                return true;
            }
            else return false;
        }

        public static bool AddPlayer(string[] separatedCommStringParts)
        {
            if (separatedCommStringParts[0] == "ap")
            {
                //add new player to database
                return true;
            }
            else return false;
        }

        public static bool CreateMatch(string[] separatedCommStringParts)
        {
            if (separatedCommStringParts[0] == "cm")
            {
                //add new match to database
                return true;
            }
            else return false;
        }

        public static bool EditMatch(string[] separatedCommStringParts)
        {
            if (separatedCommStringParts[0] == "em")
            {
                //edit an existing match
                return true;
            }
            else return false;
        }
    }
}
