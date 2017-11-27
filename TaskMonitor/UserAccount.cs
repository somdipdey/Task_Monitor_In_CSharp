using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMonitor
{
    // Summary:
    // This class holds the structure to fetch information regarding User accounts on the system
    public class UserAccount
    {
        public Int32 AccountType;
        public string Caption;
        public string Description;
        public bool Disabled;
        public string Domain;
        public string FullName;
        public DateTime InstallDate;
        public bool LocalAccount;
        public bool Lockout;
        public string Name;
        public bool PasswordChangeable;
        public bool PasswordExpires;
        public bool PasswordRequired;
        public string SID;
        public byte SIDType;
        public string Status;
    }
}
