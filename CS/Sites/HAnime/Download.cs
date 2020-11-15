using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace VidStreamIORipper.Sites.HAnime
{
    public static class Download
    {
        /// <summary>
        /// Expect download continuation on this front in the future.
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="loc"></param>
        /// <param name="videoData"></param>
        public static void DownloadHAnime(string manifest, string loc, Video videoData)
        {
            WebClient wc = new WebClient();
            string[] a = wc.DownloadString(manifest).Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            // key
            byte[] key = Encoding.ASCII.GetBytes("0123456701234567");
            int media_sequence = 0;
            string path = Directory.GetCurrentDirectory() + $"\\hanime\\{videoData.hentai_video.name}.mp4";
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\hanime\\");
            FileStream fs = File.Create(path);

            byte[] iv = media_sequence.ToBigEndianBytes();

            foreach (string str in a)
            {
                if (str[0] != '#')
                {
                    iv = media_sequence.ToBigEndianBytes();
                    iv = new byte[8].Concat(iv).ToArray(); 

                    // HLS uses AES-128 w/ CBC & PKCS7
                    RijndaelManaged algorithm = new RijndaelManaged()
                    {
                        Padding = PaddingMode.PKCS7,
                        Mode = CipherMode.CBC,
                        KeySize = 128,
                        BlockSize = 128
                    };

                    algorithm.Key = key;
                    algorithm.IV = iv;

                    wc = new WebClient();
                    byte[] data = wc.DownloadData(str);
                    MemoryStream ms = new MemoryStream();
                    CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Write);
                    cs.Write(data, 0, data.Length);
                    byte[] bytes = ms.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                    cs.Close();
                    ms.Close();
                    cs.Dispose();
                    ms.Dispose();
                    media_sequence++;
                    GC.Collect();
                }
                Console.WriteLine(str);
            }
        }
    }
}
