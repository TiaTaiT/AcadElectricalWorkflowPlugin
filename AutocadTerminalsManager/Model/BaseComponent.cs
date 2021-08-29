namespace AutocadTerminalsManager.Model
{
    public abstract class BaseComponent
    {
        /// <summary>
        /// Database Id (required)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ERP unique code (not required)
        /// </summary>
        public string ErpCode { get; set; }

        /// <summary>
        /// Designation in drawing (not required). This field must exclude from database
        /// </summary>
        public string Designation { get; set; }
    }
}
