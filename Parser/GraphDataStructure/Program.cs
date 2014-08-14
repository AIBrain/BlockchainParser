namespace GraphDataStructure {
    using System;

    internal class Program {

        private static void Main( String[] args ) {
            var degrees = 3;
            var network = Graph.Populate( "16MEiyzg9qaB1RWBhmcYd8bicVcEiTQJrE", degrees, 150 );
            network.weigh();
            network.trim( 100 );
            network.writeJSONToFile( @"C:\Users\wilso_000\Desktop\miserables.html", network.buildJsonString() );
        }
    }
}