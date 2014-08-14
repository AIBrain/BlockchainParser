using System;
using System.Collections.Generic;
using Blockchain;

namespace Database {

    public static class BlockchainHelper {

        public static void pushToMySQL( Block block, DBConnect mysql ) {
            var mysqlInputList = new List<Simplifiednput>();
            var mysqlOutputList = new List<SimplifiedOutput>();

            foreach ( var transaction in block.transactions ) {
                foreach ( var input in transaction.inputs ) {
                    var mysqlInput = new Simplifiednput();
                    mysqlInput.transactionHash = truncateTransactionHashSixteen( BitConverter.ToString( transaction.thisTransactionHash ).Replace( "-", string.Empty ) );
                    mysqlInput.previousTransactionHash = truncateTransactionHashSixteen( BitConverter.ToString( input.previousTransactionHash ).Replace( "-", string.Empty ) );
                    mysqlInput.previousTransactionOutputIndex = input.previousTransactionIndex;
                    mysqlInputList.Add( mysqlInput );
                }
                uint outputIndexCounter = 0;
                foreach ( var output in transaction.outputs ) {
                    var mysqlOutput = new SimplifiedOutput();
                    mysqlOutput.value = output.value;
                    mysqlOutput.publicAddress = output.publicKeyAddress;
                    mysqlOutput.index = outputIndexCounter;
                    mysqlOutput.transactionHash = truncateTransactionHashSixteen( BitConverter.ToString( transaction.thisTransactionHash ).Replace( "-", string.Empty ) );
                    mysqlOutput.timestamp = block.timeStamp;
                    mysqlOutputList.Add( mysqlOutput );
                    outputIndexCounter++;
                }
            }

            mysql.InsertInputs( mysqlInputList );
            mysql.InsertOutputs( mysqlOutputList );
        }

        private static string truncateTransactionHashSixteen( string hash ) {
            return hash.Substring( 0, 16 );
        }
    }
}