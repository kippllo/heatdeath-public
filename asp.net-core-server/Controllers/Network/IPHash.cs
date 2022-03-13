using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

/* Hashing Help:
    Object.GetHashCode (Note: Don't use this first one!): https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode?view=netframework-4.8#System_Object_GetHashCode
    KeyedHashAlgorithm: https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.keyedhashalgorithm?view=netframework-4.8
    HashAlgorithm Class: https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm?view=netframework-4.8
    HashAlgorithm.Create: https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.create?view=netframework-4.8#System_Security_Cryptography_HashAlgorithm_Create_System_String_
    SHA256 Class: https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256managed?view=netframework-4.8
    Base64 Convert:    https://docs.microsoft.com/en-us/dotnet/api/system.convert.tobase64string?view=netframework-4.8
    Hash vs Encryption: https://gcn.com/articles/2013/12/02/hashing-vs-encryption.aspx?m=1
*/


// This hashed IP works as a one-way client identifier. Only the client can say, "This is who I am."
// This is no way for my to look at the hashed IP and tell where a person lives or who they are. (Unless they use the same username every round, but then I would only know that exclusive piece of info.)
// I can't even use this hashed IP to track the user across different apps/games, as long I am sure to change the "hashSeed" for each application!
// This system provides complete client privacy, I don't know who my users are or where they live. But I can tell each user apart from any other user!

namespace Keybored.BackendServer.Network {

    public class IPHash {
        private const string hashSeed = "kVTgNM(V";
        private static List<string> bannedIPs = new List<string>();
        private string ipHash;

        public string ip {
            get{
                return ipHash; //This way it is readonly outside of this class!
            }
        }

        public IPHash(string IP) {
            //Never even save the unhashed ip to a property!
            ipHash = hashIP(IP);
        }

        public static string hashIP(string IP) {
            string compoundIP = hashSeed + IP;
            string output = "";
            using (SHA256 sha256 = SHA256.Create()  ){ //HashAlgorithm.Create("SHA256")
                byte[] stringBytes = Encoding.ASCII.GetBytes(compoundIP);
                byte[] hashedBytes = sha256.ComputeHash(stringBytes);
                
                output = Convert.ToBase64String(hashedBytes); //Convert the bytes into a string.
            }

            return output;
        }

        public bool isBanned {
            get {
                return bannedIPs.IndexOf(ipHash) != -1; //This may take too long with a lot of banned ip's?
            }
        }


        //TODO: Make a function to read the banned ip json file!
        // and find where to read it! (start of a game maybe?)
    }


}