using System;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using Microsoft.Graph;
using Microsoft.Identity.Client;
using MySql.Data.MySqlClient;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Renci.SshNet;
using System.Threading.Tasks;
using NativeWifi;

namespace VisualAutomationTool
{
    /*-----PARTIE LOGIQUE-----*/
    public class SimpleEncryption
    {
        public static string ReadSensibleInfo()
        {
            string temp = string.Empty;

            var key = Console.ReadKey(true); // Temporary key value to avoid a while(true)
            temp += key.KeyChar;
            while (key.Key != ConsoleKey.Enter)
            {
                key = Console.ReadKey(true);
                temp += key.KeyChar;
            }

            return temp;
        }
        public static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        public static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        //TODO:https://api.motdepasse.xyz/create/?include_digits&include_lowercase&include_uppercase&exclude_similar_characters&password_length=10&quantity=1&add_custom_characters=!=-
        public static string GenerateRandomPassword(UInt32 passwordLength, bool enableSymbols)
        {
            string password = "";

            string[] symbols = new string[] { "!", "&", "%", "$" };
            string[] letters = new string[] { "A", "B", "C", "D", "E", "F", "G", "H",
                                              "J", "K", "M", "N", "P", "Q", "R",
                                              "S", "T", "U", "V", "W", "X", "Y", "Z"};

            UInt32 symbolCount = 0;
            UInt32 letterCount = 0;
            UInt32 numberCount = 0;

            for (UInt32 i = 0; i < passwordLength; i++)
            {
                Int32 randomChoice = RandomNumberGenerator.GetInt32(3);

                switch (randomChoice)
                {
                    case 0:
                        if (enableSymbols)
                        {
                            int randomSymbolIndex = RandomNumberGenerator.GetInt32(symbols.Length - 1);

                            password += symbols[randomSymbolIndex];

                            symbolCount++;
                        }
                        else
                        {
                            password += RandomNumberGenerator.GetInt32(1, 10);
                        }

                        break;

                    case 1:
                        int randomLetterIndex = RandomNumberGenerator.GetInt32(letters.Length - 1);
                        if ((randomLetterIndex % 2 == 0)) { password += letters[randomLetterIndex].ToLower(); }
                        else { password += letters[randomLetterIndex]; }

                        letterCount++;
                        break;

                    case 2:
                        Int32 randomNumber = RandomNumberGenerator.GetInt32(1, 10);
                        password += randomNumber;

                        numberCount++;
                        break;
                }

                //if (password[password.Length-1] == password[password.Length-2])
                //{ i--; }
            }

            if (enableSymbols)
            {
                if (symbolCount < passwordLength / 5 || symbolCount > passwordLength / 4 ||
                    letterCount >= passwordLength / 2 || letterCount < passwordLength / 4 ||
                    numberCount >= passwordLength / 2 || numberCount < passwordLength / 4)
                {
                    return GenerateRandomPassword(passwordLength, true);
                }
            }
            else
            {
                if (letterCount >= passwordLength / 2 || letterCount <= passwordLength / 4 ||
                    numberCount >= passwordLength / 2 || numberCount <= passwordLength / 4)
                {
                    return GenerateRandomPassword(passwordLength, false);
                }
            }


            return password;

        }

        //public static string GenerateRandomPasswordFromAPI(UInt32 passwordLength)
        //{
        //
        //}
    }
    public class SecretFile
    {
        private byte[] mEncryptedADUsername = Array.Empty<byte>();
        private byte[] mEncryptedADPassword = Array.Empty<byte>();

        private byte[] mEncryptedMSSecret = Array.Empty<byte>();

        private byte[] mEncryptedLLUID = Array.Empty<byte>();
        private byte[] mEncryptedLLPassword = Array.Empty<byte>();

        private byte[] mEncryptedSSHUsername = Array.Empty<byte>();
        private byte[] mEncryptedSSHPassword = Array.Empty<byte>();

        private Aes mAesEncryption = Aes.Create();
        public static bool CheckForValidSecretFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine("File does not exist");
                return false;
            }

            byte[] content = System.IO.File.ReadAllBytes(filePath);

            if (content.Length <= 10)
            {
                Console.WriteLine("File is not valid");
                return false;
            }

            return true;
        }

        public void ParseSecretFile(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine().ToString();
                    if (line.Substring(0, 2) == "AD")
                    {
                        var credentials = line.Substring(3).Split(";");

                        mEncryptedADUsername = SimpleEncryption.EncryptStringToBytes(credentials[0], mAesEncryption.Key, mAesEncryption.IV);
                        mEncryptedADPassword = SimpleEncryption.EncryptStringToBytes(credentials[1], mAesEncryption.Key, mAesEncryption.IV);
                    }

                    else if (line.Substring(0, 2) == "MS")
                    {
                        var credential = line.Substring(3);

                        mEncryptedMSSecret = SimpleEncryption.EncryptStringToBytes(credential, mAesEncryption.Key, mAesEncryption.IV);
                    }

                    else if (line.Substring(0, 2) == "LL")
                    {
                        var credentials = line.Substring(3).Split(";");

                        mEncryptedLLUID = SimpleEncryption.EncryptStringToBytes(credentials[0], mAesEncryption.Key, mAesEncryption.IV);
                        mEncryptedLLPassword = SimpleEncryption.EncryptStringToBytes(credentials[1], mAesEncryption.Key, mAesEncryption.IV);
                    }

                    else if (line.Substring(0, 2) == "SS")
                    {
                        var credentials = line.Substring(3).Split(";");

                        mEncryptedSSHUsername = SimpleEncryption.EncryptStringToBytes(credentials[0], mAesEncryption.Key, mAesEncryption.IV);
                        mEncryptedSSHPassword = SimpleEncryption.EncryptStringToBytes(credentials[1], mAesEncryption.Key, mAesEncryption.IV);
                    }

                    else
                    {
                        Console.WriteLine("Format de ligne inconnu");
                    }
                }
            }
        }

        public string GetADCredentials()
        {
            return (SimpleEncryption.DecryptStringFromBytes(mEncryptedADUsername, mAesEncryption.Key, mAesEncryption.IV) + ";" +
                    SimpleEncryption.DecryptStringFromBytes(mEncryptedADPassword, mAesEncryption.Key, mAesEncryption.IV));
        }

        public string GetMSSecret()
        {
            return (SimpleEncryption.DecryptStringFromBytes(mEncryptedMSSecret, mAesEncryption.Key, mAesEncryption.IV));
        }

        public string GetLLCredentials()
        {
            return (SimpleEncryption.DecryptStringFromBytes(mEncryptedLLUID, mAesEncryption.Key, mAesEncryption.IV) + ";" +
                    SimpleEncryption.DecryptStringFromBytes(mEncryptedLLPassword, mAesEncryption.Key, mAesEncryption.IV));
        }

        public string GetSSHCredentials()
        {
            return (SimpleEncryption.DecryptStringFromBytes(mEncryptedSSHUsername, mAesEncryption.Key, mAesEncryption.IV) + ";" +
                    SimpleEncryption.DecryptStringFromBytes(mEncryptedSSHPassword, mAesEncryption.Key, mAesEncryption.IV));
        }
    }
    public class ActiveDirectoryManager
    {
        public ActiveDirectoryManager(SecretFile secretFile = null)
        {
            mSecretFile = secretFile;
            GetSensibleInfosFromUser();
        }
        public Dictionary<string, string> mServices = new Dictionary<string, string>()
        {
            {"ABI",         "Abidjan"},
            {"COMMERCIAL",  "Commercial"},
            {"COMPTA",      "Comptabilité"},
            {"ADMIN",       "Le Lab Administratif"},
            {"COM",         "Le Lab Communication"},
            {"CM",          "Le Lab Community Management"},
            {"DESIGN",      "Le Lab Design"},
            {"DEV",         "Le Lab Developpement"},
            {"EDIT",        "Le Lab Editorial"},
            {"TECH",        "Le Lab Tech Reseau"},
            {"WEBMARKET",   "Le Lab Webmarketing"},
            {"LONDRES",     "Londres"},
            {"RECRUTEMENT", "Recrutement"},
            {"SUIVI",       "SuiviClients"}
        };

        SecretFile mSecretFile = null;

        private byte[] mEncryptedUsername = Array.Empty<byte>();
        private byte[] mEncryptedPassword = Array.Empty<byte>();
        private readonly Aes mAesEncryption = Aes.Create();

        private bool mCleanMode = true;

        public void GetSensibleInfosFromUser()
        {
            if (mSecretFile != null)
            {
                var credentials = mSecretFile.GetADCredentials().Split(";");
                mEncryptedUsername = SimpleEncryption.EncryptStringToBytes(credentials[0], mAesEncryption.Key, mAesEncryption.IV);
                mEncryptedPassword = SimpleEncryption.EncryptStringToBytes(credentials[1], mAesEncryption.Key, mAesEncryption.IV);

                return;
            }

            string inputUsername = string.Empty;
            string inputPassword = string.Empty;

            try
            {
                Console.WriteLine("Veuillez entrer le nom d'utilisateur à utiliser afin de se connecter : ");
                inputUsername = SimpleEncryption.ReadSensibleInfo();

                Console.WriteLine("Veuillez entrer le mot de passe à utiliser afin de se connecter : ");
                inputPassword = SimpleEncryption.ReadSensibleInfo();
            }

            catch (Exception e)
            {
                Console.WriteLine("Erreur dans la lecture des informations, veuillez réessayer !");

                GetSensibleInfosFromUser();
            }

            if (inputUsername == null || inputPassword == null)
            {
                Console.WriteLine("Erreur dans la lecture des informations, veuillez réessayer !");
                GetSensibleInfosFromUser();
            }

            mEncryptedUsername = SimpleEncryption.EncryptStringToBytes(inputUsername, mAesEncryption.Key, mAesEncryption.IV);
            mEncryptedPassword = SimpleEncryption.EncryptStringToBytes(inputPassword, mAesEncryption.Key, mAesEncryption.IV);

            if (!CheckForValidCredentials()) { Console.WriteLine("Informations incorrects : "); GetSensibleInfosFromUser(); }

        }

        private bool CheckForValidCredentials()
        {
            bool isEmpty = (mEncryptedUsername != Array.Empty<byte>() || mEncryptedPassword != Array.Empty<byte>());
            bool isCorrect = true;

            string decryptedUsername = SimpleEncryption.DecryptStringFromBytes(mEncryptedUsername, mAesEncryption.Key, mAesEncryption.IV);
            string decryptedPassword = SimpleEncryption.DecryptStringFromBytes(mEncryptedPassword, mAesEncryption.Key, mAesEncryption.IV);
            try
            {
                var ctx = new PrincipalContext(ContextType.Domain, "srv-dc", decryptedUsername.Substring(0, decryptedUsername.Length - 1),
                                                                             decryptedPassword.Substring(0, decryptedPassword.Length - 1));

                using (UserPrincipal user = new UserPrincipal(ctx))
                {
                    using (PrincipalSearcher searcher = new PrincipalSearcher(user))
                    { }
                }
            }
            catch (DirectoryServicesCOMException e)
            {
                isCorrect = false;
            }

            return (isEmpty && isCorrect);
        }

        public string TranslateService(string service)
        {
            if (service.ToUpper().StartsWith("ABI")) { return "ABI"; }
            else if (service.ToUpper().StartsWith("COMMER")) { return "COMMERCIAL"; }
            else if (service.ToUpper().StartsWith("COMPTA")) { return "COMPTA"; }
            else if (service.ToUpper().StartsWith("ADM")) { return "ADMIN"; }
            else if (service.ToUpper().StartsWith("COMMU")) { return "COM"; }
            else if (service.ToUpper().StartsWith("CM")) { return "CM"; }
            else if (service.ToUpper().StartsWith("DES")) { return "DESIGN"; }
            else if (service.ToUpper().StartsWith("DEV") ||
                     service.ToUpper().StartsWith("DÉV")) { return "DESIGN"; }
            else if (service.ToUpper().StartsWith("ED")) { return "EDIT"; }
            else if (service.ToUpper().StartsWith("TE")) { return "TECH"; }
            else if (service.ToUpper().StartsWith("WEB")) { return "WEBMARKET"; }
            else if (service.ToUpper().StartsWith("LON")) { return "LONDRES"; }
            else if (service.ToUpper().StartsWith("REC")) { return "RECRUTEMENT"; }
            else if (service.ToUpper().StartsWith("SUI")) { return "SUIVI"; }
            else { return service.ToUpper(); }

        }

        public bool CheckForValidService(string service)
        {
            foreach (var elem in mServices)
            {
                if (elem.Key == service.ToUpper()) { return true; }
                else if (elem.Value.ToUpper() == service.ToUpper()) { return true; }
            }

            return false;
        }

        //TryException/CheckForValidService() ne peut pas être utilisé proprement car une valeur de retour est exigée, il faut donc utiliser CheckForValidService() avant l'appel
        //de cette fonction
        private PrincipalContext GetContextFromService(string service)
        {

            string decryptedUsername = SimpleEncryption.DecryptStringFromBytes(mEncryptedUsername, mAesEncryption.Key, mAesEncryption.IV);
            string decryptedPassword = SimpleEncryption.DecryptStringFromBytes(mEncryptedPassword, mAesEncryption.Key, mAesEncryption.IV);

            return new PrincipalContext(ContextType.Domain, "srv-dc", "OU=" + mServices[service.ToUpper()] + ",OU=User_Incomm,DC=okkin,DC=net",
                                    decryptedUsername.Substring(0, decryptedUsername.Length),
                                    decryptedPassword.Substring(0, decryptedPassword.Length));
        }

        public string GetAllServices()
        {
            StringBuilder allServices = new("");

            foreach (KeyValuePair<string, string> pair in mServices)
            {
                allServices.Append(String.Format("- {0} pour {1}\n", pair.Key, pair.Value));
            }

            return allServices.ToString();
        }

        public List<string> GetAllServicesAsList()
        {
            List<string> allServices = new();

            foreach (KeyValuePair<string, string> pair in mServices)
            {
                allServices.Add(pair.Key);
            }

            return allServices;
        }

        public string GetAllUsersFromService(string service)
        {
            if (!CheckForValidService(TranslateService(service))) { Console.WriteLine("Service non reconnu"); return ""; }

            StringBuilder users = new("");

            PrincipalContext ctx = GetContextFromService(service);
            using (UserPrincipal user = new UserPrincipal(ctx))
            {
                using (PrincipalSearcher searcher = new PrincipalSearcher(user))
                {
                    foreach (UserPrincipal result in searcher.FindAll())
                    {
                        users.Append(String.Format("{0} ({1})\n", result.Name, result.SamAccountName));
                    }
                }
            }

            return users.ToString();
        }

        public List<string> GetAllUsersFromServiceAsList(string service)
        {
            if (!CheckForValidService(TranslateService(service))) { Console.WriteLine("Service non reconnu"); return new(); }

            List<string> users = new();

            PrincipalContext ctx = GetContextFromService(service);
            using (UserPrincipal user = new UserPrincipal(ctx))
            {
                using (PrincipalSearcher searcher = new PrincipalSearcher(user))
                {
                    foreach (UserPrincipal result in searcher.FindAll())
                    {
                        users.Add(result.Name + " | " + result.SamAccountName);
                    }
                }
            }

            return users;
        }

        public List<UserPrincipal> GetAllUsersFromServiceAsUPList(string service)
        {
            if (!CheckForValidService(TranslateService(service))) { Console.WriteLine("Service non reconnu"); return new(); }

            List<UserPrincipal> users = new();

            PrincipalContext ctx = GetContextFromService(service);
            using (UserPrincipal user = new UserPrincipal(ctx))
            {
                using (PrincipalSearcher searcher = new PrincipalSearcher(user))
                {
                    foreach (UserPrincipal result in searcher.FindAll())
                    {
                        users.Add(result);
                    }
                }
            }

            return users;
        }

        public List<string> GetAllUsersAsList()
        {
            List<string> allusers = new();

            foreach (KeyValuePair<string, string> pair in mServices)
            {
                var currentServiceUsers = GetAllUsersFromServiceAsList(pair.Key);

                foreach (var user in currentServiceUsers)
                {
                    allusers.Add(user);
                }
            }

            return allusers;
        }

        public List<UserPrincipal> GetAllUsersAsUPList()
        {
            List<UserPrincipal> allusers = new();

            foreach (KeyValuePair<string, string> pair in mServices)
            {
                var currentServiceUsers = GetAllUsersFromServiceAsUPList(pair.Key);

                foreach (var user in currentServiceUsers)
                {
                    allusers.Add(user);
                }
            }

            return allusers;
        }

        public UserPrincipal GetUserPrincipal(string sam)
        {
            var users = GetAllUsersAsUPList();

            UserPrincipal userFound = null;
            foreach (var user in users)
            {
                if (user.SamAccountName == sam)
                {
                    userFound = user;
                }
            }

            return userFound;
        }

        public void DisplayAllUsersFromService(string service)
        {
            string translatedService = TranslateService(service);
            if (!CheckForValidService(translatedService)) { Console.WriteLine("Service non reconnu"); return; }
            Console.WriteLine("Service [" + mServices[translatedService.ToUpper()] + "] :");

            Console.Write(GetAllUsersFromService(service));
        }

        public void DisplayUserInfos(string service, string name)
        {
            string translatedService = TranslateService(service);
            if (!CheckForValidService(translatedService)) { Console.WriteLine("Service non reconnu"); return; }
            PrincipalContext ctx = GetContextFromService(translatedService);
            UserPrincipal userFound;

            try
            {
                userFound = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, name);

            }

            catch (Exception)
            {
                Console.WriteLine("bof");
                return;
            }
            string userFoundInfos = "";

            userFoundInfos += ("Infos de [" + userFound.Name + "] :\n");
            userFoundInfos += ("SamAccountName        : [" + userFound.SamAccountName + "]\n");
            userFoundInfos += ("Distinguished name    : [" + userFound.DistinguishedName + "]\n");
            userFoundInfos += ("UserPrincipalName     : [" + userFound.UserPrincipalName + "]\n");
            userFoundInfos += ("Display name          : [" + userFound.DisplayName + "]\n");
            userFoundInfos += ("Surname               : [" + userFound.Surname + "]\n");
            userFoundInfos += ("Given name            : [" + userFound.GivenName + "]\n");
            userFoundInfos += ("MiddleName            : [" + userFound.MiddleName + "]\n");
            userFoundInfos += ("Name                  : [" + userFound.Name + "]\n");

            userFoundInfos += ("Home drive            : [" + userFound.HomeDrive + "]\n");
            userFoundInfos += ("Home directory        : [" + userFound.HomeDirectory + "]\n");

            userFoundInfos += ("SID                   : [" + userFound.Sid + "]\n");
            userFoundInfos += ("Expiration date       : [" + userFound.AccountExpirationDate + "]\n");
            userFoundInfos += ("Bad logon count       : [" + userFound.BadLogonCount + "]\n");

            userFoundInfos += ("Context               : [" + userFound.Context + "]\n");
            userFoundInfos += ("Description           : [" + userFound.Description + "]\n");
            userFoundInfos += ("Email address         : [" + userFound.EmailAddress + "]\n");

            userFoundInfos += ("LastLogon             : [" + userFound.LastLogon + "]\n");
            userFoundInfos += ("LastPasswordSet       : [" + userFound.LastPasswordSet + "]\n");
            userFoundInfos += ("PasswordNeverExpires  : [" + userFound.PasswordNeverExpires + "]\n");
            userFoundInfos += ("PasswordNotRequired   : [" + userFound.PasswordNotRequired + "]\n");
            userFoundInfos += ("PermittedLogonTimes   : [" + userFound.PermittedLogonTimes + "]\n");
            userFoundInfos += ("PermittedWorkstations : [" + userFound.PermittedWorkstations + "]\n");
            userFoundInfos += ("ScriptPath            : [" + userFound.ScriptPath + "]\n");



            Console.Write(userFoundInfos);

            Console.WriteLine("Sauvegarder ces infos dans un fichier (o/n)?");
            var answer = Console.ReadLine();
            if (answer.StartsWith("o"))
            {
                string directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "Documents/AD/Infos/";
                string filePath = directory + name + ".txt";

                System.IO.Directory.CreateDirectory(directory);
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.Write(userFoundInfos);
                }

                Console.WriteLine("File can be found here : " + filePath);
            }
        }

        //TryException/CheckForValidService() ne peut pas être utilisé proprement car une valeur de retour est exigée, il faut donc utiliser CheckForValidService() avant l'appel
        //de cette fonction
        private UserPrincipal GetModelUserFromService(string service)
        {
            PrincipalContext ctx = GetContextFromService(service);
            using (UserPrincipal user = new UserPrincipal(ctx))
            {
                using (PrincipalSearcher searcher = new PrincipalSearcher(user))
                {
                    return (UserPrincipal)searcher.FindOne();
                }
            }
        }

        public bool CreateUserInService(string prenom, string nom, string service)
        {
            string translatedService = TranslateService(service);
            if (!CheckForValidService(translatedService)) { Console.WriteLine("Service non reconnu"); return false; }

            UserPrincipal model = GetModelUserFromService(translatedService);

            try
            {
                var existingUser = UserPrincipal.FindByIdentity(model.Context, IdentityType.SamAccountName,
                                                                (prenom[0].ToString().ToLower() + nom.ToLower()));
            }
            catch (Exception)
            {
                Console.WriteLine("Utilisateur déjà existant");
                return false;
            }

            UserPrincipal newUser = new UserPrincipal(model.Context);

            newUser.ExpirePasswordNow();
            newUser.DisplayName = nom.ToUpper() + " " + prenom;
            newUser.GivenName = prenom;
            newUser.SamAccountName = (prenom.Substring(0, 1) + nom).ToLower();
            newUser.Surname = nom.ToUpper();
            newUser.UserPrincipalName = (prenom.Substring(0, 1) + nom).ToLower() + "@incomm.fr";
            newUser.Name = prenom + " " + nom.ToUpper();

            newUser.SetPassword("xxxxxx");
            newUser.Enabled = true;
            newUser.PasswordNotRequired = false;
            newUser.PasswordNeverExpires = true;
            newUser.UserCannotChangePassword = true;

            newUser.ScriptPath = "Script.bat";

            newUser.Save();

            foreach (GroupPrincipal elem in model.GetGroups())
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity(elem.Context, IdentityType.SamAccountName, elem.SamAccountName);
                if (group == null)
                {
                    Console.WriteLine("Ne sera pas ajouté au groupe : " + elem.DisplayName);
                }
                else
                {
                    group.Members.Add(newUser);
                    group.Save();
                }
            }

            return true;
        }

        public void DeleteUserInService(string prenom, string nom, string service)
        {
            string translatedService = TranslateService(service);
            if (!CheckForValidService(translatedService)) { Console.WriteLine("Service non reconnu"); return; }
            PrincipalContext ctx = GetContextFromService(translatedService);

            string name = prenom[0].ToString() + nom;

            UserPrincipal userFound = null;
            try
            {
                userFound = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, name);
            }
            catch (Exception)
            {
                Console.WriteLine("Utilisateur non existant");
                return;
            }

            userFound.Delete();
        }

        public void DeleteUser(string sam)
        {
            var users = GetAllUsersAsUPList();

            var user = GetUserPrincipal(sam);
            UserPrincipal userFound = null;
            try
            {
                userFound = UserPrincipal.FindByIdentity(user.Context, IdentityType.SamAccountName, sam);
            }
            catch (Exception)
            {
                Console.WriteLine("Utilisateur non existant");
                return;
            }

            userFound.Delete();
        }

        public void About()
        {
            Console.WriteLine("Je vous souhaite la bienvenue sur l'ADMANAGER ou Active Directory Manager");
            Console.WriteLine("Son but est de simplifier et rendre plus efficace la création, suppression et mise à jour des informations sur l'AD");
            Console.WriteLine("Pour se faire, de nombreuses commandes sont à disposition");
            Console.WriteLine("Lorsqu'il vous est demandé de rentrer le nom d'un service, il est insinué qu'il faut entrer le nom abrégé de ce dernier :");
            Console.Write(GetAllServices());
        }

        
        
    }
    public class MicrosoftManager
    {
        public MicrosoftManager(SecretFile secretFile = null)
        {
            mSecretFile = secretFile;
            GetSensibleInfosFromUser();
        }

        SecretFile mSecretFile = null;

        private bool mCleanMode = true;
        private readonly string mTenantId = "xxxxxxxxx";
        public readonly string mM365BusinessStd = "xxxxxxx";
        public readonly string mM365BusinessBsc = "xxxxxxx";

        private byte[] mEncryptedSecret = Array.Empty<byte>();
        private readonly Aes mAesEncryption = Aes.Create();

        string[] mScopes = new string[] { "https://graph.microsoft.com/.default" };
        IConfidentialClientApplication mClientApp = null;
        GraphServiceClient mGraphClient = null;

        public void GetSensibleInfosFromUser()
        {
            if (mSecretFile != null)
            {
                var credential = mSecretFile.GetMSSecret();
                mEncryptedSecret = SimpleEncryption.EncryptStringToBytes(credential, mAesEncryption.Key, mAesEncryption.IV);
                return;
            }

            Console.WriteLine("Veuillez entrer la secret key :");
            var secret = SimpleEncryption.ReadSensibleInfo();
            mEncryptedSecret = SimpleEncryption.EncryptStringToBytes(secret, mAesEncryption.Key, mAesEncryption.IV);
        }

        public IConfidentialClientApplication GetApplication()
        {
            if (mClientApp == null)
            {
                return ConfidentialClientApplicationBuilder
                   .Create("xxxxxxxxxxxxxxx")
                   .WithRedirectUri("https://127.0.0.1")
                   .WithAuthority("https://login.microsoftonline.com/" + mTenantId + "/v2.0")
                   .WithClientSecret(SimpleEncryption.DecryptStringFromBytes(mEncryptedSecret, mAesEncryption.Key, mAesEncryption.IV))
                   .Build();
            }

            return mClientApp;
        }

        private GraphServiceClient GetGraphClient(IConfidentialClientApplication application)
        {
            if (mGraphClient == null)
            {
                GraphServiceClient graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                {

                    // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                    var authResult = application.AcquireTokenForClient(mScopes).ExecuteAsync();
                    authResult.Wait();

                    // Add the access token in the Authorization header of the API request.
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Result.AccessToken);
                })
                );

                return graphServiceClient;
            }

            return mGraphClient;
        }

        public string GetMailUsername(string userFullName)
        {
            var prenomNom = userFullName.Split(" ");
            var prenom = prenomNom[0];
            var nom = prenomNom[1];

            return (prenom[0].ToString().ToLower() + nom.ToLower());
        }

        public List<string> GetLicensesAsList()
        {
            List<string> availableLicenses = new();
            availableLicenses.Add("Aucune Licence");
            availableLicenses.Add("Licence Basic");
            availableLicenses.Add("Licence Standard");

            return availableLicenses;
        }

        public async Task<string> GetAllUsers(IConfidentialClientApplication application)
        {
            var graphServiceClient = GetGraphClient(application);

            StringBuilder sAllUsers = new("");

            var allUsers = new List<User>();
            var users = await graphServiceClient.Users.Request().GetAsync();
            allUsers.AddRange(users.CurrentPage);

            while (users.NextPageRequest != null)
            {
                users = await users.NextPageRequest.GetAsync();

                allUsers.AddRange(users.CurrentPage);
                // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/54
            }

            foreach (var user in allUsers)
            {
                sAllUsers.Append(String.Format("{0} [{1}]\n", user.DisplayName, user.UserPrincipalName));
            }

            return sAllUsers.ToString();
        }

        public async Task<List<string>> GetAllUsersAsList(IConfidentialClientApplication application)
        {
            var graphServiceClient = GetGraphClient(application);

            List<string> allUsers = new();

            var microsoftUsers = new List<User>();
            var users = await graphServiceClient.Users.Request().GetAsync();
            microsoftUsers.AddRange(users.CurrentPage);

            while (users.NextPageRequest != null)
            {
                users = await users.NextPageRequest.GetAsync();

                microsoftUsers.AddRange(users.CurrentPage);
                // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/54
            }

            foreach (var user in microsoftUsers)
            {
                allUsers.Add(user.DisplayName + " | " + user.Mail);
            }

            return allUsers;
        }

        public async Task<string> GetUserInfos(IConfidentialClientApplication application, string userName)
        {
            var graphServiceClient = GetGraphClient(application);

            var prenomNom = userName.Split(" ");
            var mailUsername = prenomNom[0][0].ToString().ToLower() + prenomNom[1].ToLower();

            StringBuilder userInfos = new("");
            var allUsers = new List<User>();
            var users = await graphServiceClient.Users.Request().GetAsync();
            allUsers.AddRange(users.CurrentPage);

            while (users.NextPageRequest != null)
            {
                users = await users.NextPageRequest.GetAsync();

                allUsers.AddRange(users.CurrentPage);
                // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/54
            }

            User userFound = null;
            foreach (var user in allUsers)
            {
                if (user.UserPrincipalName == (mailUsername + "@incomm.fr"))
                {
                    userFound = user;
                }
            }

            if (userFound == null)
            {
                Console.WriteLine("Utilisateur introuvable");
                return "";
            }

            var licensePage = await graphServiceClient.Users[userFound.Id].LicenseDetails.Request().GetAsync();

            string licensesInfos = "";

            foreach (var licenceInfo in licensePage)
            {
                licensesInfos += "Services plan :\n";
                foreach (var plan in licenceInfo.ServicePlans)
                {
                    licensesInfos += "ID                  : [" + plan.ServicePlanId + "]\n";
                    licensesInfos += "Name                : [" + plan.ServicePlanName + "]\n";
                    licensesInfos += "Provisioning status : [" + plan.ProvisioningStatus + "]\n";
                    licensesInfos += "Applies to          : [" + plan.AppliesTo + "]\n";
                }

                licensesInfos += "\tSku ID          : [" + licenceInfo.SkuId.ToString() + "]\n";
                licensesInfos += "\tSku part number : [" + licenceInfo.SkuPartNumber.ToString() + "]\n";
                licensesInfos += "\tID              : [" + licenceInfo.Id.ToString() + "]\n";

            }

            userInfos.Append(String.Format("Infos de [{0}]\n", userFound.DisplayName));
            userInfos.Append(String.Format("ID Utilisateur   : [{0}]\n", userFound.Id));
            userInfos.Append(String.Format("Adresse mail     : [{0}]\n", userFound.Mail));
            userInfos.Append(String.Format("Given name       : [{0}]\n", userFound.GivenName));
            userInfos.Append(String.Format("Date de création : [{0}]\n", userFound.CreatedDateTime));
            userInfos.Append(String.Format("Alias mail       : [{0}]\n", userFound.MailNickname));
            userInfos.Append(String.Format("License details  : [{0}]", licensesInfos));

            return userInfos.ToString();
        }
        public void DisplayAllUsers()
        {
            Console.Write(GetAllUsers(GetApplication()));
        }


        public string GenerateOfficePassword()
        {
            return SimpleEncryption.GenerateRandomPassword(12, true);
        }

        public async Task<bool> CreateUser(IConfidentialClientApplication application, string prenom, string nom, string password)
        {
            var graphServiceClient = GetGraphClient(application);
            string alias = prenom[0].ToString().ToLower() + nom.ToLower();

            bool alreadyExists = true;

            try
            {
                var a = await graphServiceClient.Users[alias + "@incomm.fr"].Request().GetAsync();
            }
            catch (Exception)
            {
                alreadyExists = false;
            }

            if (alreadyExists) { return false; }

            var user = new User
            {
                UsageLocation = "FR",
                AccountEnabled = true,
                DisplayName = prenom + " " + nom,
                MailNickname = (prenom[0] + nom).ToLower(),
                UserPrincipalName = (prenom[0] + nom).ToLower() + "@incomm.fr",
                Mail = (prenom[0] + nom).ToLower() + "@incomm.fr",
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = true,
                    Password = password
                }
            };

            await graphServiceClient.Users.Request().AddAsync(user);

            return true;
        }

        public async Task<bool> DeleteUser(IConfidentialClientApplication application, string userFullName)
        {
            var graphServiceClient = GetGraphClient(application);
            string alias = GetMailUsername(userFullName);

            try
            {
                await graphServiceClient.Users[alias + "@incomm.fr"].Request().DeleteAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteUserFromMail(IConfidentialClientApplication application, string mail)
        {
            var graphServiceClient = GetGraphClient(application);

            try
            {
                await graphServiceClient.Users[mail].Request().DeleteAsync();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> AssignLicenses(IConfidentialClientApplication application, string prenom, string nom, string licenceSkuID)
        {
            var graphServiceClient = GetGraphClient(application);
            var alias = prenom[0].ToString().ToLower() + nom.ToLower();

            var addLicenses = new List<AssignedLicense>()
            {
                new AssignedLicense
                {
                    SkuId = Guid.Parse(licenceSkuID)
                }
            };

            var removeLicenses = new List<Guid>();

            try
            {
                await graphServiceClient.Users[alias + "@incomm.fr"].AssignLicense(addLicenses, removeLicenses).Request().PostAsync();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void About()
        {
            Console.WriteLine("Je vous souhaite la bienvenue sur le MICROSOFTMANAGER");
            Console.WriteLine("Son but est de simplifier et de servir d'interface super simple avec les différentes API Microsoft (MSAL et Graph)");
            Console.WriteLine("Pour ce faire, de nombreuses commandes sont à disposition");
        }

        public async Task<bool> SendMail(IConfidentialClientApplication application, string subject, string body, List<string> toRecipients, List<string> ccRecipients)
        {
            //From https://docs.microsoft.com/fr-fr/graph/api/user-sendmail?view=graph-rest-1.0&tabs=csharp

            var graphServiceClient = GetGraphClient(application);

            List<Recipient> allToRecipients = new();
            foreach (var recipient in toRecipients)
            {
                allToRecipients.Add(new Recipient { EmailAddress = new EmailAddress { Address = recipient } });
            }

            List<Recipient> allCCRecipients = new();
            foreach (var recipient in ccRecipients)
            {
                allCCRecipients.Add(new Recipient { EmailAddress = new EmailAddress { Address = recipient } });
            }

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = body
                },

                ToRecipients = allToRecipients,
                CcRecipients = allCCRecipients
            };

            var saveToSentItems = false;

            try
            {
                await graphServiceClient.Users["support@incomm.fr"]
                .SendMail(message, saveToSentItems)
                .Request()
                .PostAsync();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                Clipboard.SetText(e.ToString());
                return false;
            }

            return true;
        }

        public async Task<int> GetRemainingAvailableLicenseCount(IConfidentialClientApplication application, string licenseSkuID)
        {
            var graphServiceClient = GetGraphClient(application);

            int licenseCount = -1;
            try
            {
                var subscribedSkus = await graphServiceClient.SubscribedSkus
                                                         .Request()
                                                         .GetAsync();
                foreach(var license in subscribedSkus)
                {
                    if (license.SkuId.ToString() == licenseSkuID)
                    {
                        licenseCount = (int)license.PrepaidUnits.Enabled - (int)license.ConsumedUnits; 
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString(), "Erreur lors de l'opération");
                Clipboard.SetText(e.ToString());
            }


            return licenseCount;
        }
    }


    public class LeLogManager
    {
        public LeLogManager(SecretFile secretFile = null)
        {
            mSecretFile = secretFile;
            GetSensibleInfosFromUser();
        }

        SecretFile mSecretFile;
        private bool mCleanMode = true;
        string mServerName = "xxxxxx";
        public string mDbName = "xxxxx";
        string mIncompletePassword = "xxxxxx";

        MySqlConnection mConnection = null;

        private byte[] mEncryptedUsername = Array.Empty<byte>();
        private byte[] mEncryptedPassword = Array.Empty<byte>();
        private readonly Aes mAesEncryption = Aes.Create();

        public void GetSensibleInfosFromUser()
        {
            if (mSecretFile != null)
            {
                var credentials = mSecretFile.GetLLCredentials().Split(";");
                mEncryptedUsername = SimpleEncryption.EncryptStringToBytes(credentials[0], mAesEncryption.Key, mAesEncryption.IV);
                mEncryptedPassword = SimpleEncryption.EncryptStringToBytes(credentials[1], mAesEncryption.Key, mAesEncryption.IV);

                return;
            }
            Console.WriteLine("Veuillez entrer le nom d'utilisateur :");
            mEncryptedUsername = SimpleEncryption.EncryptStringToBytes(SimpleEncryption.ReadSensibleInfo(), mAesEncryption.Key, mAesEncryption.IV);

            Console.WriteLine("Veuillez entrer les 4 caractères secrets (2 premiers et 2 derniers) à la suite :");
            var secrets = SimpleEncryption.ReadSensibleInfo();
            var tempPassword = mIncompletePassword.Replace("X", "");
            mEncryptedPassword = SimpleEncryption.EncryptStringToBytes(secrets.Substring(0, 2) + tempPassword + secrets.Substring(2, 2),
                                                                      mAesEncryption.Key, mAesEncryption.IV);
        }

        public bool Connect()
        {
            if (mEncryptedUsername == Array.Empty<byte>() || mEncryptedPassword == Array.Empty<byte>())
            {
                Console.WriteLine("Veuillez renseigner username et password avant d'établir une connexion");
                return false;
            }

            if (mConnection != null)
            {
                Console.WriteLine("La connexion est déjà établie");
                return false;
            }

            var decryptedUsername = SimpleEncryption.DecryptStringFromBytes(mEncryptedUsername, mAesEncryption.Key, mAesEncryption.IV);
            decryptedUsername = decryptedUsername.Substring(0, decryptedUsername.Length);
            var decryptedPassword = SimpleEncryption.DecryptStringFromBytes(mEncryptedPassword, mAesEncryption.Key, mAesEncryption.IV);

            mConnection = new MySqlConnection("Server=" + mServerName + ";" +
                                             "Database=" + mDbName + ";" +
                                             "UID=" + decryptedUsername + ";" +
                                             "Password=" + decryptedPassword + ";SslMode=none");

            mConnection.Open();

            return true;
        }

        public List<List<string>> GetDataFromQuery(string strCommand)
        {
            List<List<string>> fields = new List<List<string>>();

            var sqlCommand = new MySqlCommand(strCommand, mConnection);
            MySqlDataReader reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
                var row = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader[i].ToString());
                }
                fields.Add(row);
            }


            reader.Close();

            return fields;
        }

        public int PostDataFromQuery(string strCommand)
        {
            var sqlCommand = new MySqlCommand(strCommand, mConnection);
            var rowsAffected = sqlCommand.ExecuteNonQuery();

            return rowsAffected;
        }

        public string GetDataAsString(List<List<string>> data)
        {
            StringBuilder dataAsString = new("");

            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[i].Count; j++)
                {
                    if ((data[i].Count - i - 1) > 0) { dataAsString.Append(data[i][j] + " - "); }
                    else { dataAsString.Append(data[i][j]); }
                }
                dataAsString.Append("\n");
            }

            return dataAsString.ToString();
        }

        public void DisplayDataEvenly(List<List<string>> data, int spacing = -1)
        {
            if (spacing == -1) { spacing = 20; }
            StringBuilder toBeDisplayed = new("");

            for (int i = 0; i < data.Count; i++)
            {
                toBeDisplayed.Append(data[i][0] + " - ");
                for (int j = 1; j < data[i].Count; j++)
                {
                    toBeDisplayed.Append(data[i][j] + ((spacing - data[i][j].Length > 0) ? new string(' ', (spacing - data[i][j].Length)) : "") +
                                         ((j == data[i].Count - 1) ? "" : " - "));
                }
                toBeDisplayed.Append("\n");
            }

            Console.WriteLine(toBeDisplayed.ToString());
        }

        public List<List<string>> GetInfosAboutAllStaffMembers()
        {
            if (mConnection == null)
            {
                Console.WriteLine("La connexion n'est pas établie");
                return new List<List<string>>();
            }

            var fields = GetDataFromQuery("SELECT id_staff, prenom_staff, nom_staff, mail_staff, typ_user FROM " +
                                            "staff INNER JOIN typ_user ON staff.typ_tek_new = typ_user.id");

            return fields;
        }

        public List<List<string>> GetInfosAboutStaffMember(string userFullName)
        {
            if (mConnection == null)
            {
                Console.WriteLine("La connexion n'est pas établie");
                return new List<List<string>>();
            }

            var fields = GetDataFromQuery("SELECT id_staff, prenom_staff, nom_staff, mail_staff, typ_user FROM " +
                                            "staff INNER JOIN typ_user ON staff.typ_tek_new = typ_user.id WHERE " +
                                            "CONCAT(prenom_staff, ' ', nom_staff)='" + userFullName + "'");

            return fields;
        }

        public List<string> GetStaffMembersAsList()
        {
            var data = GetInfosAboutAllStaffMembers();

            List<string> allStaffMembers = new();

            foreach (var staffInfos in data)
            {
                allStaffMembers.Add(String.Format("{0}_{1} | ID: {2} SERVICE: {3}", staffInfos[1], staffInfos[2], staffInfos[0], staffInfos[4]));
            }

            return allStaffMembers;
        }

        public List<List<string>> GetInfosAboutStaffMember(int id)
        {
            if (mConnection == null)
            {
                Console.WriteLine("La connexion n'est pas établie");
                return new List<List<string>>();
            }

            var fields = GetDataFromQuery("SELECT id_staff, prenom_staff, nom_staff, mail_staff, typ_user_abrev FROM " +
                                            "staff INNER JOIN typ_user ON staff.typ_tek_new = typ_user.id WHERE " +
                                            "id_staff='" + id.ToString() + "'");

            return fields;
        }

        public int GetLastStaffId()
        {
            if (mConnection == null)
            {
                Console.WriteLine("La connexion n'est pas établie");
                return 0;
            }

            return Convert.ToInt32(GetDataFromQuery("SELECT id_staff FROM staff ORDER BY id_staff DESC")[0][0]);
        }

        public bool IsNumber(char c)
        {
            for (int i = 0; i < 10; i++)
            {
                if (c.ToString() == i.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckForValidID(string input)
        {
            if (input == "" || input == null || input.StartsWith("0"))
            {
                return false;
            }

            for (int i = 0; i < input.Length; i++)
            {
                if (!IsNumber(input[i])) { return false; }
            }

            return true;
        }

        public bool CheckForValidDate(string input)
        {
            if (input == null || input == "" || input == " " || input.Length != 10)
            {
                return false;
            }

            for (int i = 0; i < 10; i++)
            {
                if (!IsNumber(input[i]))
                {
                    if (input[i].ToString() != "-")
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool CheckForValidAgence(string agence)
        {
            string strCommand = String.Format("SELECT id_agence FROM agences WHERE agence='{0}'", agence);
            return GetDataFromQuery(strCommand).Count > 0 ? true : false;
        }

        public int GetAgenceID(string agence)
        {
            string strCommand = String.Format("SELECT id_agence FROM agences WHERE agence='{0}'", agence);
            return Convert.ToInt32(GetDataFromQuery(strCommand)[0][0]);
        }

        public string GetAgenceFromID(int agenceID)
        {
            string strCommand = String.Format("SELECT agence FROM agences WHERE id_agence={0}", agenceID);
            return GetDataFromQuery(strCommand)[0][0];
        }

        public bool CheckForValidStaffMember(string userFullName)
        {
            var prenomNom = userFullName.Split(" ");
            string strCommand = String.Format("SELECT id_staff FROM staff WHERE prenom_staff='{0}' AND nom_staff='{1}'", prenomNom[0], prenomNom[1]);
            return GetDataFromQuery(strCommand)[0].Count > 0 ? true : false;
        }

        public bool CheckForValidStaffService(string service)
        {
            string strCommand = String.Format("SELECT id FROM typ_user WHERE typ_user ='{0}' OR typ_user_abrev ='{0}'", service);
            return GetDataFromQuery(strCommand).Count > 0 ? true : false;
        }

        public int GetStaffServiceID(string service)
        {
            string strCommand = String.Format("SELECT id FROM typ_user WHERE typ_user ='{0}' OR typ_user_abrev ='{0}'", service);
            return Convert.ToInt32(GetDataFromQuery(strCommand)[0][0]);
        }

        public string GetStaffServiceFomServiceID(int serviceID)
        {
            string strCommand = String.Format("SELECT typ_user_abrev FROM typ_user WHERE id={0}", serviceID);
            return GetDataFromQuery(strCommand)[0][0];
        }

        public List<List<string>> GetStaffServices()
        {
            if (mConnection == null)
            {
                Console.WriteLine("La connexion n'est pas établie");
                return new List<List<string>>();

            }
            string strCommand = "SELECT id, typ_user, typ_user_abrev FROM typ_user";
            var data = GetDataFromQuery(strCommand);

            return data;
        }

        public List<string> GetStaffServicesAsList()
        {
            var data = GetStaffServices();

            List<string> allServices = new();

            foreach (var service in data)
            {
                allServices.Add(service[1]);
            }

            return allServices;
        }

        public List<List<string>> GetAgences()
        {
            if (mConnection == null)
            {
                Console.WriteLine("La connexion n'est pas établie");
                return new List<List<string>>();

            }
            string strCommand = "SELECT agence FROM agences";
            var data = GetDataFromQuery(strCommand);

            return data;
        }

        public List<string> GetAgencesAsList()
        {
            if (mConnection == null)
            {
                Console.WriteLine("La connexion n'est pas établie");
                return new List<string>();

            }

            string strCommand = "SELECT agence FROM agences";
            var data = GetDataFromQuery(strCommand);

            List<string> allAgences = new();
            foreach (var agence in data)
            {
                allAgences.Add(agence[0]);
            }

            return allAgences;
        }

        public void CreateStaffMember(string prenom, string nom, string service, bool suivi, string agence, bool stagiaire,
                                      string ftpPassword = "", string entree = "", string sortie = "", string emailPassword = "", bool nonPhysique = false, int agenceMere = -1)
        {
            if (mConnection == null)
            {
                Console.WriteLine("La connexion n'est pas établie");
                return;
            }

            var newId = (GetLastStaffId() + 1);

            var mail = prenom[0].ToString().ToLower() + nom.ToLower() + "@incomm.fr";

            var agenceID = GetAgenceID(agence);
            var serviceID = GetStaffServiceID(service);

            var type_staff = 1;
            var droits = "aucun";

            switch (serviceID)
            {
                case 6:
                    if (suivi) { droits = "suivi_client2"; }
                    else { droits = "rm"; }
                    type_staff = 0;
                    break;

                case 16:
                    droits = "aucun";
                    break;

                case 17:
                    droits = "procontact_tech";
                    break;

                default:
                    droits = "aucun";
                    agenceMere = -1;
                    break;
            }

            var strCommand = String.Format("INSERT INTO staff" +
                                           "(id_staff, societe, type_staff, ex_staff, prenom_staff, " +
                                           "nom_staff, mail_staff, login_staff, pwd_staff, css, " +
                                           "udm, agence_mere, id_agence, typ_tek_new_ex, typ_tek_new, " +
                                           "stagiaire, ftp_pass, signature_mail, entree, sortie, " +
                                           "suivi_client, droits_vrp_2, non_physique, vendeur_mois_mois, email_pop, email_pwd) " +
                                           "VALUES " +
                                           //id_staff, societe, type_staff, ex, prenom_staff
                                           "({0}, 'incomm', '{1}', 0, '{2}', " +
                                           //nom_staff, mail_staff, login_staff, pwd_staff, css
                                           "'{3}', '{4}', '{5}', '{6}', 'htdtc', " +
                                           //udm, agence_mere?, id_agence?, typ_tek_new_ex, typ_tek_new
                                           "'udm-custom_htdtc', {7}, {8}, 1, {9}, " +
                                           //stagiaire, ftp_pass, signature_mail, entree, sortie
                                           "{10}, '{11}', '{12}', '{13}', '{14}', " +
                                           //suivi, droits, non_phys, vendeur_mois_mois, email_pop, email_pwd
                                           "{15}, '{16}', {17}, -1, '{18}', '{19}')",
                                           newId, type_staff, prenom,
                                           nom, mail, prenom, nom,
                                           agenceMere, agenceID, serviceID,
                                           Convert.ToInt32(stagiaire).ToString(), ftpPassword, prenom + " " + nom,
                                           entree, sortie, Convert.ToInt32(suivi).ToString(), droits, Convert.ToInt32(nonPhysique).ToString(), mail, emailPassword);

            PostDataFromQuery(strCommand);
        }

        public void DeleteStaffMember(string prenom, string nom, bool convertToExStaff)
        {

            if (convertToExStaff)
            {
                var strCommand = String.Format("INSERT INTO staff (ex_staff) VALUES (1) WHERE prenom_staff='{0}' AND nom_staff='{1}'", prenom, nom);
                PostDataFromQuery(strCommand);
            }
            else
            {
                var strCommand = String.Format("DELETE FROM staff WHERE prenom_staff='{0}' AND nom_staff='{1}'", prenom, nom);
                PostDataFromQuery(strCommand);
            }


        }

        public bool Disconnect()
        {
            if (mConnection == null)
            {
                Console.WriteLine("La connexion n'a pas été établie, le disconnect n'a pas pu avoir lieu");
                return false;
            }

            mConnection.Close();
            mConnection = null;

            return true;
        }

        public string GetDefaultDb()
        {
            return mDbName;
        }

        public void SetDebugDbAsDefaultDb()
        {
            Disconnect();
            mDbName = "debug";
            Connect();
        }

        public void SetProdDbAsDefaultDb()
        {
            Disconnect();
            mDbName = "prod";
            Connect();
        }
    }

    public class SecureShellManager
    {
        public SecureShellManager(SecretFile secretFile = null)
        {
            mSecretFile = secretFile;
            GetSensibleInfosFromUser();
        }

        private SecretFile mSecretFile = null;

        private byte[] mEncryptedUsername = Array.Empty<byte>();
        private byte[] mEncryptedPassword = Array.Empty<byte>();
        private readonly Aes mAesEncryption = Aes.Create();

        public void GetSensibleInfosFromUser()
        {
            if (mSecretFile != null)
            {
                var credentials = mSecretFile.GetSSHCredentials().Split(";");
                mEncryptedUsername = SimpleEncryption.EncryptStringToBytes(credentials[0], mAesEncryption.Key, mAesEncryption.IV);
                mEncryptedPassword = SimpleEncryption.EncryptStringToBytes(credentials[1], mAesEncryption.Key, mAesEncryption.IV);

            }
        }

        public List<string> GetUsers(string filerIP)
        {
            List<string> users = new();

            string sshUsername = SimpleEncryption.DecryptStringFromBytes(mEncryptedUsername, mAesEncryption.Key, mAesEncryption.IV);
            string sshPassword = SimpleEncryption.DecryptStringFromBytes(mEncryptedPassword, mAesEncryption.Key, mAesEncryption.IV);
            using (var shell = new SshClient(filerIP, sshUsername, sshPassword))
            {
                shell.Connect();

                var listCommand = shell.CreateCommand("sudo lid -g incomm");

                string list = listCommand.Execute();

                string line = "";
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i] == '\n')
                    {
                        users.Add(line);
                        line = "";
                    }
                    else
                    {
                        line += list[i];
                    }

                }

                shell.Disconnect();
            }

            return users;
        }

        public void CreateFTPAccess(string username, string password, string filerIP)
        {

            string sshUsername = SimpleEncryption.DecryptStringFromBytes(mEncryptedUsername, mAesEncryption.Key, mAesEncryption.IV);
            string sshPassword = SimpleEncryption.DecryptStringFromBytes(mEncryptedPassword, mAesEncryption.Key, mAesEncryption.IV);
            using (var shell = new SshClient(filerIP, sshUsername, sshPassword))
            {
                shell.Connect();

                var startCommand = shell.CreateCommand("echo 'Connecté sur " + filerIP + "'");
                var userCreationCommand = shell.CreateCommand("sudo useradd -g incomm -d /var/www -s /bin/false " + username);

                var passwordChangeCommand = shell.CreateCommand("echo '" + password + "\n" + password + "' | sudo passwd " + username + " --stdin");

                var endCommand = shell.CreateCommand("echo '" + username + " créé avec mot de passe : " + password + "'");

                startCommand.Execute();
                userCreationCommand.Execute();
                passwordChangeCommand.Execute();
                endCommand.Execute();

                shell.Disconnect();
            }
        }

        public void DeleteFTPAccess(string username, string filerIP)
        {

            string sshUsername = SimpleEncryption.DecryptStringFromBytes(mEncryptedUsername, mAesEncryption.Key, mAesEncryption.IV);
            string sshPassword = SimpleEncryption.DecryptStringFromBytes(mEncryptedPassword, mAesEncryption.Key, mAesEncryption.IV);
            using (var shell = new SshClient(filerIP, sshUsername, sshPassword))
            {
                shell.Connect();

                var startCommand = shell.CreateCommand("echo 'Connecté sur " + filerIP + "'");
                var userDeletionCommand = shell.CreateCommand("sudo userdel " + username);


                var endCommand = shell.CreateCommand("echo '" + username + " supprimé'");

                startCommand.Execute();
                userDeletionCommand.Execute();
                endCommand.Execute();

                shell.Disconnect();
            }
        }

    }

    /*-----Partie GUI-----*/
    public partial class Home : Window
    {
        private void InitializeManagers()
        {
            bool secretFound = SecretFile.CheckForValidSecretFile("secrets.txt");

            WelcomeDialog welcomeDialog = new(Environment.UserName, secretFound);
            welcomeDialog.ShowDialog();
            if (!secretFound) { Environment.Exit(1); }

            SecretFile secret = new();
            if (secretFound)
            {
                secret.ParseSecretFile("secrets.txt");
            }
            else { secret = null; }
            
            this.ADManager = new(secret);
            this.MicroManager = new(secret);
            this.LLManager = new(secret);
            this.SSHManager = new(secret);
        }

        private void ConnectivityCheckup()
        {
            try
            {
                this.LLManager.Connect();
            }
            catch (Exception e)
            {
                MessageBox.Show("Echec de connexion, veuillez vérifier sur quel réseau vous êtes connecté.\nVeuillez vous connecter sur IncommTech24 en priorité");
            }
        }
        public Home()
        {
            InitializeComponent();
            InitializeManagers();
            ConnectivityCheckup();
            this.DataContext = this;

            this.WindowState = WindowState.Maximized;
        }

        public ActiveDirectoryManager ADManager { get; set; }
        public MicrosoftManager MicroManager    { get; set; }
        public LeLogManager LLManager           { get; set; }
        public SecureShellManager SSHManager    { get; set; }

        private async void DebugTestClick(object sender, RoutedEventArgs e)
        {
            //List<string> toRecipients = new();
            //toRecipients.Add("schasseray@incomm.fr");
            //
            //List<string> ccRecipients = new();
            //toRecipients.Add("schasseray@incomm.fr");
            //await this.MicroManager.SendMail(this.MicroManager.GetApplication(), "Allo", "Ceci est un test", toRecipients, ccRecipients);

        }

        /*OVERALL INTERFACE*/

        private void MenuInterfaceFullscreen(object sender, RoutedEventArgs e)
        {
            if (this.WindowStyle == WindowStyle.SingleBorderWindow)
            { this.WindowState = WindowState.Maximized; this.WindowStyle = WindowStyle.None; }
            else
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }
        }

        private void MenuInterfaceCleanContent(object sender, RoutedEventArgs e)
        {
            ContentStack.Children.Clear();
        }

        private void MenuInterfaceCleanHistory(object sender, RoutedEventArgs e)
        {
            HistoryStack.Children.Clear();
        }
        private void MenuInterfaceCleanAll(object sender, RoutedEventArgs e)
        {
            ContentStack.Children.Clear();
            HistoryStack.Children.Clear();
        }

        /*AIO*/

        private async void MenuAIOCreateUser(object sender, RoutedEventArgs e)
        {
            int basicCount = await this.MicroManager.GetRemainingAvailableLicenseCount(this.MicroManager.GetApplication(), this.MicroManager.mM365BusinessBsc);
            int standardCount = await this.MicroManager.GetRemainingAvailableLicenseCount(this.MicroManager.GetApplication(), this.MicroManager.mM365BusinessStd);

            AIOCreateUserDialog createUserDialog = new(this.LLManager.GetStaffServicesAsList(), this.LLManager.GetAgencesAsList(),
                                                       this.ADManager.GetAllServicesAsList(), this.MicroManager.GetLicensesAsList(),
                                                       basicCount, standardCount);
                                                       

            if (createUserDialog.ShowDialog() == true)
            {
                string prenom = createUserDialog.Prenom.Text;
                string nom    = createUserDialog.Nom.Text;
                string username = prenom[0].ToString().ToLower() + nom.ToLower();

                bool mailToSupport = createUserDialog.CheckBoxMailToSupport.IsChecked == true;

                string llService = createUserDialog.ComboLLServices.SelectedItem.ToString();
                string llAgence  = createUserDialog.ComboLLAgences.SelectedItem.ToString();
                string dateIn    = createUserDialog.DateIn.Text.ToString();
                string dateOut   = createUserDialog.DateOut.Text.ToString();
                bool suivi       = createUserDialog.CheckboxSuivi.IsChecked == true;
                bool stagiaire   = createUserDialog.CheckBoxStagiaire.IsChecked == true;
                bool nonPhysique = createUserDialog.CheckBoxNonPhysique.IsChecked == true;

                bool adCreation  = createUserDialog.CheckBoxAD.IsChecked == true;
                string adService = createUserDialog.ComboADServices.SelectedItem.ToString();

                bool microCreation  = createUserDialog.CheckBoxMicro.IsChecked == true;
                string microLicence = createUserDialog.ComboMicroLicences.SelectedItem.ToString();
                string microPassword = this.MicroManager.GenerateOfficePassword();

                bool ftpCreation = createUserDialog.CheckBoxFTP.IsChecked == true;
                string ftpPassword = SimpleEncryption.GenerateRandomPassword(8, false);

                string informations = String.Format("{0} {1}\nPrénom : {2}\nNom : {3}\n\n" +
                                                    "LeLog :\nService : {4}\nAgence : {5}\nEntrée : {6}\nSortie : {7}\nSuivi/Stagiaire/NonPhysique : {8}/{9}/{10}\n\n" +
                                                    "Active Directory : {11}\nService : {12}\n\n" +
                                                    "Microsoft : {13}\nLicence : {14}\nMot de passe : {15}\n\n" +
                                                    "FTP : {16}\nMot de passe : {17}",
                                                    prenom, nom, prenom, nom,
                                                    llService, llAgence, dateIn, dateOut, suivi, stagiaire, nonPhysique,
                                                    adCreation, adService,
                                                    microCreation, microLicence, microPassword,
                                                    ftpCreation, ftpPassword);

                ConfirmationDialog confirmationDialog = new("Voulez vous créer un utilisateur avec les informations suivantes :\n" + informations,
                                                            "Créer l'utilisateur", "Ne pas créer l'utilisateur");

                if (confirmationDialog.ShowDialog() == true)
                {
                    if (adCreation)
                    {
                        try
                        {
                            this.ADManager.CreateUserInService(prenom, nom, adService);
                        }
                        catch(Exception)
                        {
                            informations += "\nEchec de la création sur l'Active Directory, l'utilisateur doit déjà exister, sinon contacter l'autorité compétente";
                        }
                    }

                    if (microCreation)
                    {
                        
                        await this.MicroManager.CreateUser(this.MicroManager.GetApplication(), prenom, nom, microPassword);

                        if (microLicence == "Licence Basic")
                        {
                            await this.MicroManager.AssignLicenses(this.MicroManager.GetApplication(), prenom, nom, this.MicroManager.mM365BusinessBsc);
                        }
                        else if (microLicence == "Licence Standard")
                        {
                            await this.MicroManager.AssignLicenses(this.MicroManager.GetApplication(), prenom, nom, this.MicroManager.mM365BusinessStd);
                        }
                    }

                    if (ftpCreation)
                    {
                        try
                        {
                            this.SSHManager.CreateFTPAccess(username, ftpPassword, "xxxxxx");
                            this.SSHManager.CreateFTPAccess(username, ftpPassword, "xxxxxx");
                            this.SSHManager.CreateFTPAccess(username, ftpPassword, "xxxxxx");
                        }
                        catch (Exception)
                        {
                            informations += "\nEchec de la création de l'utilisateur sur les filers FTP, soit un problème de connexion SSH, soit l'utilisateur est déjà present";
                        }
                    }

                    this.LLManager.CreateStaffMember(prenom, nom, llService, suivi, llAgence, stagiaire, ftpPassword, dateIn, dateOut, microPassword, nonPhysique);

                    ConfirmationDialog saveConfirmationDialog = new("Voulez vous enregistrer ces informations", "Enregistrer", "Ne pas enregistrer");
                    if (saveConfirmationDialog.ShowDialog() == true)
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.FileName = String.Format("{0} {1} Création.txt", prenom, nom);
                        saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        saveFileDialog.Filter = "All files(*.txt) | *.txt";
                        if (saveFileDialog.ShowDialog() == true)
                            System.IO.File.WriteAllText(saveFileDialog.FileName, informations);
                    }

                    List<string> toRecipients = new();
                    List<string> ccRecipients = new();
                    toRecipients.Add("support@incomm.fr");
                    ccRecipients.Add("schasseray@incomm.fr");

                    if (mailToSupport)
                    {
                        await this.MicroManager.SendMail(this.MicroManager.GetApplication(), String.Format("Création de {0} {1} en {2} à {3}", prenom, nom, llService, llAgence),
                                                     informations, toRecipients, ccRecipients);
                    }
                    

                    UserControls.ContentDisplay informationsContentDisplay = new("Création d'un utilisateur/nouvel entrant",
                                                                                 informations, NotepadEditor);

                    HistoryStack.Children.Add(informationsContentDisplay);
                }

                MessageBox.Show("Utilisateur créé");
            }
        }

        private async void MenuAIODeleteUser(object sender, RoutedEventArgs e)
        {
            List<string> microUsers = await this.MicroManager.GetAllUsersAsList(this.MicroManager.GetApplication());
            AIODeleteUserDialog deleteUserDialog = new(this.LLManager.GetStaffMembersAsList(), this.ADManager.GetAllUsersAsList(),
                                                       microUsers, this.SSHManager.GetUsers("xxxxx"));

            if (deleteUserDialog.ShowDialog() == true)
            {
                bool convertToExStaff = deleteUserDialog.RadioExStaff.IsChecked == true;
                string llUserInfos = deleteUserDialog.ComboLLUsers.SelectedItem.ToString();
                string prenom = llUserInfos.Split("|")[0].Trim().Split("_")[0];
                string nom = llUserInfos.Split("|")[0].Trim().Split("_")[1];

                //string command = String.Format("SELECT id FROM staff WHERE prenom_staff={0} AND nom_staff={1}", prenom, nom);
                //this.LLManager.GetDataFromQuery(command);

                bool adDeletion = deleteUserDialog.CheckBoxAD.IsChecked == true;
                string adUserFullName = deleteUserDialog.ComboADUsers.SelectedItem.ToString();
                string sam = adUserFullName.Split("|")[1].Trim();

                bool microDeletion = deleteUserDialog.CheckBoxMicro.IsChecked == true;
                string microUserFullName = deleteUserDialog.ComboMicroUsers.SelectedItem.ToString();
                string mail = microUserFullName.Split("|")[1].Trim();

                bool ftpDeletion = deleteUserDialog.CheckBoxFTP.IsChecked == true;
                string ftpUserInfos = deleteUserDialog.ComboFTPUsers.SelectedItem.ToString();
                string ftpUsername = ftpUserInfos.Split("(")[0].Trim();

                string informations = String.Format("Lelog :{0}\n{1}\n\nActive Directory : {2}\n{3}\n\n" +
                                                    "Microsoft : {4}\n{5}\n\nFTP : {6}\n{7}\n\nSuppression par {8}",
                                                    convertToExStaff ? "Passage en ex staff" : "Suppression", llUserInfos, adDeletion, adUserFullName, microDeletion, microUserFullName,
                                                    ftpDeletion, ftpUserInfos, Environment.UserName);

                ConfirmationDialog confirmationDialog = new(String.Format("Voulez vous {0} sur :\n{1}", adUserFullName, informations),
                                                                          "Supprimer l'utilisateur", "Ne pas supprimer l'utilisateur");

                if (confirmationDialog.ShowDialog() == true)
                {
                    ConfirmationDialog saveConfirmationDialog = new("Voulez vous enregistrer ces informations", "Enregistrer", "Ne pas enregistrer");
                    if (saveConfirmationDialog.ShowDialog() == true)
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.FileName = String.Format("{0} {1} Suppression.txt", prenom, nom);
                        saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        saveFileDialog.Filter = "All files(*.txt) | *.txt";
                        if (saveFileDialog.ShowDialog() == true)
                            System.IO.File.WriteAllText(saveFileDialog.FileName, informations);
                    }

                    this.LLManager.DeleteStaffMember(prenom, nom, convertToExStaff);
                    
                    if (adDeletion)
                    {
                        this.ADManager.DeleteUser(sam);
                    }
                    
                    if (microDeletion)
                    {
                        await this.MicroManager.DeleteUserFromMail(this.MicroManager.GetApplication(), mail);
                    }
                    
                    if (ftpDeletion)
                    {
                        this.SSHManager.DeleteFTPAccess(ftpUsername, "xxxxx");
                        this.SSHManager.DeleteFTPAccess(ftpUsername, "xxxxx");
                        this.SSHManager.DeleteFTPAccess(ftpUsername, "xxxxx");
                    }

                    UserControls.ContentDisplay deletionContentDisplay = new(String.Format("Suppression d'un utilisateur : {0}", adUserFullName), informations, NotepadEditor);

                    HistoryStack.Children.Add(deletionContentDisplay);
                }
                MessageBox.Show("Utilisateur supprimé");
            }
        }

        /*NOTEPAD EDITOR MENU ITEM*/

        private void NotepadEditorSelectionChanged(object sender, RoutedEventArgs e)
        {

            int row = NotepadEditor.GetLineIndexFromCharacterIndex(NotepadEditor.CaretIndex);
            int col = NotepadEditor.CaretIndex - NotepadEditor.GetCharacterIndexFromLineIndex(row);

            string currentRunAs = Environment.UserName;

            var now = DateTime.Now;
            string lastInteraction = String.Format("{0}:{1}:{2}", now.Hour.ToString(), now.Minute.ToString(), now.Second.ToString());

            //var wlan = new WlanClient();
            //
            //List<String> allConnectedSSIDs = new List<string>();
            
            

            //foreach (WlanClient.WlanInterface wlanInterface in wlan.Interfaces)
            //{
            //    Wlan.Dot11Ssid ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
            //    allConnectedSSIDs.Add(new String(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength)));
            //}

            //string connectedSSID = allConnectedSSIDs[0];
            //
            //string currentDefaultDB = this.LLManager.GetDefaultDb();

            CursorPosition.Text = String.Format("Ligne {0}, Charactère {1} | Utilisateur : {2} | Dernière interaction à {3} ", (row + 1), (col + 1), currentRunAs, lastInteraction);
        }

        private void MenuSetFontToArial(object sender, RoutedEventArgs e)
        {
            NotepadEditor.FontFamily = new FontFamily("Arial");
        }

        private void MenuSetFontToGeorgia(object sender, RoutedEventArgs e)
        {
            NotepadEditor.FontFamily = new FontFamily("Georgia");
        }

        private void MenuSetFontToComicSansMs(object sender, RoutedEventArgs e)
        {
            NotepadEditor.FontFamily = new FontFamily("Comic Sans Ms");
        }

        private void MenuIncreaseFontSize(object sender, RoutedEventArgs e)
        {
            NotepadEditor.FontSize += 1;
        }

        private void MenuDecreaseFontSize(object sender, RoutedEventArgs e)
        {
            NotepadEditor.FontSize -= 1;
        }

        private void MenuSetFontSizeTo12(object sender, RoutedEventArgs e)
        {
            NotepadEditor.FontSize = 12;
        }

        private void MenuSetFontSizeTo24(object sender, RoutedEventArgs e)
        {
            NotepadEditor.FontSize = 24;
        }

        private void MenuSetFontSizeTo32(object sender, RoutedEventArgs e)
        {
            NotepadEditor.FontSize = 32;
        }

        private void MenuSetFontSizeTo64(object sender, RoutedEventArgs e)
        {
            NotepadEditor.FontSize = 64;
        }

        private void MenuOpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "All files(*.*) | *.*";
            if (openFileDialog.ShowDialog() == true)
                NotepadEditor.Text = System.IO.File.ReadAllText(openFileDialog.FileName);
        }

        private void MenuSaveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "All files(*.*) | *.*";
            if (saveFileDialog.ShowDialog() == true)
                System.IO.File.WriteAllText(saveFileDialog.FileName, NotepadEditor.Text);
        }

        /*ACTIVE DIRECTORY*/

        private void MenuADDisplayServices(object sender, RoutedEventArgs e)
        {
            string services = this.ADManager.GetAllServices();

            ContentStack.Children.Add(new UserControls.ContentDisplay("Services de l'Active Directory", services, NotepadEditor));
        }

        private void MenuADDisplayUsersFromAbidjan(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("ABI");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service ABIDJAN", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromCommercial(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("COMMERCIAL");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service COMMERCIAL", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromCompta(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("COMPTA");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service COMPTABILITE", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromAdmin(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("ADMIN");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service ADMINISTRATIF", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromCom(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("COM");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service COMMUNICATION", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromCM(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("CM");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service COMMUNITY MANAGEMENT", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromDesign(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("DESIGN");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service DESIGN", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromDev(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("DEV");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service DEVELOPPEMENT", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromEdito(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("EDIT");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service EDITORIAL", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromTech(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("TECH");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service TECH", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromWeb(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("WEBMARKET");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service WEBMARKETING", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromLondon(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("LONDRES");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service LONDRES", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromRecrutement(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("RECRUTEMENT");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service RECRUTEMENT", users, NotepadEditor));
        }
        private void MenuADDisplayUsersFromSuivi(object sender, RoutedEventArgs e)
        {
            string users = this.ADManager.GetAllUsersFromService("SUIVI");

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du service SUIVI CLIENTS", users, NotepadEditor));
        }

        private void MenuADCreateUser(object sender, RoutedEventArgs e)
        {
            ADCreateUserDialog createUserDialog = new(this.ADManager.GetAllServicesAsList());

            if (createUserDialog.ShowDialog() == true)
            {
                if (this.ADManager.CheckForValidService(createUserDialog.ServicesCombo.SelectedItem.ToString()))
                {
                    string prenom  = createUserDialog.Prenom.Text;
                    string nom     = createUserDialog.Nom.Text;
                    string service = createUserDialog.ServicesCombo.SelectedItem.ToString();

                    ConfirmationDialog confirmationDialog = new(String.Format("Voulez vous créer \"{0} {1}\" du service \"{2}\" ?", prenom, nom, service),
                                                                "Créer l'utilisateur", "Ne pas créer l'utilisateur");
                    if (confirmationDialog.ShowDialog() == true)
                    {
                        this.ADManager.CreateUserInService(createUserDialog.Prenom.Text, createUserDialog.Nom.Text, createUserDialog.ServicesCombo.SelectedItem.ToString());
                        MessageBox.Show("Utilisateur créé !");

                        UserControls.ContentDisplay history = new("Création d'un utilisateur AD",
                                                              String.Format("{0} {1}\nService : {2}\nCréé par ", prenom, nom, service), NotepadEditor);

                        HistoryStack.Children.Add(history);
                    }
                    
                    
                }
            }
        }

        private void MenuADDeleteUser(object sender, RoutedEventArgs e)
        {
            ADDeleteUserDialog deleteUserDialog = new(this.ADManager.GetAllServicesAsList(), this.ADManager);

            if (deleteUserDialog.ShowDialog() == true)
            {
                if (this.ADManager.CheckForValidService(deleteUserDialog.ServicesCombo.SelectedItem.ToString()))
                {
                    var prenomNom = deleteUserDialog.UsersCombo.SelectedItem.ToString().Split(" ");
                    string prenom = prenomNom[0];
                    string nom = prenomNom[1];
                    string service = deleteUserDialog.ServicesCombo.SelectedItem.ToString();

                    ConfirmationDialog confirmationDialog = new(String.Format("Voulez vous supprimer \"{0}\" du service \"{1}\" ?", deleteUserDialog.UsersCombo.SelectedItem.ToString(), service),
                                                                "Supprimer l'utilisateur", "Ne pas supprimer l'utilisateur");
                    if (confirmationDialog.ShowDialog() == true)
                    {
                       
                        this.ADManager.DeleteUserInService(prenom, nom, deleteUserDialog.ServicesCombo.SelectedItem.ToString());
                        MessageBox.Show("Utilisateur supprimé !");

                        UserControls.ContentDisplay history = new("Suppression d'un utilisateur AD",
                                                              String.Format("{0} {1}\nService : {2}\nSuppression par {3}", prenom, nom, service, Environment.UserName), NotepadEditor);

                        HistoryStack.Children.Add(history);
                    }


                }
            }
        }

        /*Microsoft manager*/
        private async void MenuMicroDisplayUsers(object sender, RoutedEventArgs e)
        {
            string users = await this.MicroManager.GetAllUsers(this.MicroManager.GetApplication());

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs microsoft", users, NotepadEditor));
        }

        private async void MenuMicroDisplayUserInfos(object sender, RoutedEventArgs e)
        {
            InputDialog usernameInputDialog = new("Prénom et nom de l'utilisateur :");
            if (usernameInputDialog.ShowDialog() == true)
            {
                if (usernameInputDialog.Answer.Text.Length > 3 && usernameInputDialog.Answer.Text.Contains(" ") && usernameInputDialog.Answer.Text.Split(" ")[0].Length > 1 && usernameInputDialog.Answer.Text.Split(" ")[1].Length > 1)
                {
                    var userInfos = await this.MicroManager.GetUserInfos(this.MicroManager.GetApplication(), usernameInputDialog.Answer.Text);
                    ContentStack.Children.Add(new UserControls.ContentDisplay(usernameInputDialog.Answer.Text, userInfos, NotepadEditor));
                }
                else
                {
                    MessageBox.Show("Veuillez entrer une réponse valide !", "Erreur :(");
                }
            }
        }

        private async void MenuMicroCreateUser(object sender, RoutedEventArgs e)
        {
            int basicCount = await this.MicroManager.GetRemainingAvailableLicenseCount(this.MicroManager.GetApplication(), this.MicroManager.mM365BusinessBsc);
            int standardCount = await this.MicroManager.GetRemainingAvailableLicenseCount(this.MicroManager.GetApplication(), this.MicroManager.mM365BusinessStd);

            MicroCreateUserDialog createUserDialog = new(this.MicroManager.GetLicensesAsList(), basicCount, standardCount);

            if (createUserDialog.ShowDialog() == true)
            {
                string prenom = createUserDialog.Prenom.Text;
                string nom = createUserDialog.Nom.Text;
                string mail = prenom[0].ToString().ToLower() + nom.ToLower() + "@incomm.fr";
                string license = createUserDialog.LicensesCombo.SelectedItem.ToString();
                
                string pwd = this.MicroManager.GenerateOfficePassword();
                ConfirmationDialog confirmationDialog = new(String.Format("Voulez vous créer {0} avec les infos suivantes :\nMail : {1}\nLicence : {2}\nMDP : {3}", prenom + " " + nom, mail, license, pwd),
                                                            "Créer l'utilisateur", "Ne pas créer l'utilisateur");

                if (confirmationDialog.ShowDialog() == true)
                {
                    ConfirmationDialog saveConfirmationDialog = new("Voulez vous enregistrer les informations liées à l'utilisateur",
                                                                    "Enregister les informations", "Ne pas enregistrer les informations");

                    string informations = String.Format("{0} {1}\nPrénom : [{2}]\nNom : [{3}]\nMail : [{4}]\nLicence : [{5}]\nPwd : [{6}]\n\nCréé par {7}",
                                                            prenom, nom, prenom, nom, mail, license, pwd, Environment.UserName);

                    if (saveConfirmationDialog.ShowDialog() == true)
                    {
                        

                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        saveFileDialog.Filter = "All files(*.*) | *.*";
                        if (saveFileDialog.ShowDialog() == true)
                            System.IO.File.WriteAllText(saveFileDialog.FileName, informations);
                    }

                    await this.MicroManager.CreateUser(this.MicroManager.GetApplication(), prenom, nom, pwd);

                    if (license == "Licence Basic") 
                    {
                        await this.MicroManager.AssignLicenses(this.MicroManager.GetApplication(), prenom, nom, this.MicroManager.mM365BusinessBsc);
                    }
                    else if(license == "Licence Standard") 
                    {
                        await this.MicroManager.AssignLicenses(this.MicroManager.GetApplication(), prenom, nom, this.MicroManager.mM365BusinessStd);
                    }
                    

                    MessageBox.Show("Utilisateur créé !");

                    UserControls.ContentDisplay history = new("Création d'un utilisateur Microsoft",
                                                              informations, NotepadEditor);

                    HistoryStack.Children.Add(history);
                }
            }
            
        }

        private async void MenuMicroDeleteUser(object sender, RoutedEventArgs e)
        {
            var users = await this.MicroManager.GetAllUsersAsList(this.MicroManager.GetApplication());
            MicroDeleteUserDialog deleteUserDialog = new(this.MicroManager, users);

            if (deleteUserDialog.ShowDialog() == true)
            {
                var nameMail = deleteUserDialog.UsersCombo.SelectedItem.ToString().Split("|");
                string prenom = nameMail[0].Split(" ")[0].Trim();
                string nom = nameMail[0].Split(" ")[1].Trim();
                string mail   = nameMail[1].Trim();
                ConfirmationDialog confirmationDialog = new(String.Format("Voulez vous supprimer {0} au mail associé suivant : {1}", prenom + " " + nom, mail),
                                                            "Supprimer l'utilisateur", "Ne pas supprimer l'utilisateur");

                if (confirmationDialog.ShowDialog() == true)
                {
                    await this.MicroManager.DeleteUser(this.MicroManager.GetApplication(), prenom + " " + nom);
                    MessageBox.Show("Utilisateur supprimé !");

                    UserControls.ContentDisplay history = new("Suppression d'un utilisateur Microsoft",
                                                              String.Format("Suppression de {0} {1}\nPrénom : {2}\nNom : {3}\nMail : {4}\nSuppression par {5}",prenom, nom, prenom, nom, mail, Environment.UserName)
                                                              , NotepadEditor);

                    HistoryStack.Children.Add(history);
                }
            }
        }

        /*LeLog*/

        private void MenuLLImportFiche(object sender, RoutedEventArgs e)
        {
            string id = "";
            InputDialog idInputDialog = new("Veuillez entrer l'ID de la fiche");
            if (idInputDialog.ShowDialog() == true)
            {
                id = idInputDialog.Answer.Text.Trim();
                if (!this.LLManager.CheckForValidID(id))
                {
                    MessageBox.Show("Veuillez entrer un ID de fiche valide");
                    return;
                }
            }

            this.LLManager.Connect();
            string retrieveFicheContentCommand = String.Format("SELECT desc_int FROM interventions WHERE id={0}", id);
            var ficheContent = this.LLManager.GetDataFromQuery(retrieveFicheContentCommand)[0][0];
            this.LLManager.Disconnect();


            string toBeDisplayed = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\" /></head><body>";
            toBeDisplayed += ficheContent;
            toBeDisplayed += "</body></html>";

            WebBrowser ficheViewer = new();
            ficheViewer.NavigateToString(toBeDisplayed);
            ficheViewer.Width = 400;
            ficheViewer.Height = 300;
            ContentStack.Children.Add(ficheViewer);
        }
        private void MenuLLDisplayAllStaffUsers(object sender, RoutedEventArgs e)
        {
            bool success = false;
            try
            {
                 success = this.LLManager.Connect();
            }
            catch (Exception exce)
            {
                MessageBox.Show(exce.ToString());
                Clipboard.SetText(exce.ToString());
            }

            string staffUsers = this.LLManager.GetDataAsString(this.LLManager.GetInfosAboutAllStaffMembers());
            this.LLManager.Disconnect();

            ContentStack.Children.Add(new UserControls.ContentDisplay("Utilisateurs du Staff :", staffUsers, NotepadEditor));
        }

        private void MenuLLDisplayUserInfos(object sender, RoutedEventArgs e)
        {
            InputDialog usernameInputDialog = new("Prénom et nom de l'utilisateur :");
            if (usernameInputDialog.ShowDialog() == true)
            {
                if (usernameInputDialog.Answer.Text.Length > 3 && usernameInputDialog.Answer.Text.Contains(" ") && usernameInputDialog.Answer.Text.Split(" ")[0].Length > 1 && usernameInputDialog.Answer.Text.Split(" ")[1].Length > 1)
                {
                    bool success = false;
                    try
                    {
                        success = this.LLManager.Connect();
                    }
                    catch (Exception exce)
                    {
                        MessageBox.Show(exce.ToString());
                        Clipboard.SetText(exce.ToString());
                    }

                    var userInfos = this.LLManager.GetDataAsString(this.LLManager.GetInfosAboutStaffMember(usernameInputDialog.Answer.Text));

                    this.LLManager.Disconnect();
                    ContentStack.Children.Add(new UserControls.ContentDisplay(usernameInputDialog.Answer.Text, userInfos, NotepadEditor));
                }
                else
                {
                    MessageBox.Show("Veuillez entrer une réponse valide !", "Erreur :(");
                }
            }
        }

        private void MenuLLDisplayServices(object sender, RoutedEventArgs e)
        {
            bool success = false;
            try
            {
                success = this.LLManager.Connect();
            }
            catch (Exception exce)
            {
                MessageBox.Show(exce.ToString());
                Clipboard.SetText(exce.ToString());
            }

            string services = this.LLManager.GetDataAsString(this.LLManager.GetStaffServices());
            this.LLManager.Disconnect();

            ContentStack.Children.Add(new UserControls.ContentDisplay("Services :", services, NotepadEditor));
        }

        private void MenuLLDisplayAgences(object sender, RoutedEventArgs e)
        {
            bool success = false;
            try
            {
                success = this.LLManager.Connect();
            }
            catch (Exception exce)
            {
                MessageBox.Show(exce.ToString());
                Clipboard.SetText(exce.ToString());
            }

            string agences = this.LLManager.GetDataAsString(this.LLManager.GetAgences());
            this.LLManager.Disconnect();

            ContentStack.Children.Add(new UserControls.ContentDisplay("Agences :", agences, NotepadEditor));
        }

        private void MenuLLCreateUser(object sender, RoutedEventArgs e)
        {
            bool success = false;
            try
            {
                success = this.LLManager.Connect();
            }
            catch (Exception exce)
            {
                MessageBox.Show(exce.ToString());
                Clipboard.SetText(exce.ToString());
            }

            LLCreateUserDialog createUserDialog = new(this.LLManager.GetAgencesAsList(), this.LLManager.GetStaffServicesAsList());

            if (createUserDialog.ShowDialog() == true)
            {
                string prenom = createUserDialog.Prenom.Text.Trim();
                string nom = createUserDialog.Nom.Text.Trim();
                string agence = createUserDialog.ComboAgences.SelectedItem.ToString();
                string service = createUserDialog.ComboServices.SelectedItem.ToString();

                bool suivi     = createUserDialog.CheckboxSuivi.IsChecked == true ? true : false ;
                bool stagiaire = createUserDialog.CheckBoxStagiaire.IsChecked == true ? true : false;
                bool nonPhysique = createUserDialog.CheckBoxNonPhysique.IsChecked == true ? true : false;

                string emailPwd = createUserDialog.EmailPasswordInfo.Text;
                string ftpPwd = createUserDialog.FTPPasswordInfo.Text;

                string dateIn = createUserDialog.DateIn.Text;
                string dateOut = createUserDialog.DateOut.Text;

                string informations = String.Format("[{0} {1}]\nAgence : [{2}]\nService : [{3}]\nDétails : [Suivi = {4} | Stagiaire = {5} | NonPhysique = {6}]\nEmail mdp : [{7}]\nFTP mdp : [{8}]\nDates : [{9}] à [{10}]",
                                                                          prenom, nom, agence, service, suivi, stagiaire, nonPhysique, emailPwd, ftpPwd, dateIn, dateOut);

                ConfirmationDialog confirmationDialog = new("Voulez vous créer le nouveau membre du staff suivant :\n" + informations, "Créer le nouveaux membre", "Ne pas créer le nouveau membre");

                if (confirmationDialog.ShowDialog() == true)
                {
                    this.LLManager.CreateStaffMember(prenom, nom, service, suivi, agence, stagiaire, ftpPwd, dateIn, dateOut, emailPwd, nonPhysique);
                    MessageBox.Show("Utilisateur créé");

                    UserControls.ContentDisplay history = new("Création d'un utilisateur/staff member LeLog",
                                                              informations, NotepadEditor);

                    HistoryStack.Children.Add(history);
                }
            }

            this.LLManager.Disconnect();
        }

        private void MenuLLDeleteUser(object sender, RoutedEventArgs e)
        {
            LLDeleteUserDialog deleteUserDialog = new(this.LLManager.GetStaffServicesAsList(), this.LLManager.GetStaffMembersAsList());

            if (deleteUserDialog.ShowDialog() == true)
            {
                var user = deleteUserDialog.ComboUsers.SelectedItem.ToString();

                string userFullName = user.Split("|")[0].Trim();
                string prenom = userFullName.Split("_")[0];
                string nom = userFullName.Split("_")[1];
                string infosSup     = user.Split("|")[0].Trim();
                bool convertToExStaff = deleteUserDialog.RadioExStaff.IsChecked == true ? true : false;

                string question = convertToExStaff ? "Voulez vous passer en ex staff" : "Voulez vous supprimer";
                string answer = convertToExStaff ? "Passer en ex staff" : "Supprimer";

                ConfirmationDialog confirmationDialog = new(String.Format("{0} {1}", question, userFullName),
                                                            String.Format("{0}", answer),
                                                            String.Format("Ne pas {0}", answer));

                string informations = String.Format("{0}\nInfos : {1}\nStatut : {2}\n{2} par {3}",
                                                    userFullName, infosSup, convertToExStaff ? "Passé en ex staff" : "Supprimé",
                                                    Environment.UserName);

                if (confirmationDialog.ShowDialog() == true)
                {
                    this.LLManager.DeleteStaffMember(prenom, nom, convertToExStaff);
                    MessageBox.Show(convertToExStaff ? "Utilisateur passé en ex staff" : "Utilisateur supprimé");

                    UserControls.ContentDisplay history = new(String.Format("{0} d'un utilisateur/staff member LeLog", convertToExStaff ? "Passage en ex staff" : "Suppression"),
                                                              informations, NotepadEditor);

                    HistoryStack.Children.Add(history);
                }
            }
        }

        private void MenuLLSetDefaultDB(object sender, RoutedEventArgs e)
        {
            if (this.LLManager.GetDefaultDb() == "prod") 
            {
                this.LLManager.SetDebugDbAsDefaultDb();
            }
            else
            {
                this.LLManager.SetProdDbAsDefaultDb();
            }
        }

        /*SSH*/

        private void MenuSSHCreateUser(object sender, RoutedEventArgs e)
        {
            SSHCreateUserDialog createUserDialog = new();

            if (createUserDialog.ShowDialog() == true)
            {
                string prenom = createUserDialog.Prenom.Text;
                string nom = createUserDialog.Nom.Text;
                string username = prenom[0].ToString().ToLower() + nom.ToLower();
                string password = SimpleEncryption.GenerateRandomPassword(8, false);

                ConfirmationDialog confirmationDialog = new(String.Format("Voulez vous créer l'utilisateur FTP suivant :\n{0} {1}\nMot de passe : {2}", prenom, nom, password),
                                                            "Créer l'utilisateur", "Ne pas créer l'utilisateur");

                if (confirmationDialog.ShowDialog() == true)
                {
                    ConfirmationDialog saveConfirmationDialog = new("Voulez vous enregistrer les informations de l'utilisateur ?",
                                                                    "Enregister les informations", "Ne pas enregistrer les informations");

                    string informations = String.Format("{0} {1}\nPrénom : {2}\nNom : {3}\nMot de passe : {4}\nCréé par {5}",
                                                     prenom, nom, prenom, nom, password, Environment.UserName);

                    if (saveConfirmationDialog.ShowDialog() == true)
                    {
                        

                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        saveFileDialog.Filter = "All files(*.*) | *.*";
                        if (saveFileDialog.ShowDialog() == true)
                            System.IO.File.WriteAllText(saveFileDialog.FileName, informations);
                    }

                    this.SSHManager.CreateFTPAccess(username, password, "xxxxx");
                    this.SSHManager.CreateFTPAccess(username, password, "xxxxx");
                    this.SSHManager.CreateFTPAccess(username, password, "xxxxx");

                    MessageBox.Show("Utilisateur créé");

                    UserControls.ContentDisplay history = new("Création d'un utilisateur FTP/Création des accès FTP pour un utilisateur",
                                                              informations, NotepadEditor);

                    HistoryStack.Children.Add(history);
                }
            }
        }

        private void MenuSSHDeleteUser(object sender, RoutedEventArgs e)
        {
            SSHDeleteUserDialog deleteUserDialog = new(this.SSHManager.GetUsers("xxxxxx"));

            if (deleteUserDialog.ShowDialog() == true)
            {
                var data = deleteUserDialog.ComboUsers.SelectedItem.ToString().Split("(");
                string username = data[0];
                string id       = data[1].Replace(")", "");

                ConfirmationDialog confirmationDialog = new(String.Format("Voulez vous supprimer l'utilisateur suivant :\n{0}", username),
                                                            "Supprimer l'utilisateur", "Ne pas supprimer l'utilisateur");

                string informations = String.Format("{0}\n{1}\nSupprimé par {2}",
                                                    username, id, Environment.UserName);

                if (confirmationDialog.ShowDialog() == true)
                {
                    this.SSHManager.DeleteFTPAccess(username, "xxxxxx");
                    this.SSHManager.DeleteFTPAccess(username, "xxxxxx");
                    this.SSHManager.DeleteFTPAccess(username, "xxxxxx");

                    MessageBox.Show("Utilisateur supprimé");

                    UserControls.ContentDisplay history = new("Suppression d'un utilisateur FTP/Retrait des accès FTP pour un utilisateur",
                                                              informations, NotepadEditor);

                    HistoryStack.Children.Add(history);
                }
            }
        }

        private void MenuSSHDisplayUsers(object sender, RoutedEventArgs e)
        {
            var users = this.SSHManager.GetUsers("xxxxx");

            string allUsers = "";

            foreach(var user in users)
            {
                allUsers += user;
                allUsers += '\n';
            }

            UserControls.ContentDisplay usersContentDisplay = new("Liste des utilisateurs FTP", allUsers, NotepadEditor);
            ContentStack.Children.Add(usersContentDisplay);
        }

        /*MISC*/

        private void MenuMiscGeneratePassword(object sender, RoutedEventArgs e)
        {

        }

        /*HELP*/

        private void MenuHelpClick(object sender, RoutedEventArgs e)
        {
            string info = "L'interface se décompose en 3 éléments :\n" +
                          "Le menu (les onglets) où sont répertiorées les fonctions principales du Tool\n" +
                          "Le corps du tool où sont affichés le contenu à gauche sous forme de mini fenêtre intégrées, au milieu le bloc note géant et à droite l'historique des opérations importantes\n" +
                          "Le footer où sont écrites les informations pertinentes du tool, ces informations sont actualisées à chaque intéraction avec le bloc note (même un simple clic de souris)\n\n" +
                          "Comme dit précédemment, les fonctions principales sont repertoriées dans le Menu :\n" +
                          "ALL IN ONE : Les deux grands process pour les nouveaux entrants/sortants, rassemblant des fonctions des autres onglets\n" +
                          "ACTIVE DIRECTORY : Affichage des utilisateurs, de leurs infos, création et suppression sur l'AD\n" +
                          "MICROSOFT : Affichage des utilisateurs, de leurs infos, création, suppression et assignation de licences Microsoft\n" +
                          "LELOG : Affichage des utilisateurs, de leurs infos, création et suppression/passage en ex staff\n" +
                          "SSH : Création et suppression des utilisateurs sur les filers (accès et retrait)\n\n" +
                          "Pour toute information supplémentaire, demander au service TECH !";

            ConfirmationDialog confirmationDialog = new(info, "OK j'ai compris", "J'ai pas compris", true);
            if (confirmationDialog.ShowDialog() != true)
            {
                MessageBox.Show("Mes excuses, veuillez demander au service TECH pour plus d'informations, merci et bonne journée avec le Visual Automation Tool");
            }
        }
    }
}
