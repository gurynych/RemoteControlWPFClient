namespace RemoteControlWPFClient.BusinessLayer.DTO
{
    public class UserDTO
    {
        public string Login { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public byte[] AuthToken { get; set; }

        public UserDTO()
        {            

        }

        public UserDTO(string email, string password) : this()
        {
            Email = email;
            Password = password;
        }

        public UserDTO(string login, string email, string password) : this(email, password)
        {
            Login = login;
            Email = email;
            Password = password;
        }

    }
}
