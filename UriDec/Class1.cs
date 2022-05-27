using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HtmlAgilityPack;

namespace UriDec
{
    public class SourceObj
    {
        public bool bk;
        public string uri;
        public int res;
    }
    public static class GoGoStream
    {
        public static void DecryptUri(HtmlDocument doc, string baseUri, out List<SourceObj> decrypted, out string refer)
        {
            string s_crypto = null;
            string m_crypto = null;
            string ts = null;
            
            HtmlNode[] nodes = doc.DocumentNode.ChildNodes[1].NextSibling.ChildNodes[1].ChildNodes.Where(x => x.Name == "meta" && x.GetAttributeValue("name", null) == "crypto" || x.Name == "crypto" || x.Attributes.Contains("data-value")).ToArray();
            
            foreach (HtmlNode node in nodes.Where(x => x.Name == "script"))
            {
                switch (node.GetAttributeValue("data-name", null))
                {
                    case "episode":
                    {
                        s_crypto = node.GetAttributeValue("data-value", null);
                        break;
                    }
                    case "ts":
                    {
                        ts = node.GetAttributeValue("data-value", null);
                        break;
                    }
                    default:
                        continue;
                }
            }

            var bsa = doc.DocumentNode.ChildNodes[2].ChildNodes.Where(x => x.Name == "body").First();
            var atr_key = bsa.Attributes[0].Value.Split('-')[1];
            var bsaa = bsa.ChildNodes.Where(x => x.Name == "div").First();
            var kiv = bsaa.Attributes[0].Value.Split('-')[1];

            //var k_atr_key2 = bsaa.ChildNodes[1].Attributes[0].Value.Split('-')[1];
            //m_crypto = nodes.First(x => x.Name == "meta").GetAttributeValue("content", null);
            
            Random rnd = new Random();
            //Method finalized by Insight
            var aa = DecryptAES128(Convert.FromBase64String(s_crypto),
                Encoding.UTF8.GetBytes(atr_key), Encoding.UTF8.GetBytes(kiv));
            var bbb = Encoding.UTF8.GetString(aa);
            var ad = bbb.Substring(0x0, bbb.IndexOf('&'));
            /*var aa = DecryptAES128(Convert.FromBase64String(s_crypto), Encoding.UTF8.GetBytes(ts + ts),
                Encoding.UTF8.GetBytes(ts), null, 256, 128, PaddingMode.PKCS7);
            
            var ab = DecryptAES128(Convert.FromBase64String(m_crypto), aa, Encoding.UTF8.GetBytes(ts));
            var ac = Encoding.UTF8.GetString(ab);
            //var ak = aj['substr'](0x0, aj['indexOf']('&'))
            var ad = ac.Substring(0x0, ac.IndexOf('&'));
            */

            var _0xe663c3 = "https://goload.pro/encrypt-ajax.php?id=" +
                            $"{Convert.ToBase64String(EncryptAES128(Encoding.UTF8.GetBytes(ad), Encoding.UTF8.GetBytes(atr_key), Encoding.UTF8.GetBytes(kiv)))}" +
                            $"{bbb.Substring(bbb.IndexOf('&'))}" + 
                            $"&alias={ad}";
            
            
            int getRand(int girth, int height) => (int)Math.Floor((decimal)rnd.NextDouble() * (height - girth + 0x1)) + girth;

            string rand(int j)
            {
                var bl = "";
                while (j > 0x0)
                {
                    j--;
                    bl += getRand(0x0, 0x9).ToString();
                }

                return bl.ToString();
            }
            var al = rand(0x10);
            //Encoding.UTF8.GetBytes("1285672383939852")
            var fin = EncryptAES128(Encoding.UTF8.GetBytes(ad), 
                Encoding.UTF8.GetBytes(atr_key), Encoding.UTF8.GetBytes(kiv));
            //var da = "https://" + baseUri + $"/encrypt-ajax.php?id={Convert.ToBase64String(fin)}{bbb.Substring(bbb.IndexOf('&'))}&time={rand(0x2) + al + rand(0x2)}";
            
            //https://gogoplay5.com/encrypt-ajax.php?id=6VMJTbRSsOBzH+nlDStksg==&title=Akebi-chan+no+Sailor-fuku&typesub=SUB&sub=&cover=Y292ZXIvYWtlYmktY2hhbi1uby1zYWlsb3ItZnVrdS5wbmc=&mip=0.0.0.0&refer=none&ch=d41d8cd98f00b204e9800998ecf8427e&op=1&alias=MTc3ODQ1
            vsApiObj apiObj;
            JsonDocument jDoc;
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("Referer", $"https://{baseUri}/streaming.php");
                wc.Headers.Add("x-requested-with", "XMLHttpRequest");
                wc.Headers.Add("Accept", "*/*");
                wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.0) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.");
                wc.Headers.Add("Accept-Ecoding", "identity");

                var e = wc.DownloadString(_0xe663c3);
                jDoc = JsonDocument.Parse(e);

                var parsee = jDoc.RootElement.GetProperty("data").GetString();
                var newE = Encoding.UTF8.GetString(DecryptAES128(Convert.FromBase64String(parsee), 
                    Encoding.UTF8.GetBytes("54674138327930866480207815084989"), Encoding.UTF8.GetBytes(kiv)));
                // Key -> <div class="videocontent videocontent-{key}> (Universal, if it changes often, I'll add it into the scraper at the top)
                jDoc = JsonDocument.Parse(newE);
            }

            int b = jDoc.RootElement.GetProperty("source").GetArrayLength();
            List<SourceObj> sources = new List<SourceObj>();
            
            for (int idx = 0; idx < b; idx++)
            {
                string bb = jDoc.RootElement.GetProperty("source")[idx].GetProperty("label").GetString();
                int beres = 0;
                int.TryParse(bb.Length > 4 ? bb.Substring(0, bb.IndexOf('P') - 1) : "-1", out beres);
                sources.Add(new SourceObj()
                {
                    uri = jDoc.RootElement.GetProperty("source")[idx].GetProperty("file").GetString().Replace("\\", string.Empty),
                    res = beres,
                    bk = false,
                });
            }
            
            WebHeaderCollection whc = new WebHeaderCollection();

            refer = $"https://gogoplay.io/streaming.php?id={bbb.Split("&refer")[0]}";
            decrypted = sources;
        }

        public static WebHeaderCollection GetEncHeaders()
        {
            var collection = new WebHeaderCollection();
            collection.Add("Accept", "*/*");
            collection.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.0) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.");
            collection.Add("Accept-Encoding", "identity");
            collection.Add("sec-fetch-site", "same-origin");
            collection.Add("sec-fetch-mode", "no-cors");
            collection.Add("sec-fetch-dest", "video");
            return collection;
        }

        public static void tReq()
        {
            //--header="Referer: https://gogoplay.io/streaming.php?id=MTc3ODMy&title=Douluo+Dalu+2nd+Season&typesub=SUB&sub=&cover=Y292ZXIvZG91bHVvLWRhbHUtMm5kLXNlYXNvbi5wbmc="
            //--header="User-Agent: Mozilla/5.0 (Windows NT 6.0) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0."
            //--header="sec-fetch-site: same-origin"
            //--header="Host: vidstreamingcdn.com"
            //--header="sec-fetch-mode: no-cors"
            //"https://vidstreamingcdn.com/cdn25/cef41383ed281bc8af5846c088527561/EP.1.v1.1639333994.720p.mp4?expiry=1641699119255"
            WebClient wcc = new WebClient();
            List<SourceObj> lst;
            string refe;
            var docu = new HtmlDocument();
            docu.LoadHtml(wcc.DownloadString("https://gogoplay.io/streaming.php?id=MTExNTM4&title=Douluo+Dalu+2nd+Season&typesub=SUB&sub=&cover=Y292ZXIvZG91bHVvLWRhbHUtMm5kLXNlYXNvbi5wbmc="));
            DecryptUri(docu, "gogoplay1.com", out lst, out refe);
            var collection = new WebHeaderCollection();
            collection.Add("Host", "vidstreamingcdn.com");
            collection.Add("Referer", refe);
            //collection.Add("Referer", "https://gogoplay.io/streaming.php?id=MTExNTM4&title=Douluo+Dalu+2nd+Season&typesub=SUB&sub=&cover=Y292ZXIvZG91bHVvLWRhbHUtMm5kLXNlYXNvbi5wbmc=");
            collection.Add("Accept", "*/*");
            collection.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.0) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.");
            collection.Add("Accept-Encoding", "identity");
            collection.Add("sec-fetch-site", "same-origin");
            collection.Add("sec-fetch-mode", "no-cors");
            collection.Add("sec-fetch-dest", "video");
            //collection.Add("TE", "trailers");
            collection.Add("range", "bytes=0-10");
            WebClient wc = new WebClient();
            wc.Headers = collection;
            var b = wc.DownloadString("https://vidstreamingcdn.com/cdn17/510c529e80f4cc7f634b06e508db6bc5/EP.1.v1.1639498373.720p.mp4?mac=cV04WsDAZusbuizzamke5eAERlSWq4vNvs/KCLxdNqU=&expiry=1641702423340");

        }
        
        public static Byte[] EncryptAES128(Byte[] data, Byte[] Key, Byte[] IV = null, int kSize = 256,
            int blockSize = 128, PaddingMode pm = PaddingMode.PKCS7)
        {
            RijndaelManaged algorithm = GetRijndael(Key, IV, kSize, blockSize, pm);

            Byte[] bytes;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateEncryptor(Key, IV),
                    CryptoStreamMode.Write))
                    cs.Write(data, 0, data.Length);
                bytes = ms.ToArray();
            }

            GC.Collect();

            return bytes;
        }
        
        public static Byte[] DecryptAES128(Byte[] data, Byte[] Key, Byte[] IV, Byte[] saltBuffer = null,
            int kSize = 256, int blockSize = 128, PaddingMode pm = PaddingMode.PKCS7)
        {
            if (IV == null)
                DeriveKeyAndIV(Key, data, saltBuffer, out Key, out IV, out data);

            RijndaelManaged algorithm = GetRijndael(Key, IV, kSize, blockSize, pm);
            algorithm.Mode = CipherMode.CBC;
            algorithm.FeedbackSize = 128;
            Byte[] bytes;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Write))
                    cs.Write(data, 0, data.Length);
                bytes = ms.ToArray();
            }

            GC.Collect();

            return bytes;
        }
        
        public static void DeriveKeyAndIV(byte[] p, byte[] source, byte[] salt, out byte[] key, out byte[] iv,
            out byte[] encryptedBytes)
        {
            salt = new byte[8];
            encryptedBytes = new byte[source.Length - salt.Length - 8];

            Buffer.BlockCopy(source, 8, salt, 0, salt.Length);
            Buffer.BlockCopy(source, salt.Length + 8, encryptedBytes, 0, encryptedBytes.Length);

            //http://www.openssl.org/docs/crypto/EVP_BytesToKey.html#KEY_DERIVATION_ALGORITHM @#%@#$^#$&@^#$%!!#$^!
            //https://stackoverflow.com/questions/8008253/c-sharp-version-of-openssl-evp-bytestokey-method
            List<byte> concatenatedHashes = new List<byte>(48);
            byte[] currentHash = new byte[0];
            MD5 md5 = MD5.Create();
            bool enoughBytesForKey = false;

            while (!enoughBytesForKey)
            {
                int preHashLength = currentHash.Length + p.Length + salt.Length;

                byte[] preHash = new byte[preHashLength];


                Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
                Buffer.BlockCopy(p, 0, preHash, currentHash.Length, p.Length);
                Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + p.Length, salt.Length);

                currentHash = md5.ComputeHash(preHash);
                concatenatedHashes.AddRange(currentHash);

                if (concatenatedHashes.Count >= 48)
                    enoughBytesForKey = true;
            }

            key = new byte[32];
            iv = new byte[16];
            concatenatedHashes.CopyTo(0, key, 0, 32);
            concatenatedHashes.CopyTo(32, iv, 0, 16);
            md5.Clear();
            md5 = null;
        }
        
        private static RijndaelManaged GetRijndael(Byte[] Key, Byte[] IV, int kSize = 128, int blockSize = 128,
            PaddingMode pm = PaddingMode.None)
        {
            RijndaelManaged Alg = new RijndaelManaged()
            {
                Padding = pm,
                Mode = CipherMode.CBC,
                KeySize = kSize,
                BlockSize = blockSize,
                Key = Key,
            };
            if (IV != null)
                Alg.IV = IV;
            return Alg;
        }
    }
    
    public class Source
    {
        public string file { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string @default { get; set; }
    }

    public class SourceBk
    {
        public string file { get; set; }
        public string label { get; set; }
        public string type { get; set; }
    }

    public class vsApiObj
    {
        public List<Source> source { get; set; }
        public List<SourceBk> source_bk { get; set; }
        public List<object> track { get; set; }
        public List<object> advertising { get; set; }
        public string linkiframe { get; set; }
    }

}
