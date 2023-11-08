using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace Planetoid3D {
    public static class DataManager {
        /// <summary>
        /// Convert an object to a byte array
        /// </summary>
        public static byte[] ToByteArray(object source) {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream()) {
                formatter.Serialize(stream, source);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Convert a byte array to an object
        /// </summary>
        public static object ToObject(byte[] array) {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(array)) {
                return formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// The encryption key used for the cryptography methods
        /// </summary>
        private static string key = "6f31252bd26040ecb157f1c47676dfcb8c73f262e9984acbbb98243b853ab323";


        /// <summary>
        /// Encrypt a given text file
        /// </summary>
        public static void Encrypt(string filename) {
            if (File.Exists(filename)) {
                StreamReader streamReader = new StreamReader(filename);
                char[] content = streamReader.ReadToEnd().ToCharArray();
                streamReader.Close();
                int b = 0;
                for (int a = 0; a < content.Length; a++) {
                    content[a] = (char)(content[a] + key[b]);
                    b++;
                    if (b == key.Length) {
                        b = 0;
                    }
                }
                //File.SetAttributes(filename, FileAttributes.Normal);
                StreamWriter streamWriter = new StreamWriter(filename);
                //File.SetAttributes(filename, FileAttributes.Hidden);
                streamWriter.Write(content, 0, content.Length);
                streamWriter.Close();
                //Temprary File
                if (filename == MenuManager.GetSettingPath()) {
                    File.Copy(filename, filename + "~1", true);
                }
            }
        }

        /// <summary>
        /// Decrypt a given text file
        /// </summary>
        public static void Decrypt(string filename) {
            if (File.Exists(filename)) {
                StreamReader streamReader = new StreamReader(filename);
                char[] content = streamReader.ReadToEnd().ToCharArray();
                streamReader.Close();
                int b = 0;
                for (int a = 0; a < content.Length; a++) {
                    content[a] = (char)(content[a] - key[b]);
                    b++;
                    if (b == key.Length) {
                        b = 0;
                    }
                }
                //File.SetAttributes(filename, FileAttributes.Normal);
                StreamWriter streamWriter = new StreamWriter(filename);
                //File.SetAttributes(filename, FileAttributes.Hidden);
                streamWriter.Write(content, 0, content.Length);
                streamWriter.Close();
            }
        }
    }
}
