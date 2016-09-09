using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightLockTwo.WhatsApp
{

    public class Sender
    {

        public enum OriginType
        {
            Single,
            Group
        }

        public string Name { get; }
        
        public OriginType Origin { get; }

        public string ProfilePicture { get; }

        public Sender(string name, OriginType origin, string profilePicture)
        {
            this.Name = name;
            this.Origin = origin;
            this.ProfilePicture = profilePicture;
        }

    }


}
