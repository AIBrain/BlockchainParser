namespace GraphDataStructure {
    using System;

    public class Edge {
        private UInt32 _weight;
        private Decimal _value;
        private int _degree;

        public Edge( Node target, Decimal value, UInt32 weight ) {
            this.Target = target;
            this._value = value;
            this._weight = weight;
            this._degree = 0;
        }

        public Edge( Node target, Decimal value, UInt32 weight, int degree ) {
            this.Target = target;
            this._value = value;
            this._weight = weight;
            this._degree = degree;
        }

        public UInt32 Weight {
            get {
                return this._weight;
            }
            set {
                this._weight = value;
            }
        }

        public Node Target {
            get;
            private set;
        }

        public Decimal Value {
            get {
                return this._value;
            }
            set {
                this._value = value;
            }
        }

        public bool findEdge( Node nodeToFind ) {
            if ( nodeToFind.Address.Equals( this.Target.Address ) )
                return true;

            return false;
        }

        public void addValue( Decimal value ) {
            this._value += value;
        }

        public void updateWeight() {
            this._weight++;
        }

        public int Degree {
            set {
                this._degree = value;
            }
            get {
                return this._degree;
            }
        }
    }
}