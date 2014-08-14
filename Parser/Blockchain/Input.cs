namespace Blockchain
{
    using System;

    public class Input
    {
        public Byte[] PreviousTransactionHash;
        public UInt32 PreviousTransactionIndex;
        public UInt64 VL_scriptLength;
        public Byte[] VL_inputScript;
        public UInt32 SequenceNumber;
    }
}
