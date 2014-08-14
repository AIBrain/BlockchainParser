using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace Database {

    public sealed class DBConnect {
        private MySqlConnection _connection;
        private String _server;
        private String _database;
        private String _uid;
        private String _password;
        private String _timeout;

        public DBConnect() {
            Initialize();
        }

        private void Initialize() {
            this._server = "localhost";
            this._database = "mydb";
            this._uid = "root";
            this._password = "tiny";
            this._timeout = "100";
            var connectionString = String.Format( "SERVER={0};DATABASE={1};UID={2};PASSWORD={3};Connect Timeout={4}", this._server, this._database, this._uid, this._password, this._timeout );

            this._connection = new MySqlConnection( connectionString );
        }

        public void SetMaxAddressQuerryTime( int time ) {
            var querry = String.Format( "SET SESSION MAX_STATEMENT_TIME={0};", time );
            var cmd = new MySqlCommand( querry, this._connection );
            cmd.ExecuteNonQuery();
        }

        public void UnlockTables() {
            const String querry = "UNLOCK TABLES;";
            var cmd = new MySqlCommand( querry, this._connection );
            cmd.ExecuteNonQuery();
        }

        public bool OpenConnection() {
            try {
                this._connection.Open();
                return true;
            }
            catch ( MySqlException ex ) {
                //When handling errors, you can your application's response based
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch ( ex.Number ) {
                    case 0:
                        Console.WriteLine( "Cannot connect to server.  Contact administrator" );
                        break;

                    case 1045:
                        Console.WriteLine( "Invalid username/password, please try again" );
                        break;
                }
                return false;
            }
        }

        public bool CloseConnection() {
            try {
                this._connection.Close();
                return true;
            }
            catch ( MySqlException ex ) {
                Console.WriteLine( ex.Message );
                return false;
            }
        }

        public void InsertOutputs( List<SimplifiedOutput> outputs ) {
            var query = new StringBuilder();
            query.Append( "LOCK TABLES output WRITE; INSERT INTO output (value, publicAddress, outputIndex, transactionHash, timestamp) VALUES" );
            var isFirstElement = true;
            foreach ( var output in outputs ) {
                if ( !isFirstElement ) {
                    query.Append( "," );
                }
                query.Append( "(" );
                query.Append( output.Value.ToString() );
                query.Append( ",'" );
                query.Append( output.PublicAddress.ToString() );
                query.Append( "'," );
                query.Append( output.Index.ToString() );
                query.Append( ",'" );
                query.Append( output.TransactionHash );
                query.Append( "'," );
                query.Append( output.Timestamp.ToString() );
                query.Append( ")" );
                isFirstElement = false;
            }
            query.Append( ";" );

            //create command and assign the query and connection from the constructor
            var cmd = new MySqlCommand( query.ToString(), this._connection );

            //Execute command
            cmd.ExecuteNonQuery();

            //close connection
            //this.CloseConnection();
        }

        public void InsertInputs( List<Simplifiednput> inputs ) {
            var query = new StringBuilder();
            query.Append( "LOCK TABLES input WRITE; INSERT INTO input (transactionHash, previousTransactionHash, previousTransactionOutputIndex) VALUES" );
            var isFirstElement = true;
            foreach ( var input in inputs ) {
                if ( !isFirstElement ) {
                    query.Append( "," );
                }
                query.Append( "('" );
                query.Append( input.TransactionHash );
                query.Append( "','" );
                query.Append( input.PreviousTransactionHash );
                query.Append( "'," );
                query.Append( input.PreviousTransactionOutputIndex.ToString() );
                query.Append( ")" );
                isFirstElement = false;
            }
            query.Append( ";" );
            //create command and assign the query and connection from the constructor
            var cmd = new MySqlCommand( query.ToString(), this._connection );

            //Execute command
            cmd.ExecuteNonQuery();

            //close connection
            //this.CloseConnection();
        }

        public List<Transaction> getRecivedFrom( String address ) {
            var jsonList = new List<Transaction>();
            var query = new StringBuilder();
            query.Append( "Select output.publicAddress as source, SUM(totals.value) as value, COUNT(output.publicAddress) AS weight " );
            query.Append( "from output,( SELECT previousTransactionHash, value, previousTransactionOutputIndex from input " );
            query.Append( "left outer join output on input.transactionHash = output.transactionHash " );
            query.Append( "where publicAddress = '" );
            query.Append( address );
            query.Append( "') AS totals " );
            query.Append( "where totals.previousTransactionHash = output.transactionHash and totals.previousTransactionOutputIndex = output.outputIndex group by output.publicAddress;" );

            //Create Command
            var cmd = new MySqlCommand( query.ToString(), this._connection ) {
                                                                                 CommandTimeout = 120
                                                                             };
            //Create a data reader and Execute the command
            try {
                var dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while ( dataReader.Read() ) {
                    var transaction = new Transaction {
                                                          Source = ( String ) dataReader[ "source" ],
                                                          Target = address,
                                                          Value = Convert.ToUInt64( dataReader[ "value" ] ),
                                                          Weight = Convert.ToUInt16( dataReader[ "weight" ] )
                                                      };
                    jsonList.Add( transaction );
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                //this.CloseConnection();

                //return list to be displayed
                return jsonList;
            }
            catch ( MySqlException e ) {
                Console.WriteLine( "Skipping" );
                return null;
            }
        }

        public List<Transaction> getSentTo( String address ) {
            var jsonList = new List<Transaction>();
            var query = new StringBuilder();

            query.Append( "select output.publicAddress as target, SUM(totals.value) as value, COUNT(output.publicAddress) AS weight from output," );
            query.Append( "(SELECT input.transactionHash,output.value " );
            query.Append( "from input " );
            query.Append( "left outer join output on input.previousTransactionHash = output.transactionHash and input.previousTransactionOutputIndex = output.outputIndex " );
            query.Append( "where publicAddress = '" );
            query.Append( address );
            query.Append( "') AS totals " );
            query.Append( "where totals.transactionHash = output.transactionHash group by output.publicAddress;" );

            //Create Command
            var cmd = new MySqlCommand( query.ToString(), this._connection );
            cmd.CommandTimeout = 120;
            //Create a data reader and Execute the command
            try {
                var dataReader = cmd.ExecuteReader();
                //Read the data and store them in the list
                while ( dataReader.Read() ) {
                    var transaction = new Transaction();
                    transaction.Source = address;
                    transaction.Target = ( String )dataReader[ "target" ];
                    transaction.Value = Convert.ToUInt64( dataReader[ "value" ] );
                    transaction.Weight = Convert.ToUInt16( dataReader[ "weight" ] );
                    jsonList.Add( transaction );
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                //this.CloseConnection();

                //return list to be displayed
                return jsonList;
            }
            catch ( MySqlException e ) {
                Console.WriteLine( "Skipping" );
                return null;
            }
        }
    }
}