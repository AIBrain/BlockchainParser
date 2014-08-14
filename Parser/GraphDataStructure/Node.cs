using System.Collections.Generic;
using System.Linq;

namespace GraphDataStructure {
    using System;

    public class Node {
        private List<Edge> _neighbors;
        // This is used to create a weighted graph

        public Node() {
            this.Address = null;
            this._neighbors = new List<Edge>();
            this.Weight = 1;
            //this._value = new List<Decimal>();
        }

        public Node( String data ) {
            this.Address = data;
            this._neighbors = new List<Edge>();
            this.Weight = 1;
            //this._value = new List<Decimal>();
        }

        public Node( String data, List<Edge> neighbors ) {
            this.Address = data;
            this._neighbors = neighbors;
            this.Weight = 1;
            //this._value = new List<Decimal>();
        }

        public string Address { get; set; }

        public int Degree { get; set; }

        public List<Edge> Neighbors {
            set {
                this._neighbors = value;
            }
            get {
                return this._neighbors;
            }
        }

        public uint Weight { get; set; }

        public void addNeighbor( Node neighbor, Decimal cost, UInt32 weight, int degree ) {
            var newEdge = new Edge( neighbor, cost, weight, degree );
            if ( neighborExists( newEdge ) ) {
                updateEdge( newEdge );
            } else {
                this._neighbors.Add( newEdge );
            }
        }

        public bool neighborExists( Edge edgeCheck ) {
            foreach ( var edge in this._neighbors ) {
                if ( edge.Target.Address.Equals( edgeCheck.Target.Address ) )
                    return true;
            }
            return false;
        }

        public void updateEdge( Edge edge ) {
            var index = getEdge( edge );

            this._neighbors.ElementAt( index ).addValue( edge.Value );
            this._neighbors.ElementAt( index ).updateWeight();
        }

        public void removeEdge( Node target ) {
            var index = getGraphNode( target );
            if ( index > -1 )
                this._neighbors.ElementAt( index );
            //TODO
        }

        public int getEdge( Edge edge ) {
            for ( var i = 0; i < this._neighbors.Count; i++ ) {
                if ( edge.Target.Address.Equals( this._neighbors.ElementAt( i ).Target.Address ) )
                    return i;
            }
            return -1;
        }

        public int getGraphNode( Node node ) {
            for ( var i = 0; i < this._neighbors.Count(); i++ ) {
                if ( this._neighbors.ElementAt( i ).Target.Address.Equals( node.Address ) ) {
                    return i;
                }
            }
            return -1;
        }
    }
}