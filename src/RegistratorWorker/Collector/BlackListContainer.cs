using System;
using System.Collections.Generic;
using System.Linq;
using RegistratorWorker.Common;

namespace RegistratorWorker.Collector
{
    public class BlackListContainer
    {
        private List<ContainerInfo> _data = new List<ContainerInfo>();
        private static BlackListContainer _current;
        private BlackListContainer()
        {

        }

        public static BlackListContainer Current
        {
            get
            {
                if (_current == null)
                    _current = new BlackListContainer();
                return _current;
            }
        }

        public List<ContainerInfo> Data => _data;

        public bool IsInBlackList(ContainerInfo info)
        {
            return _data.Any(c => c.Id == info.Id);
        }
        public void Add(ContainerInfo containerInfo)
        {
            _data.Add(containerInfo);
        }

        public void Remove(ContainerInfo containerInfo)
        {
            _data.Remove(containerInfo);
        }
        public void Remove(string id)
        {
            var found = _data.FirstOrDefault(x => x.Id == id);
            if (found != null)
                Remove(found);
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
            var index = _data.FindIndex(c => c.Id == containerInfo.Id);
            if (index == -1)
                throw new ApplicationException($"The container with Id {containerInfo.Id} can not be found");
            _data.RemoveAt(index);
            _data.Insert(index, containerInfo);
        }
    }
}