using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace covergen;

internal class FolderScanner
{
    public FolderScanner(DirectoryInfo di)
    {
        var files = di.EnumerateDirectories("*", SearchOption.AllDirectories);

    }

}
