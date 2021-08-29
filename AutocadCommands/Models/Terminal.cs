namespace AutocadCommands.Models
{
    public class Terminal
    {
        /// <summary>
        /// Block Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DESC1
        /// </summary>
        public string Description1 { get; set; }

        /// <summary>
        /// DESC2
        /// </summary>
        public string Description2 { get; set; }

        /// <summary>
        /// DESC3
        /// </summary>
        public string Description3 { get; set; }

        /// <summary>
        /// TERM01
        /// </summary>
        public int TerminalNumber { get; set; }

        /// <summary>
        /// LINKTERM
        /// </summary>
        public string UniqId { get; set; }

        /// <summary>
        /// CATDESC
        /// </summary>
        public string CatalogDescription { get; set; }

        /// <summary>
        /// CAT
        /// </summary>
        public string Catalog { get; set; }

        /// <summary>
        /// MFG
        /// </summary>
        public string Manufacturing { get; set; }

        /// <summary>
        /// TAGSTRIP
        /// </summary>
        public string TagStrip { get; set; }

        /// <summary>
        /// CABLEDESIGNATION
        /// </summary>
        public string Cable { get; set; }

        /// <summary>
        /// Coordinate X
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Coordinate Y
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Coordinate Z
        /// </summary>
        public double Z { get; set; }
    }
}