using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Helpers
{
    public abstract class Util
    {
        public static string GetHashString(byte[] hashed)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashed)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }
}
