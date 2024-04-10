using Newtonsoft.Json;

namespace DataClasses
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Score { get; set; }
        public string JsonSerialized()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}