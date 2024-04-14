
namespace TunicRandomizer
{
    public class ServerSettings
    {
        public string Port
        {
            get;
            set;
        }

        public bool Autoconnect
        {
            get;
            set;
        }

        public ServerSettings()
        {
            Port = "51111";
            Autoconnect = false;
        }
    }
}
