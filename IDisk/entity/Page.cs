using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Page<T>
{
    public List<T> List;
    public int PageNum;
    public int PageSize;
    public int TotalSize;
}

