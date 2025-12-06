namespace IdentityAuthorization.Data
{
    public class Contract: Auditable
    {
        #region Properties
        // Example property
        public string Number { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;

        #endregion
        #region Constructors
        private Contract() { }
        // Public constructor
        public Contract(string number, string address)
        {
            // add validation logic here
            Number = number;
            Address = address;
            CreatedAt = DateTime.UtcNow;

        }
        #endregion
        #region Methods
        public void Update(string number, string address)
        {
            // add validation logic here
            Number = number;
            Address = address;
            UpdatedAt = DateTime.UtcNow;
        }
        #endregion
    }
}
