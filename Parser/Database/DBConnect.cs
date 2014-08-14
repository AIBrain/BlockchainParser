using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace Database {

    public sealed class DBConnect {
        private MySqlConnection _connection;
        private string _server;
        private string _database;
        private string _uid;
        private string _password;
        private string _timeout;

        public DBConnect() {
            Initialize();
        }

        private void Initialize() {
            this._server = "localhost";
            this._database = "mydb";
            this._uid = "root";
            this._password = "tiny";
            this._timeout = "100";
            var connectionString = string.Format( "SERVER={0};DATABASE={1};UID={2};PASSWORD={3};Connect Timeout={4}", this._server, this._database, this._uid, this._password, this._timeout );

            this._connection = new MySqlConnection( connectionString );
        }

        public void SetMaxAddressQuerryTime( int time ) {
            var querry = string.Format( "SET SESSION MAX_STATEMENT_TIME={0};", time );
            var cmd = new MySqlCommand( querry, this._connection );
            cmd.ExecuteNonQuery();
        }

        public void UnlockTables() {
            const string querry = "UNLOCK TABLES;";
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
                query.Append( output.value.ToString() );
                query.Append( ",'" );
                query.Append( output.publicAddress.ToString() );
                query.Append( "'," );
                query.Append( output.index.ToString() );
                query.Append( ",'" );
                query.Append( output.transactionHash );
                query.Append( "'," );
                query.Append( output.timestamp.ToString() );
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
                query.Append( input.transactionHash );
                query.Append( "','" );
                query.Append( input.previousTransactionHash );
                query.Append( "'," );
                query.Append( input.previousTransactionOutputIndex.ToString() );
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

        public List<Transaction> getRecivedFrom( string address ) {
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
            var cmd = new MySqlCommand( query.ToString(), this._connection );
            cmd.CommandTimeout = 120;
            //Create a data reader and Execute the command
            try {
                var dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while ( dataReader.Read() ) {
                    var transaction = new Transaction();
                    transaction.source = ( string )dataReader[ "source" ];
                    transaction.target = address;
                    transaction.value = Convert.ToUInt64( dataReader[ "value" ] );
                    transaction.weight = Convert.ToUInt16( dataReader[ "weight" ] );
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

        public List<Transaction> getSentTo( string address ) {
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
                    transaction.source = address;
                    transaction.target = ( string )dataReader[ "target" ];
                    transaction.value = Convert.ToUInt64( dataReader[ "value" ] );
                    transaction.weight = Convert.ToUInt16( dataReader[ "weight" ] );
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