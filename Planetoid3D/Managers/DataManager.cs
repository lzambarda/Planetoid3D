using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
namespace Planetoid3D
{
    public static class DataManager
    {
        /// <summary>
        /// Convert an object to a byte array
        /// </summary>
        // public static byte[] ToByteArray(object source) {
        //     var formatter = new BinaryFormatter();
        //     using (var stream = new MemoryStream()) {
        //         formatter.Serialize(stream, source);
        //         return stream.ToArray();
        //     }
        // }

        /// <summary>
        /// Convert a byte array to an object
        /// </summary>
        // public static object ToObject(byte[] array) {
        //     var formatter = new BinaryFormatter();
        //     using (var stream = new MemoryStream(array)) {
        //         return formatter.Deserialize(stream);
        //     }
        // }

        /// <summary>
        /// The encryption key used for the cryptography methods
        /// </summary>
        private static string key = "6f31252bd26040ecb157f1c47676dfcb8c73f262e9984acbbb98243b853ab323";

        /// <summary>
        /// Shift every character of the text by the key, forward to encrypt and backward to decrypt
        /// </summary>
        private static string Transform(string text, int direction)
        {
            char[] content = text.ToCharArray();
            for (int a = 0; a < content.Length; a++)
            {
                content[a] = (char)(content[a] + direction * key[a % key.Length]);
            }
            return new string(content);
        }

        /// <summary>
        /// Read and decrypt a given text file, the file on disk is left untouched
        /// </summary>
        public static string ReadDecrypted(string filename)
        {
            return Transform(File.ReadAllText(filename), -1);
        }

        /// <summary>
        /// Encrypt a text and write it to the given file, replacing it only once fully written
        /// </summary>
        public static void WriteEncrypted(string filename, string text)
        {
            string temporary = filename + ".tmp";
            File.WriteAllText(temporary, Transform(text, 1));
            File.Move(temporary, filename, true);
            //Keep a backup copy of the settings
            if (filename == MenuManager.GetSettingPath())
            {
                File.Copy(filename, filename + "~1", true);
            }
        }

        /// <summary>
        /// Load an encrypted XML file into the given document, the file on disk is left untouched
        /// </summary>
        public static void LoadEncryptedXml(XmlDocument document, string filename)
        {
            document.LoadXml(ReadDecrypted(filename));
        }

        /// <summary>
        /// Encrypt the given document and save it to a file
        /// </summary>
        public static void SaveEncryptedXml(XmlDocument document, string filename)
        {
            WriteEncrypted(filename, document.OuterXml);
        }

        /// <summary>
        /// Encrypt a given plain text file in place
        /// </summary>
        public static void Encrypt(string filename)
        {
            if (File.Exists(filename))
            {
                WriteEncrypted(filename, File.ReadAllText(filename));
            }
        }
    }
}
