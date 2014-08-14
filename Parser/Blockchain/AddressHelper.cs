using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Security.Cryptography;
using System.Numerics;

namespace Blockchain
{
    
    public class AddressHelper
    {
        const string ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        public static string EllipticCurveToBTCAddress(byte[] key)
        {
            var PreHashQ = AppendBitcoinNetwork(RipeMD160(Sha256(key)), 0);
            return Base58Encode(ConcatAddress(PreHashQ, Sha256(Sha256(PreHashQ))));
        }

        public static string ripemdToBTCAddress(byte[] key)
        {
            var PreHashQ = AppendBitcoinNetwork(key, 0);
            return Base58Encode(ConcatAddress(PreHashQ, Sha256(Sha256(PreHashQ))));
        }
        private static byte[] HexToByte(string HexString)
        {
            if (HexString.Length % 2 != 0)
                throw new Exception("Invalid HEX");
            var retArray = new byte[HexString.Length / 2];
            for (var i = 0; i < retArray.Length; ++i)
            {
                retArray[i] = byte.Parse(HexString.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return retArray;
        }
        private static byte[] Sha256(byte[] array)
        {
            var hashstring = new SHA256Managed();
            return hashstring.ComputeHash(array);
        }
        private static byte[] RipeMD160(byte[] array)
        {
            var hashstring = new RIPEMD160Managed();
            return hashstring.ComputeHash(array);
        }
        private static byte[] AppendBitcoinNetwork(byte[] RipeHash, byte Network)
        {
            var extended = new byte[RipeHash.Length + 1];
            extended[0] = (byte)Network;
            Array.Copy(RipeHash, 0, extended, 1, RipeHash.Length);
            return extended;
        }

        private static byte[] ConcatAddress(byte[] RipeHash, byte[] Checksum)
        {
            var ret = new byte[RipeHash.Length + 4];
            Array.Copy(RipeHash, ret, RipeHash.Length);
            Array.Copy(Checksum, 0, ret, RipeHash.Length, 4);
            return ret;
        }

        private static string Base58Encode(byte[] array)
        {
            var retString = string.Empty;
            BigInteger encodeSize = ALPHABET.Length;
            BigInteger arrayToInt = 0;
            for (var i = 0; i < array.Length; ++i)
            {
                arrayToInt = arrayToInt * 256 + array[i];
            }
            while (arrayToInt > 0)
            {
                var rem = (int)(arrayToInt % encodeSize);
                arrayToInt /= encodeSize;
                retString = ALPHABET[rem] + retString;
            }
            for (var i = 0; i < array.Length && array[i] == 0; ++i)
                retString = ALPHABET[0] + retString;

            return retString;
        }
    }
}
