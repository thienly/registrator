using System;
using System.Collections.Generic;
using System.Linq;
using Consul;

namespace RegistratorWorker.Collector
{
    public sealed class CollectorInternalData
    {
        private List<AgentServiceRegistration> _data = new List<AgentServiceRegistration>();
        private static CollectorInternalData _current;
        private CollectorInternalData()
        {
            
        }

        public static CollectorInternalData Current
        {
            get
            {
                if (_current == null)  
                    _current = new CollectorInternalData();
                return _current;
            }
        }

        public List<AgentServiceRegistration> Data => _data;

        public void Add(AgentServiceRegistration containerInfo)
        {
            _data.Add(containerInfo);
        }

        public void Remove(AgentServiceRegistration containerInfo)
        {
            _data.Remove(containerInfo);
        }
        public void Remove(string id)
        {
            var found = _data.FirstOrDefault(x=>x.ID == id);
            if (found != null)
                Remove(found);
        }
        public void AddRange(IEnumerable<AgentServiceRegistration> containerInfos)
        {
            _data.AddRange(containerInfos);
        }

        public void RemoveRange(IEnumerable<string> Ids)
        {
            foreach (var id in Ids)
            {
                var found = _data.FirstOrDefault(x=>x.ID == id);
                if (found != null)
                {
                    _data.Remove(found);
                }
            }
        }

        public void Update(AgentServiceRegistration containerInfo)
        {
            var index = _data.FindIndex(c=>c.ID == containerInfo.ID);
            if (index == -1)
                throw new ApplicationException($"The container with Id {containerInfo.ID} can not be found");
            _data.RemoveAt(index);
            _data.Insert(index,containerInfo);
        }

        public void Clear()
        {
            _data.Clear();
        }
    }
}
