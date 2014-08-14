using System;
using System.Collections.Generic;
using Blockchain;

namespace Database {

    public static class BlockchainHelper {

        public static void pushToMySQL( Block block, DBConnect mysql ) {
            var mysqlInputList = new List<Simplifiednput>();
            var mysqlOutputList = new List<SimplifiedOutput>();

            foreach ( var transaction in block.Transactions ) {
                foreach ( var input in transaction.inputs ) {
                    var mysqlInput = new Simplifiednput {
                                                            TransactionHash = truncateTransactionHashSixteen( BitConverter.ToString( transaction.thisTransactionHash ).Replace( "-", String.Empty ) ),
                                                            PreviousTransactionHash = truncateTransactionHashSixteen( BitConverter.ToString( input.PreviousTransactionHash ).Replace( "-", String.Empty ) ),
                                                            PreviousTransactionOutputIndex = input.PreviousTransactionIndex
                                                        };
                    mysqlInputList.Add( mysqlInput );
                }
                UInt32 outputIndexCounter = 0;
                foreach ( var output in transaction.outputs ) {
                    var mysqlOutput = new SimplifiedOutput {
                                                               Value = output.value,
                                                               PublicAddress = output.publicKeyAddress,
                                                               Index = outputIndexCounter,
                                                               TransactionHash = truncateTransactionHashSixteen( BitConverter.ToString( transaction.thisTransactionHash ).Replace( "-", String.Empty ) ),
                                                               Timestamp = block.TimeStamp
                                                           };
                    mysqlOutputList.Add( mysqlOutput );
                    outputIndexCounter++;
                }
            }

            mysql.InsertInputs( mysqlInputList );
            mysql.InsertOutputs( mysqlOutputList );
        }

        private static String truncateTransactionHashSixteen( String hash ) {
            return hash.Substring( 0, 16 );
        }
    }
}