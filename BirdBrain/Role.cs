using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.UniqueConstraints;

namespace BirdBrain
{
    public class Role
    {
        [UniqueConstraint]
        public string Id { get; set; }
        public string Name { get; set; }

        public Role(string name)
        {
            Id = "roles/" + name;
            Name = name;
        }
    }
}
