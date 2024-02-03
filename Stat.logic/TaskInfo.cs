using System.Security.Principal;

namespace Stat.logic
{
    internal class TaskInfo
    {
        public string Name { get; set; }
        public string owner { get; set; }
        public System.DateTime Time { get; set; }
        public string FullPath { get; set; }

        public TaskInfo(string owner, System.DateTime time, string name = "", string fullPath = null)
        {
            this.owner = owner;
            this.Time = time;
            this.Name = name;
            FullPath = fullPath;
        }
    }
}
