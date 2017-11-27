using System;
using System.Collections.Generic;
using System.Linq;
using RegistratorWorker.Common;

namespace RegistratorWorker.Collector
{
    public class CollectorInternalData
    {
        private List<ContainerInfo> _data = new List<ContainerInfo>();
        private static CollectorInternalData _current;
        private CollectorInternalData()
        {
            
        }

        public static CollectorInternalData Current
        {
            get
            {
                if (_current == null)
                    return new CollectorInternalData();
                return _current;
            }
        }

        public List<ContainerInfo> Data => _data;

        public void Add(ContainerInfo containerInfo)
        {
            _data.Add(containerInfo);
        }

        public void Remove(ContainerInfo containerInfo)
        {
            _data.Remove(containerInfo);
        }

        public void AddRange(IEnumerable<ContainerInfo> containerInfos)
        {
            _data.AddRange(containerInfos);
        }

        public void RemoveRange(IEnumerable<ContainerInfo> containerInfos)
        {
            foreach (var containerInfo in containerInfos)
            {
                _data.Remove(containerInfo);
            }
        }

        public void Update(ContainerInfo containerInfo)
        {
            var index = _data.FindIndex(c=>c.Id == containerInfo.Id);
            if (index == -1)
                throw new ApplicationException($"The container with Id {containerInfo.Id} can not be found");
            _data.RemoveAt(index);
            _data.Insert(index,containerInfo);
        }
    }
}
