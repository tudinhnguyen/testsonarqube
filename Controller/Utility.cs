using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Reflection;
using Teechip.View;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Threading;
using System.ServiceProcess;
using System.Configuration.Install;
using Microsoft.Win32;

namespace Teechip.Controller
{
    class Utility
    {
        public static void CreateRegKey(string strKey, string strParentKey = @"MIME\Database\Content Type")
        {
            RegistryKey key;
            key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(string.Format(@"{0}\{1}", strParentKey, strKey), true);

            if (key == null)
            {
                key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(strParentKey, true);
                key = key.CreateSubKey(strKey);
            }

            key.SetValue("CLSID", "{25336920-03F9-11cf-8FD0-00AA00686F13}", RegistryValueKind.String);
            key.SetValue("Encoding", new byte[] { 08, 00, 00, 00 }, RegistryValueKind.Binary);

            key.Close();
        }

        // [Obfuscation(Feature = "virtualization", Exclude = false)]
        private const int PBKDF2IterCount = 1000; // default for Rfc2898DeriveBytes
        // [Obfuscation(Feature = "virtualization", Exclude = false)]
        private const int PBKDF2SubkeyLength = 256 / 8; // 256 bits
        // [Obfuscation(Feature = "virtualization", Exclude = false)]
        private const int SaltSize = 128 / 8; // 128 bits

        // [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static DateTime ExpireDate = DateTime.Now.AddDays(1);

        internal static Thread m_oThread = null;

        // [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static bool ServiceMode = false;
        public static bool SaveProfit = false;
        
        public static bool IsServiceMode = false;

        public static int ServiceModeUserID = -1;

       
        internal static void StopThread()
        {
            if (m_oThread != null)
            {
                m_oThread.Abort();
                m_oThread = null;
            }
        }

       
        static internal void SaveAccount(string file, string username, string pwd)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(file))
                {
                    sw.WriteLine(Utility.Encrypt(username));
                    sw.WriteLine(Utility.Encrypt(pwd));
                }
            }
            catch
            {
            }
        }

        static internal bool LoadAccount(string file, ref string username, ref string pwd)
        {
            try
            {
                if (File.Exists(file))
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        username = Utility.Decrypt(sr.ReadLine());
                        pwd = Utility.Decrypt(sr.ReadLine());
                    }
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        static internal void SaveAccount(string file, string username, string pwd, int type)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(file))
                {
                    sw.WriteLine(Utility.Encrypt(username));
                    sw.WriteLine(Utility.Encrypt(pwd));
                    sw.WriteLine(Utility.Encrypt(type.ToString()));
                }
            }
            catch
            {
            }
        }

        static internal bool LoadAccount(string file, ref string username, ref string pwd, ref int type)
        {
            try
            {
                if (File.Exists(file))
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        username = Utility.Decrypt(sr.ReadLine());
                        pwd = Utility.Decrypt(sr.ReadLine());
                        type = int.Parse(Utility.Decrypt(sr.ReadLine()));
                    }
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        // [Obfuscation(Feature = "virtualization", Exclude = false)]
        internal static string HashPassword(string password)
        {
            byte[] salt;
            byte[] subkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, SaltSize, PBKDF2IterCount))
            {
                salt = deriveBytes.Salt;
                subkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
            }

            byte[] outputBytes = new byte[1 + SaltSize + PBKDF2SubkeyLength];
            Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);
            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SaltSize, PBKDF2SubkeyLength);
            return Convert.ToBase64String(outputBytes);
        }

        // [Obfuscation(Feature = "virtualization", Exclude = false)]
        private static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

            // Wrong length or version header.
            if (hashedPasswordBytes.Length != (1 + SaltSize + PBKDF2SubkeyLength) || hashedPasswordBytes[0] != 0x00)
                return false;

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, SaltSize);
            byte[] storedSubkey = new byte[PBKDF2SubkeyLength];
            Buffer.BlockCopy(hashedPasswordBytes, 1 + SaltSize, storedSubkey, 0, PBKDF2SubkeyLength);

            byte[] generatedSubkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, PBKDF2IterCount))
            {
                generatedSubkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
            }
            return storedSubkey.SequenceEqual(generatedSubkey);
        }

        static readonly string PasswordHash = "P@@Sw0rd";
        static readonly string SaltKey = "S@LT&KEY";
        static readonly string VIKey = "@1B2c3D4e5F6g7H8";
        static readonly string prefix = "123456789zaqwsxcderfvbgtyhnmjuiklop";
        static readonly string sufix = "zaqwsxlpokijmnhyutrfvcde212457900987654#$";

        public static string Encrypt(string plainText)
        {
            //int i, idx;
            //Random ran = new Random();
            //for (i = 0; i < 5; i++)
            //{
            //    idx = ran.Next(1, prefix.Length-1);
            //    plainText = prefix.Substring(idx, 1) + plainText;
            //}
            //for (i = 0; i < 10; i++)
            //{
            //    idx = ran.Next(1, sufix.Length - 1);
            //    plainText = plainText + sufix.Substring(idx, 1);
            //}

            //return plainText;
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string encryptedText)
        {
            //return encryptedText.Substring(5, encryptedText.Length - 10 - 5);

            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }


        #region Log functions

        public static void LogError(Exception ex)
        {
            LogError(ex.Message + "\r\n" + ex.StackTrace);
        }

        public static void LogError(string strMsg, string strFile = "log\\error.log", bool bAppend = true)
        {
            try
            {
                StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), strFile), bAppend);
                sw.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff: ") + strMsg);
                sw.Close();
            }
            catch
            {
            }
        }

        public static void LogInfo(string strMsg, string strFile = @"log\infor_log.log", bool bAppend = true, bool bAddTime = true)
        {
#if DEBUG
            try
            {
                StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), strFile), bAppend);
                if (bAddTime)
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff: ") + strMsg);
                }
                else
                {
                    sw.WriteLine(strMsg);
                }
                sw.Close();
            }
            catch
            {
            }
#endif
        }

        public static void LogInfoRelease(string strMsg, string strFile = @"log\infor_log.log", bool bAppend = true, bool bAddTime = true)
        {
            try
            {
                StreamWriter sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), strFile), bAppend);
                if (bAddTime)
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff: ") + strMsg);
                }
                else
                {
                    sw.WriteLine(strMsg);
                }
                sw.Close();
            }
            catch
            {
            }
        }

        #endregion Log functions
    }
}
