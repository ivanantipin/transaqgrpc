using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Firelib
{
    public static class MarshalUtf8
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding();

        //--------------------------------------------------------------------------------
        public static IntPtr StringToHGlobalUtf8(string data)
        {
            Byte[] dataEncoded = Utf8.GetBytes(data + "\0");

            int size = Marshal.SizeOf(dataEncoded[0]) * dataEncoded.Length;

            IntPtr pData = Marshal.AllocHGlobal(size);

            Marshal.Copy(dataEncoded, 0, pData, dataEncoded.Length);

            return pData;
        }

        //--------------------------------------------------------------------------------
        public static string PtrToStringUtf8(IntPtr pData)
        {
            // this is just to get buffer length in bytes
            String errStr = Marshal.PtrToStringUTF8(pData);
            if (errStr == null)
            {
                return "";
            }

            return Uri.UnescapeDataString(errStr);

        }

        //--------------------------------------------------------------------------------
    }
}