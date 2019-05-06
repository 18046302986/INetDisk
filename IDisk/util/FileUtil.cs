using CloudManager.entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FileUtil
{

    public static DownloadResult saveFileContent(Stream stream,String filename, String path)
    {
        DownloadResult result = new DownloadResult();
         
        string targetFile = path + @"\" + filename;
        try
        {
            using (var responseStream = stream)
            {
                using (var fs = new FileStream(targetFile, FileMode.Append))
                {
                    long beforeLength=fs.Length;
                    responseStream.CopyTo(fs);
                    if (fs.Length==beforeLength) {
                        result.state = 1;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception caught: {0}", e);
            result.state = 2;
        }
   
        return result;
    }
}

