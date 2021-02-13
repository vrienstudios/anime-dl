using ADLCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Integrations
{
    /// <summary>
    /// MAL Integration will save username/password under the file ./userdata.adl
    /// The username and password will both be encrypted with a 16-byte (128-bit) key, and a unique IV appended to the last block.
    /// The encryption is there so that your username and password are relatively safe in the case of exposure to your ADL installation.
    /// If a key is not provided when setting up MAL integration, a randomly generated key will be used, and the key will be stored as a BASE64 string within this application at ADLCore/Static/keyFile.bin
    /// </summary>
    class MyAnimeList : Interfaces.Integration
    {
        public MyAnimeList() : base(Site.MyAnimeList)
        {

        }

        public override listType FindObjectsFromList()
        {
            throw new NotImplementedException();
        }

        public override void LoadUserData()
        {
            throw new NotImplementedException();
        }
    }
}
