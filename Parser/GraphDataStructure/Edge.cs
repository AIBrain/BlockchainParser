namespace GraphDataStructure {

    public class Edge {
        private uint _weight;
        private decimal _value;
        private int _degree;

        public Edge( Node target, decimal value, uint weight ) {
            this.Target = target;
            this._value = value;
            this._weight = weight;
            this._degree = 0;
        }

        public Edge( Node target, decimal value, uint weight, int degree ) {
            this.Target = target;
            this._value = value;
            this._weight = weight;
            this._degree = degree;
        }

        public uint Weight {
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

        public decimal Value {
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

        public void addValue( decimal value ) {
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