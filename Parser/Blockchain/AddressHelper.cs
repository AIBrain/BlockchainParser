using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Numerics;

namespace Blockchain
{
    
    public class AddressHelper
    {
        const String ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        public static String EllipticCurveToBTCAddress(Byte[] key)
        {
            var PreHashQ = AppendBitcoinNetwork(RipeMD160(Sha256(key)), 0);
            return Base58Encode(ConcatAddress(PreHashQ, Sha256(Sha256(PreHashQ))));
        }

        public static String ripemdToBTCAddress(Byte[] key)
        {
            var PreHashQ = AppendBitcoinNetwork(key, 0);
            return Base58Encode(ConcatAddress(PreHashQ, Sha256(Sha256(PreHashQ))));
        }
        private static Byte[] HexToByte(String HexString)
        {
            if (HexString.Length % 2 != 0)
                throw new Exception("Invalid HEX");
            var retArray = new Byte[HexString.Length / 2];
            for (var i = 0; i < retArray.Length; ++i)
            {
                retArray[i] = Byte.Parse(HexString.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return retArray;
        }
        private static Byte[] Sha256(Byte[] array)
        {
            var hashstring = new SHA256Managed();
            return hashstring.ComputeHash(array);
        }
        private static Byte[] RipeMD160(Byte[] array)
        {
            var hashstring = new RIPEMD160Managed();
            return hashstring.ComputeHash(array);
        }
        private static Byte[] AppendBitcoinNetwork(Byte[] ripeHash, Byte Network)
        {
            var extended = new Byte[ripeHash.Length + 1];
            extended[0] = Network;
            Array.Copy(ripeHash, 0, extended, 1, ripeHash.Length);
            return extended;
        }

        private static Byte[] ConcatAddress(Byte[] RipeHash, Byte[] Checksum)
        {
            var ret = new Byte[RipeHash.Length + 4];
            Array.Copy(RipeHash, ret, RipeHash.Length);
            Array.Copy(Checksum, 0, ret, RipeHash.Length, 4);
            return ret;
        }

        private static String Base58Encode(Byte[] array)
        {
            var retString = String.Empty;
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
