// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.YoutrackUser.cs
// Last Modified: 12/24/2015 2:49 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

using System.Collections.Generic;
using System.Text;

namespace MMG.PlasticExtensions.YouTrackPlugin.Core.Models
{
    public class YoutrackUser
    {
        private readonly string _username;
        private readonly string _displayName;
        private readonly string _email;

        public YoutrackUser(string pUsername, string pDisplayName, string pEmail)
        {
            _username = pUsername;
            _displayName = pDisplayName;
            _email = pEmail;
        }

        public string Username
        {
            get { return _username; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public string Email
        {
            get { return _email; }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("{ Username = ");
            builder.Append(Username);
            builder.Append(", DisplayName = ");
            builder.Append(DisplayName);
            builder.Append(", Email = ");
            builder.Append(Email);
            builder.Append(" }");
            return builder.ToString();
        }

        public override bool Equals(object value)
        {
            var type = value as YoutrackUser;
            return (type != null) && EqualityComparer<string>.Default.Equals(type.Username, Username)
                   && EqualityComparer<string>.Default.Equals(type.DisplayName, DisplayName)
                   && EqualityComparer<string>.Default.Equals(type.Email, Email);
        }

        public override int GetHashCode()
        {
            int num = 0x7a2f0b42;
            num = (-1521134295*num) + EqualityComparer<string>.Default.GetHashCode(Username);
            num = (-1521134295*num) + EqualityComparer<string>.Default.GetHashCode(DisplayName);
            return (-1521134295*num) + EqualityComparer<string>.Default.GetHashCode(Email);
        }
    }
}