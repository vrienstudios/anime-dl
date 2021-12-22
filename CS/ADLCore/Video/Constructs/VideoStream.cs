using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ADLCore.Video.Constructs
{
    public class VideoStream
    {
        MemoryStream byteStream;

        public delegate void newBytes(Byte[] bytes);

        public event newBytes onNewByte;

        Boolean streamFinished;

        public VideoStream()
        {
            streamFinished = false;
            aOne = 0;
            aTwo = 0;
            byteStream = new MemoryStream();
        }

        int aOne;
        int aTwo;

        /// <summary>
        /// Get any new bytes as an array if not using the events.
        /// </summary>
        /// <returns>System.Byte[]</returns>
        public Byte[] getNewBytes()
        {
            byte[] buffer = new byte[aTwo];
            byteStream.Position = 0;
            byteStream.Read(buffer, aOne, aTwo);
            aOne += aTwo;

            return buffer;
        }

        //Used internally
        public void addNewBytes(Byte[] bytes)
        {
            aTwo += bytes.Length;
            byteStream.Write(bytes, aOne, bytes.Length);
            onNewByte?.Invoke(bytes);
        }

        //If using events, you should call this after processing the bytes.
        public void clearMemory()
        {
            aOne = 0;
            aTwo = 0;
            byteStream.SetLength(0);
        }

        public bool isFinished() => streamFinished;
    }
}