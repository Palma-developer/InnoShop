namespace UserService.Domain.ValueObject
{
    public class Email
    {
        public string Value { get; set; }

        
        private Email() { }

        
        public Email(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value), "Email не может быть пустым");

            if (!value.Contains("@"))
                throw new ArgumentException("Email должен содержать @", nameof(value));

            Value = value;
        }

        public override string ToString() => Value;
    }
}
