using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdBrain
{
    public class Role
    {
        public string Name { get; private set; }

        public Role(string name)
        {
            Name = name;
        }
    }
}
