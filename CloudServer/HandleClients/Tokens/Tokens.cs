using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudServer.HandleClients
{
    public class Token
    {
        public string IP;
        public int ID;
        public string Username;
        public string AuthToken;
        public string LastDevice;
        public DateTime LastRequest;

        #region Gets / Sets
        public string ip
        {
            get
            {
                return IP;
            }
            set
            {
                IP = value;
            }
        }

        public string username
        {
            get
            {
                return Username;
            }
            set
            {
                Username = value;
            }
        }

        public string authtoken
        {
            get
            {
                return AuthToken;
            }
            set
            {
                AuthToken = value;
            }
        }

        public string lastdevice
        {
            get
            {
                return LastDevice;
            }
            set
            {
                LastDevice = value;
            }
        }

        public DateTime lastrequest
        {
            get
            {
                return LastRequest;
            }
            set
            {
                LastRequest = value;
            }
        }
        #endregion

        public Token(string IP, int ID, string Username, string AuthToken)
        {
            this.IP = IP;
            this.ID = ID;
            this.Username = Username;
            this.AuthToken = AuthToken;
            this.LastRequest = DateTime.Now;
        }

        public Token(string IP, int ID, string Username, string LastDevice, string AuthToken)
        {
            this.IP = IP;
            this.ID = ID;
            this.Username = Username;
            this.AuthToken = AuthToken;
            this.LastDevice = LastDevice;
            this.LastRequest = DateTime.Now;
        }
    }

    public class Tokens
    {
        public static List<Token> AuthTokens = new List<Token>();

        /// <summary>
        /// Checks for Token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool CheckToken(Token token)
        {
            List<Token> TempAuthTokens = new List<Token>();

            lock (AuthTokens)
            {
                TempAuthTokens = AuthTokens;
            }

            int index = 0;
            foreach (Token toke in TempAuthTokens)
            {
                if (toke.IP == token.IP)
                {
                    Console.WriteLine("Token IPs are the Same!");

                    if (toke.Username == token.Username)
                    {
                        Console.WriteLine("Token Usernames are the Same!");

                        if (toke.AuthToken == token.AuthToken)
                        {
                            lock (AuthTokens)
                            {
                                AuthTokens[index].LastRequest = DateTime.Now;
                            }

                            Console.WriteLine("Tokens are the Same!");

                            return true;
                        }
                    }
                }

                index++;
            }

            //Return false if not found
            return false;
        }

        /// <summary>
        /// Reutnr username associated with the token
        /// </summary>
        /// <param name="Roken"></param>
        /// <returns></returns>
        public static Token GetToken(string IP)
        {
            List<Token> tokens = new List<Token>();

            lock (AuthTokens)
            {
                tokens = AuthTokens;
            }

            int index = 0;
            foreach (Token token in tokens)
            {
                if (token.IP == IP)
                {
                    lock (AuthTokens)
                    {
                        tokens[index].LastRequest = DateTime.Now;
                    }

                    return token;
                }

                index++;
            }

            //Return false if not found
            return null;
        }

        /// <summary>
        /// Reutnr username associated with the token
        /// </summary>
        /// <param name="Roken"></param>
        /// <returns></returns>
        public static Token GetTokenByToken(string Token)
        {
            List<Token> tokens = new List<Token>();

            lock (AuthTokens)
            {
                tokens = AuthTokens;
            }

            int index = 0;
            foreach (Token token in tokens)
            {
                if (token.AuthToken == Token)
                {
                    lock (AuthTokens)
                    {
                        tokens[index].LastRequest = DateTime.Now;
                    }

                    return token;
                }

                index++;
            }

            //Return false if not found
            return null;
        }

        /// <summary>
        /// Generates Random Token
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string GenerateToken(string Ip, int ID, string Username)
        {
            //Check for Previous Tokens
            Token PreviousToken = GetToken(Ip);

            if (PreviousToken != null && PreviousToken.ID == ID && PreviousToken.Username == Username)
                return PreviousToken.AuthToken;

            //Check if the IP is Null or Invalid
            if (Ip != null && Ip != "")
            {
                string random = FilterToken(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));

                Token token = new Token(Ip, ID, Username, random);

                //While the token we generated is already used, try and regen a new one
                while (CheckToken(token))
                {
                    random = FilterToken(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));

                    token = new Token(Ip, ID, Username, random);
                }

                lock (AuthTokens)
                {
                    //Add token to the memory
                    AuthTokens.Add(token);
                }

                return random;
            }

            return null;
        }

        /// <summary>
        /// Filters Guid Input to create a clean GUID
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string FilterToken(string input)
        {
            input = input.Replace("+", "");
            input = input.Replace("=", "");

            return input;
        }
    }
}
