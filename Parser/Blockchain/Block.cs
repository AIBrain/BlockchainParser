using System.Collections.Generic;

namespace Blockchain
{
    using System;

    public sealed class Block
    {
        public Block()
        {
            this.Transactions = new List<BlockchainTransaction>();
        }
        public UInt32 VersionNumber;
        public Byte[] PreviousBlockHash;
        public Byte[] MerkleRootHash;
        public UInt32 TimeStamp;
        public UInt32 TargetDifficulty;
        public UInt32 Nonce;
        public UInt64 VlTransactionCount;
        public readonly List<BlockchainTransaction> Transactions;
    }
}
