using System;

namespace RegistratorWorker.Common
{
    public enum WorkerRole
    {
        Worker=1,
        Manager=2
    }
    public class ServerInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Uri IpAddress { get; set; }
        public WorkerRole Role { get; set; }
    }
}
