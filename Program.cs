using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
                            tempServerReply = CreateLeague(separatedCommStringParts);
                            if (tempServerReply != "sr:disapproved") message = Encoding.ASCII.GetBytes(tempServerReply);
                            else message = Encoding.ASCII.GetBytes("sr:disapproved");
                            break;
                        case "ct":
                            tempServerReply = CreateTourney(separatedCommStringParts);
                            if (tempServerReply != "sr:disapproved") message = Encoding.ASCII.GetBytes(tempServerReply);
                            else message = Encoding.ASCII.GetBytes("sr:disapproved");
                            break;
                        case "cm":
                            tempServerReply = CreateMatch(separatedCommStringParts);
                            if (tempServerReply != "sr:disapproved") message = Encoding.ASCII.GetBytes(tempServerReply);
                            else message = Encoding.ASCII.GetBytes("sr:disapproved");
                            break;
                        case "em":
                            tempServerReply = EditMatch(separatedCommStringParts);
                            if (tempServerReply != "sr:disapproved") message = Encoding.ASCII.GetBytes(tempServerReply);
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
                case "lastusedleague":
                    reply = LastUsedLeague(separatedCommStringParts);
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
            using (var db = new KrzmauContext())
            {
                if (db.Uzytkownicies.Where(u => u.Administrator.Equals(separatedCommStringParts[2])).Any()) isAdmin = true;
            }
            if (isAdmin) return "sr:approved";
            else return "sr:disapproved";
        }

        // todo: fuse LoginCheck with IsAdmin 
        public static string LoginCheck(string[] separatedCommStringParts)
        {
            bool loginCheck = false;
            //check if username exists in database and password is correct
            using (var db = new KrzmauContext())
            {
                var user = db.Uzytkownicies.SingleOrDefault(u => u.Nazwa.Equals(separatedCommStringParts[2]));
                if (user.Haslo.Equals(separatedCommStringParts[3]) && !user.Equals(null)) loginCheck = true;
            }
            if (loginCheck) return "sr:approved";
            else return "sr:disapproved";
        }

        public static string AllTourneys(string[] separatedCommStringParts)
        {
            string allTourneys = "sr";
            //zapytanie o wszystkie turnieje, dopisywanie kolejnych nazw do stringa w pętli (dzielić nazwy ":")
            using (var db = new KrzmauContext())
            {
                foreach (var tourney in db.Turniejes)
                {
                    allTourneys += ":" + tourney.Idturnieju;
                }
            }
            return allTourneys;
        }

        public static string MyTourneys(string[] separatedCommStringParts)
        {
            string myTourneys = "sr";
            //zapytanie o turnieje do których zapisany jest zawodnik o nazwie separatedCommStringParts[2]
            using (var db = new KrzmauContext())
            {
                var user = db.Uzytkownicies.Single(u => u.Nazwa.Equals(separatedCommStringParts[2]));
                var matches = db.Meczes.Where(m => m.Idzawodnikajeden.Equals(user.Iduzytkownika) || m.Idzawodnikadwa.Equals(user.Iduzytkownika));
                // todo: switch from list to set?
                List<int?> tourneyIds = new();
                foreach (var match in matches)
                {
                    if (tourneyIds.Contains(match.Idturnieju))
                    {
                        tourneyIds.Add(match.Idturnieju);
                        myTourneys += ":" + match.Idturnieju;
                    }
                }
            }

            return myTourneys;
        }

        public static string LastUsedLeague(string[] separatedCommStringParts)
        {
            string lastUsed = "sr:";
            using (var db = new KrzmauContext())
            {
                var userLeagues = db.Ligis.Where(l => l.Idwlasciciela.Equals(separatedCommStringParts[2]));
                lastUsed += userLeagues.Max(m => m.Idligi).ToString();
            }
            return lastUsed;
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
                    db.Add(newUser);
                    db.SaveChanges();
                    createAccount = true;
                }
            }
            // todo: add check to see if no extra ':' are present
            return createAccount;    
        }

        public static string CreateLeague(string[] separatedCommStringParts)
        {
            using (var db = new KrzmauContext())
            {
                if (!db.Uzytkownicies.Where(u => u.Nazwa.Equals(separatedCommStringParts[1])).Any())
                {
                    var newLeague = new Ligi { Idwlasciciela = db.Uzytkownicies.Single(u => u.Nazwa.Equals(separatedCommStringParts[1])).Iduzytkownika };
                    db.Add(newLeague);
                    db.SaveChanges();
                    return "sr:" + newLeague.Idligi.ToString();
                }
            }
            
            return "sr:disapproved";
        }

        public static string CreateTourney(string[] separatedCommStringParts)
        {
            // add new Tourney to database, use CreateMatch to generate an empty bracket
            using (var db = new KrzmauContext())
            {
                if (!db.Ligis.Where(u => u.Idligi.Equals(Int32.Parse(separatedCommStringParts[1]))).Any())
                {
                    var newTourney = new Turnieje { Idligi = Int32.Parse(separatedCommStringParts[1]) };
                    db.Add(newTourney);
                    db.SaveChanges();
                    return "sr:" + newTourney.Idturnieju.ToString();
                }
            }

            return "sr:disapproved";
        }

        public static string CreateMatch(string[] separatedCommStringParts)
        {
            using (var db = new KrzmauContext())
            {
                if (!db.Turniejes.Where(u => u.Idturnieju.Equals(Int32.Parse(separatedCommStringParts[1]))).Any())
                {
                    var newMatch = new Mecze { Idturnieju = Int32.Parse(separatedCommStringParts[1])};
                    db.Add(newMatch);
                    db.SaveChanges();
                    return "sr:" + newMatch.Idmeczu.ToString();
                }
            }

            return "sr:disapproved";
        }

        public static string EditMatch(string[] separatedCommStringParts)
        {
            // most logic heavy lifting goes on clientside
            // em:idmeczu:zaw1:zaw2:wyn1:wyn2:nastmecz
            using (var db = new KrzmauContext())
            {
                if (!db.Meczes.Where(u => u.Idmeczu.Equals(Int32.Parse(separatedCommStringParts[1]))).Any())
                {
                    var editedMatch = db.Meczes.Single(m => m.Idmeczu.Equals(separatedCommStringParts[1]));
                    if (separatedCommStringParts[2] != "empty") editedMatch.Idzawodnikajeden = Int32.Parse(separatedCommStringParts[2]);
                    if (separatedCommStringParts[3] != "empty") editedMatch.Idzawodnikadwa = Int32.Parse(separatedCommStringParts[3]);
                    if (separatedCommStringParts[4] != "empty") editedMatch.Wynikjeden = Int32.Parse(separatedCommStringParts[4]);
                    if (separatedCommStringParts[5] != "empty") editedMatch.Wynikdwa = Int32.Parse(separatedCommStringParts[5]);
                    if (separatedCommStringParts[6] != "empty") editedMatch.Idnastepnegomeczu = Int32.Parse(separatedCommStringParts[6]);
                    db.SaveChanges();
                    return "sr:" + editedMatch.Idmeczu.ToString();
                }
            }

            return "sr:disapproved";
        }
    }
}
