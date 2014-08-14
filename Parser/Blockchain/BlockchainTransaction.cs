using System.Security.Cryptography;
using System.Collections.Generic;

namespace Blockchain
{
    using System;

    public sealed class BlockchainTransaction
    {
        public BlockchainTransaction()
        {
            inputs = new List<Input>();
            outputs = new List<Output>();
            thisTransactionHash = new Byte[32];
        }
        public UInt32 TransactionVersionNumber;
        public UInt64 VL_inputCount;
        public UInt64 VL_outputCount;
        public UInt32 TransactionLockTime;
        public List<Input> inputs;
        public List<Output> outputs;

        private Byte[] _thisTransactionHash;

        public Byte[] thisTransactionHash
        {
            set { this._thisTransactionHash = Sha256(Sha256(value)); }
            get { return this._thisTransactionHash; }
        }
        private static Byte[] Sha256(Byte[] array)
        {
            var hashstring = new SHA256Managed();
            return hashstring.ComputeHash(array);
        }
    }
}
