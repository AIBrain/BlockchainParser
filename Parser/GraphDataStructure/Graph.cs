using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database;

namespace GraphDataStructure {

    public class Graph {
        private List<Node> _nodeList;
        private Dictionary<String, int> _index;

        public Graph() {
            this._nodeList = new List<Node>();
            this._index = new Dictionary<String, int>();
        }

        private void reIndex() {
            this._index = new Dictionary<String, int>();
            for ( var i = 0; i < _nodeList.Count; i++ ) {
                _index.Add( _nodeList[ i ].Address, i );
            }
        }

        public void removeNeighbors() {
            var edgesToRemove = new List<Edge>();
            foreach ( var node in this._nodeList ) {
                foreach ( var edge in node.Neighbors ) {
                    if ( !_index.ContainsKey( edge.Target.Address ) ) {
                        edgesToRemove.Add( edge );
                    }
                }
                foreach ( var remove in edgesToRemove ) {
                    node.Neighbors.Remove( remove );
                }
            }
        }

        public List<Node> NodeSet {
            get {
                return this._nodeList;
            }
        }

        public void addNode( String address, int degree ) {
            var node = new Node( address );
            this._nodeList.Add( node );
            node.Degree = degree;
            this._index.Add( address, this._nodeList.Count - 1 );
        }

        public void addDirectedEdge( String from, String to, Decimal value, UInt32 weight, int degree ) {
            this._nodeList.ElementAt( getNode( from ) ).addNeighbor( this._nodeList.ElementAt( getNode( to ) ), value, weight, degree );
        }

        public int getNode( String nodeAddress ) {
            return this._index[ nodeAddress ];
        }

        public void writeJSONToFile( String fileName, String data ) {
            using ( var file = new System.IO.StreamWriter( fileName, true ) ) {
                file.Write( data );
            }
        }

        public String buildNodeJsonString() {
            var jsonString = new StringBuilder( "\"nodes\": [ " );
            var firstNodeSet = true;
            foreach ( var node in this._nodeList ) {
                if ( firstNodeSet ) {
                    jsonString.Append( "{\"name\":\"" + node.Address + "\",\"group\":" + node.Degree + "}" );
                    firstNodeSet = false;
                } else {
                    jsonString.Append( ",{\"name\":\"" + node.Address + "\",\"group\":" + node.Degree + "}" );
                }
            }
            jsonString.Append( "]," );

            return jsonString.ToString();
        }

        public String buildLinkJsonString() {
            var jsonString = new StringBuilder( "\"links\": [" );
            var firstNode = true;

            for ( var i = 0; i < this._nodeList.Count; i++ ) {
                for ( var j = 0; j < this._nodeList.ElementAt( i ).Neighbors.Count; j++ ) {
                    if ( firstNode ) {
                        firstNode = false;
                        jsonString.Append( "{\"source\":" + i + ",\"target\":" + getNode( this._nodeList.ElementAt( i ).Neighbors.ElementAt( j ).Target.Address ) + ",\"value\":" + 1 + "}" );
                    } else {
                        jsonString.Append( ",{\"source\":" + i + ",\"target\":" + getNode( this._nodeList.ElementAt( i ).Neighbors.ElementAt( j ).Target.Address ) + ",\"value\":" + 1 + "}" );
                    }
                }
            }
            jsonString.Append( "]" );
            return jsonString.ToString();
        }

        public String buildJsonString() {
            var jsonString = new StringBuilder( "{" );

            jsonString.Append( buildNodeJsonString() );
            jsonString.Append( buildLinkJsonString() );
            jsonString.Append( "}" );

            return jsonString.ToString();
        }

        public static Graph Populate( String publicAddress, int degree, int tooBigToAddToNetwork ) {
            var database = new DBConnect();
            database.OpenConnection();
            database.SetMaxAddressQuerryTime( 500 );
            var graph = new Graph();
            var count = 0;

            var currentDegree = new Queue<String>();
            var nextDegree = new Queue<String>();

            graph.addNode( publicAddress, 0 );
            currentDegree.Enqueue( publicAddress );

            while ( count < degree ) {
                var currentAddress = currentDegree.Dequeue();
                Console.WriteLine( "Network has {0} nodes", graph._nodeList.Count );
                Console.Write( "{0} Deg: {1} Qrying ", currentAddress, count );
                if ( currentAddress.Substring( 0, 5 ) != "1dice" ) {
                    var sendersList = database.getSentTo( currentAddress );
                    var reciverList = database.getRecivedFrom( currentAddress );
                    if ( sendersList != null && reciverList != null ) {
                        Console.WriteLine( " Proc {0} trans", ( sendersList.Count + reciverList.Count ) );
                        if ( sendersList.Count < tooBigToAddToNetwork ) {
                            foreach ( var sender in sendersList ) {
                                if ( !graph._index.ContainsKey( sender.Target ) ) {
                                    nextDegree.Enqueue( sender.Target );
                                    graph.addNode( sender.Target, count + 1 );
                                    //graph.addDirectedEdge(sender.source, sender.target, Convert.ToDecimal(sender.value), sender.weight, count+1);
                                }
                                //else
                                {
                                    graph.addDirectedEdge( sender.Source, sender.Target, Convert.ToDecimal( sender.Value ), sender.Weight, count + 1 );
                                }
                            }

                            foreach ( var reciver in reciverList ) {
                                if ( !graph._index.ContainsKey( reciver.Source ) ) {
                                    nextDegree.Enqueue( reciver.Source );
                                    graph.addNode( reciver.Source, count + 1 );
                                    //graph.addDirectedEdge(reciver.source, reciver.target, Convert.ToDecimal(reciver.value), reciver.weight, count+1);
                                }
                                //else
                                {
                                    graph.addDirectedEdge( reciver.Source, reciver.Target, Convert.ToDecimal( reciver.Value ), reciver.Weight, count + 1 );
                                }
                            }
                        }
                    }
                }
                Console.WriteLine( currentDegree.Count + " queries to go" );
                if ( currentDegree.Count <= 0 ) {
                    count++;
                    currentDegree = nextDegree;
                    nextDegree = new Queue<String>();
                }
            }
            database.CloseConnection();
            return graph;
        }

        public void trim( int maxNumberOfNodes ) {
            var weightMinimum = 1;
            while ( this._nodeList.Count > maxNumberOfNodes ) {
                var nodesToDelete = new List<Node>();
                foreach ( var node in this._nodeList ) {
                    if ( ( node.Weight < weightMinimum ) || node.Neighbors.Count <= 0 ) {
                        nodesToDelete.Add( node );
                    }
                }
                foreach ( var nodeToDelete in nodesToDelete ) {
                    this._nodeList.Remove( nodeToDelete );
                }
                reIndex();
                removeNeighbors();
                weigh();
                weightMinimum++;
            }
        }

        public void weigh() {
            foreach ( var node in this._nodeList ) {
                foreach ( var neighbor in node.Neighbors ) {
                    this._nodeList.ElementAt( getNode( neighbor.Target.Address ) ).Weight = neighbor.Weight;
                }
            }
        }
    }
}