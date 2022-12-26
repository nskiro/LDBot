using System;
using System.IO;
using System.Threading.Tasks;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace LDBotV2
{
    public class LicMan
    {
        //Config database access
        private static IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "maMhKzLKMu577YA82mA0TtYIhSeDv0yputCZHWAc",
            BasePath = "https://ld-bot-license-manager-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        private static IFirebaseClient client = new FireSharp.FirebaseClient(config);

        public static async Task<bool> checkLicense(string key)
        {
            try
            {
                if (client != null)
                {
                    FirebaseResponse response = await client.GetTaskAsync("Users/" + key);
                    if (response.Body != "null")
                        return true;
                    else
                        return false;
                }
                return false;
            }
            catch(Exception e)
            {
                ToolHelper.raiseOnWriteError(e);
                return false;
            }
        }

        public static void createLicense()
        {
            string key = HardwareInfo.getUniqueId();
            if (!File.Exists(key))
            {
                File.Create(key);
            }    
        }
    }
}
