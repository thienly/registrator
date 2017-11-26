using System.Collections.Generic;
using EtonRegistratorShared;

namespace RegistratorWorker.Collector
{
    public class CollectorInternalData
    {
        private List<ContainerInfo> _data = new List<ContainerInfo>();
        private CollectorInternalData _current;
        private CollectorInternalData()
        {
            
        }

        public CollectorInternalData Current
        {
            get
            {
                if (_current == null)
                    return new CollectorInternalData();
                return _current;
            }
        }
        public void Add(ContainerInfo containerInfo)
        {
            _data.Add(containerInfo);
        }

        public void Remove(ContainerInfo containerInfo)
        {
            _data.Remove(containerInfo);
        }
    }
}
