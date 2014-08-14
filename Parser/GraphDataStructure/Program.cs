namespace GraphDataStructure {

    internal class Program {

        private static void Main( string[] args ) {
            var degrees = 3;
            var network = Graph.populate( "16MEiyzg9qaB1RWBhmcYd8bicVcEiTQJrE", degrees, 150 );
            network.weigh();
            network.trim( 100 );
            network.writeJSONToFile( @"C:\Users\wilso_000\Desktop\miserables.html", network.buildJsonString() );
        }
    }
}