using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;

namespace BirdBrainTest
{
    class TestServiceLocator : ServiceLocatorImplBase
    {
        private static Dictionary<Type, Dictionary<String, Object>> instances;

        public TestServiceLocator()
        {
            if (instances == null)
            {
                instances = new Dictionary<Type, Dictionary<String, Object>>();
            }
        }

        public void DoSetDefaultInstance(Type serviceType, Object instance)
        {
            if (!instances.ContainsKey(serviceType))
            {
                instances[serviceType] = new Dictionary<String, Object>();
            }
            instances[serviceType][""] = instance;
        }

        public void DoSetClearDefaultInstance(Type serviceType)
        {
            if (instances.ContainsKey(serviceType))
            {
                instances.Remove(serviceType);
            }
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (key == null)
            {
                key = "";
            }
            if (!instances.ContainsKey(serviceType) || !instances[serviceType].ContainsKey(key))
            {
                return null;
            }
            return instances[serviceType][key];
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return instances[serviceType].Values;
        }
    }
}
