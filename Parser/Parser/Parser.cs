using System;
using System.IO;
using Blockchain;
using Database;

namespace Parser
{
    static class Parser
    {
        static bool shouldExit;
        static public void Parse(String path)
        {
            var fileIndex = 0;
            var mysql = new DBConnect();
            mysql.OpenConnection();
            while (!shouldExit)
            {
                
                var fileName = path+getFileName(fileIndex);
                if (File.Exists(fileName))
                {
                    var currentFile = File.ReadAllBytes(fileName);
                    //List<UInt32> blockIndexes = buildIndex(ref currentFile);
                    parseFileDataIntoClasses(ref currentFile, mysql);
                    fileIndex++;
                    Console.WriteLine("Finished proccessing file " + fileName);
                }
                else
                {
                    shouldExit = true;
                    mysql.UnlockTables();
                    mysql.CloseConnection();
                    Console.WriteLine("Done.");
                    Console.WriteLine("Outputs{0}", ScryptParser.outputs );
                    Console.WriteLine("Invalid : {0}", ScryptParser.invalidOutputAddresses );
                    Console.WriteLine("Unparsible : {0}", ScryptParser.unparsibleOuptuAddresses );
                    Console.ReadLine();
                }
            }
        }
        private static String getFileName(int fileIndex) {
            if (fileIndex <= 99999)
            {
                var fileName = "blk";
                var fileNumber = fileIndex.ToString();
                for (var i = fileNumber.Length; i < 5; i++)
                {
                    fileName += "0";
                }
                fileName += fileNumber;
                fileName += ".dat";
                return fileName;
            }
            throw new NotImplementedException();
        }

        private static bool seekToNextHeader(ref UInt32 cursor, ref Byte[] currentFile)
        {
            while (cursor + 3 < currentFile.Length)
            {
                if (currentFile[cursor] == 249)
                {
                    if (currentFile[cursor + 1] == 190)
                    {
                        if (currentFile[cursor + 2] == 180)
                        {
                            if (currentFile[cursor + 3] == 217)
                            {
                                return true;
                            }
                        }
                    }
                }
                cursor++;
            }
            return false;
        }

        private static Byte[] getBlockData(ref UInt32 cursor, ref Byte[] currentFile)
        {
            var header = ParseFourBytesToElement(ref cursor, ref currentFile);
            var blockDataLength = ParseFourBytesToElement(ref cursor, ref currentFile);
            if (cursor + blockDataLength < currentFile.Length)
            {
                var block = new Byte[blockDataLength];
                Array.Copy(currentFile, cursor, block, 0, blockDataLength);
                return block;
            }
            return null;
        }
        private static void parseFileDataIntoClasses(ref Byte[] currentFile,DBConnect mysql)
        {
            UInt32 fileCursor = 0;
            while(seekToNextHeader(ref fileCursor, ref currentFile))
            {
                var blockData = getBlockData(ref fileCursor, ref currentFile);
                if(blockData == null)
                {
                    // null means end of file
                    return;
                }
                
                var completedBlock = parseBlockDataIntoClass(blockData);
                BlockchainHelper.pushToMySQL(completedBlock, mysql);
            }
        }

        private static Block parseBlockDataIntoClass(Byte[] blockByteArray)
        {
            UInt32 cursor = 0;
            var block = new Block {
                                      VersionNumber = ParseFourBytesToElement( ref cursor, ref blockByteArray ),
                                      PreviousBlockHash = parseThirtyTwoBytesToElement( ref cursor, ref blockByteArray ),
                                      MerkleRootHash = parseThirtyTwoBytesToElement( ref cursor, ref blockByteArray ),
                                      TimeStamp = ParseFourBytesToElement( ref cursor, ref blockByteArray ),
                                      TargetDifficulty = ParseFourBytesToElement( ref cursor, ref blockByteArray ),
                                      Nonce = ParseFourBytesToElement( ref cursor, ref blockByteArray ),
                                      VlTransactionCount = parseVaribleLengthInteger( ref cursor, ref blockByteArray )
                                  };

            //Block class is done, moving to transaction

            for (UInt64 i = 0; i < block.VlTransactionCount; i++)
            {
                var transaction = new BlockchainTransaction();
                var transactionCursor = cursor;
                transaction.TransactionVersionNumber = ParseFourBytesToElement(ref cursor, ref blockByteArray);
                transaction.VL_inputCount = parseVaribleLengthInteger(ref cursor, ref blockByteArray);

                //fill transaction inputs
                for (UInt64 j = 0; j < transaction.VL_inputCount; j++)
                {
                    var input = new Input {
                                              PreviousTransactionHash = parseThirtyTwoBytesToElement( ref cursor, ref blockByteArray ),
                                              PreviousTransactionIndex = ParseFourBytesToElement( ref cursor, ref blockByteArray ),
                                              VL_scriptLength = parseVaribleLengthInteger( ref cursor, ref blockByteArray )
                                          };
                    input.VL_inputScript = ParseCustomLengthToElement(ref cursor, ref blockByteArray, input.VL_scriptLength);
                    input.SequenceNumber = ParseFourBytesToElement(ref cursor, ref blockByteArray);

                    transaction.inputs.Add(input);
                }

                transaction.VL_outputCount = parseVaribleLengthInteger(ref cursor, ref blockByteArray);

                //fill transaction outputs
                for (UInt64 k = 0; k < transaction.VL_outputCount; k++)
                {
                    var output = new Output {
                                                value = parseEightBytesToElement( ref cursor, ref blockByteArray ),
                                                VL_outputScriptLength = parseVaribleLengthInteger( ref cursor, ref blockByteArray )
                                            };

                    output.publicKeyAddress = ScryptParser.getPublicKey(ParseCustomLengthToElement(ref cursor, ref blockByteArray, output.VL_outputScriptLength));

                    transaction.outputs.Add(output);
                }

                transaction.TransactionLockTime = ParseFourBytesToElement(ref cursor, ref blockByteArray);

                var wholeTransactionData = new Byte[cursor - transactionCursor];
                Array.Copy(blockByteArray, transactionCursor, wholeTransactionData, 0, cursor - transactionCursor);
                transaction.thisTransactionHash = wholeTransactionData;
                block.Transactions.Add(transaction);
            }
            return block;
        }
        private static UInt32 ParseFourBytesToElement(ref UInt32 cursor, ref Byte[] blockByteArray)
        {
            var buffer = new Byte[4];
            Array.Copy(blockByteArray, cursor, buffer, 0, 4);
            cursor += 4;
            return BitConverter.ToUInt32(buffer, 0);
        }
        private static UInt64 parseEightBytesToElement(ref UInt32 cursor, ref Byte[] blockByteArray)
        {
            var buffer = new Byte[8];
            Array.Copy(blockByteArray, cursor, buffer, 0, 8);
            cursor += 8;
            return BitConverter.ToUInt64(buffer, 0);
        }
        private static Byte[] parseThirtyTwoBytesToElement(ref UInt32 cursor, ref Byte[] blockByteArray)
        {
            var buffer = new Byte[32];
            Array.Copy(blockByteArray, cursor, buffer, 0, 32);
            cursor += 32;
            return buffer;
        }
        private static UInt64 parseVaribleLengthInteger(ref UInt32 cursor, ref Byte[] blockByteArray)
        {
            var buffer = new Byte[2];
            buffer[0] = blockByteArray[cursor];
            UInt32 transactionCount = BitConverter.ToUInt16(buffer, 0);
            if (transactionCount < 253)
            {
                cursor += 1;
                return transactionCount;
                 
            }
            if (transactionCount == 253)
            {
                buffer = new Byte[2];
                Array.Copy(blockByteArray, cursor + 1, buffer, 0, 2);
                cursor += 3;
                return BitConverter.ToUInt16(buffer, 0);
                
            }
            if (transactionCount == 254)
            {
                buffer = new Byte[4];
                Array.Copy(blockByteArray, cursor + 1, buffer, 0, 4);
                cursor += 5;
                return BitConverter.ToUInt32(buffer, 0);
            }
            if (transactionCount == 255)
            {
                buffer = new Byte[8];
                Array.Copy(blockByteArray, cursor + 1, buffer, 0, 8);
                cursor += 9;
                return BitConverter.ToUInt64(buffer, 0);
            }
            throw new NotImplementedException();
        }
        private static Byte[] ParseCustomLengthToElement(ref UInt32 cursor, ref Byte[] blockByteArray, UInt64 length)
        {
            var buffer = new Byte[length];
            Array.Copy(blockByteArray, cursor, buffer, 0, (UInt32)length);
            //Array.Reverse(buffer);
            cursor += (UInt32)length;
            return buffer;
        }
    }
}
