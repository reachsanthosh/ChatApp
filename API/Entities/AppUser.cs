namespace API.Entities
{
    public class AppUser
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public byte[] PaasswordHash {get; set;}

        public byte[] PaasswordSalt {get; set;}
    }
}