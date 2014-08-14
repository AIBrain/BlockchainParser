using System;
using Blockchain;

namespace Parser
{
    public static class ScryptParser
    {
        public static int unparsibleOuptuAddresses = 0;
        public static int invalidOutputAddresses = 0;
        public static UInt64 outputs = 0;
        public static String getPublicKey(Byte[] scrypt)
        {
            outputs++;
            var length = scrypt.Length;
            if (length == 67)
            {
                return sixtySevenByte(scrypt);
            }
            if (length == 66)
            {
                return sixtySixByte(scrypt);
            }
            if (length == 25)
            {
                return twentyFiveByte(scrypt);
            }
            if (length < 20)
            {
                invalidOutputAddresses++;
                return lessThanTwenty();
            }
            unparsibleOuptuAddresses++;
            unparsibleOuptuAddresses++;
            return "";
        }
        private static String sixtySevenByte(Byte[] scrypt)
        {
            var key = new Byte[65];
            Array.Copy(scrypt, 1, key, 0, 65);
            return AddressHelper.EllipticCurveToBTCAddress(key);
        }
        private static String sixtySixByte(Byte[] scrypt)
        {
            var key = new Byte[65];
            Array.Copy(scrypt, key, 65);
            return AddressHelper.EllipticCurveToBTCAddress(key);
        }
        private static String twentyFiveByte(Byte[] scrypt)
        {
            var key = new Byte[20];
            Array.Copy(scrypt, 3, key, 0, 20);
            return AddressHelper.ripemdToBTCAddress(key);
        }
        private static String lessThanTwenty()
        {
            return "Unspendable";
        }

    }
}
