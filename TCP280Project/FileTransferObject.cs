using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCP280Project;

public class FileTransferObject
{
    public byte[] FileBytes { get; set; }
    public string FileName { get; set; }
    public string JsonSerialized()
    {
        return JsonConvert.SerializeObject(this);
    }
}
